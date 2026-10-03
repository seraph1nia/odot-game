using System.Text.Json;

namespace DevRunner;

internal sealed partial class Runner
{
    private static bool SameCamera(CameraObservation a, CameraObservation b) => Math.Abs(a.Zoom - b.Zoom) < 0.001
        && Math.Abs(a.PanX - b.PanX) < 0.001 && Math.Abs(a.PanZ - b.PanZ) < 0.001
        && Math.Abs(a.Size - b.Size) < 0.001 && Math.Abs(a.Height - b.Height) < 0.001 && a.Rotation.SequenceEqual(b.Rotation);

    private static void RequireOverview(UiObservation frame)
    {
        Require(Math.Abs(frame.Camera.Zoom - 1) < 0.001 && Math.Abs(frame.Camera.PanX) < 0.001 && Math.Abs(frame.Camera.PanZ) < 0.001
            && Math.Abs(frame.Camera.BaseSize - frame.Camera.Size) < 0.001, "reset fitted overview");
        Require(Enumerable.Range(0, 9).All(i => frame.Targets["Plot" + i].Visible), "all nine plots visible at overview");
        TrioPresentation(frame);
        Countryside(frame);
    }
    private static Task Wheel(Child client, float x, float y, bool inward)
        => client.Send(FormattableString.Invariant($"wheel {x} {y} {(inward ? "up" : "down")}"));

    private async Task FocusWorld(Child client, CancellationToken token)
    {
        UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token);
        if (!frame.WindowFocused) NativeWindowClose.Focus(client, options, frame.NativeWindow);
        await WaitUi(client, p => p.WindowFocused, "owned world input window focus", token);
        await client.Send("click 32 70");
        await WaitUi(client, p => p.WindowFocused && p.FocusedControl.Length == 0, "world releases HUD navigation focus", token);
    }
    private async Task<UiObservation> CameraZoom(Child client, CancellationToken token)
    {
        await FocusWorld(client, token);
        UiObservation before = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(before.Camera.ReferenceY > 0 && before.Camera.ReferenceY < before.HudTop, "off-center reference in world area");
        await Wheel(client, before.Camera.ReferenceX, before.Camera.ReferenceY, true);
        UiObservation after = await WaitUi(client, p => p.Camera.Zoom > before.Camera.Zoom, "wheel zoom through Godot input", token);
        Require(after.Camera.Size < before.Camera.Size
            && Math.Abs(after.Camera.ReferenceX - before.Camera.ReferenceX) < 1 && Math.Abs(after.Camera.ReferenceY - before.Camera.ReferenceY) < 1,
            "off-center ground point stays beneath cursor");
        Require(after.Camera.Height == before.Camera.Height && after.Camera.Rotation.SequenceEqual(before.Camera.Rotation), "zoom retains height and orthographic angle");
        Require(after.SelectedSlot == before.SelectedSlot, "zoom preserves world selection");
        return after;
    }
    private Task<UiObservation> HoldPan(Child client, string[] keys, CancellationToken token) => PanKeys(client, keys, true, token);
    private async Task<UiObservation> PanKeys(Child client, string[] keys, bool focusWorld, CancellationToken token)
    {
        if (focusWorld) await FocusWorld(client, token);
        UiObservation before = await UiProtocol.Probe(client, options.StartupTimeout, token);
        try
        {
            foreach (string key in keys) await client.Send("key-down " + key);
            await WaitUi(client, p => p.Camera.MovementSeconds >= before.Camera.MovementSeconds + 0.12, "continuous held pan", token);
        }
        finally { foreach (string key in keys) await client.Send("key-up " + key); }
        UiObservation after = await UiProtocol.Probe(client, options.StartupTimeout, token);
        double elapsed = after.Camera.MovementSeconds - before.Camera.MovementSeconds;
        var expected = CameraPanExpectation.Calculate(before.Camera, elapsed, before.HudTop / before.Height, keys);
        Require(elapsed > 0 && Math.Abs(after.Camera.PanX - expected.X) < .025 && Math.Abs(after.Camera.PanZ - expected.Z) < .025,
            "cardinal/diagonal speed follows actual frame delta and view size, with travel bounds");
        Require(after.Camera.Height == before.Camera.Height && after.Camera.Rotation.SequenceEqual(before.Camera.Rotation) && after.SelectedSlot == before.SelectedSlot, "pan preserves angle, height and selection");
        UiObservation stopped = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(SameCamera(after.Camera, stopped.Camera) && stopped.Camera.DirectionX == 0 && stopped.Camera.DirectionY == 0, "key release stops panning");
        return after;
    }
    private async Task CameraControls(Child client, bool thorough, CancellationToken token)
    {
        RequireOverview(await UiProtocol.Probe(client, options.StartupTimeout, token));
        string before = Gameplay(Latest(client));
        int acknowledgements = client.History().Count(e => e.Type == "ack");
        await CameraZoom(client, token);
        await CameraDrag(client, token);
        await HoldPan(client, ["D"], token);
        await Pick(client, 4, token);
        await Checkpoint(client, thorough ? "camera-navigation" : "packed-camera-navigation", token);
        await Click(client, "ResetView", token);
        RequireOverview(await UiProtocol.Probe(client, options.StartupTimeout, token));
        if (thorough)
        {
            UiObservation arrow = await HoldPan(client, ["Right"], token);
            Require(arrow.Camera.PanX > 0 && arrow.Camera.PanZ < 0, "right arrow pans in screen-right ground direction");
            await Click(client, "ResetView", token);
            UiObservation diagonal = await HoldPan(client, ["W", "D"], token);
            Require(diagonal.Camera.PanX != 0 && diagonal.Camera.PanZ < 0, "W plus D combines ground directions");
            await Click(client, "ResetView", token);
            UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token);
            for (int i = 0; i < 20; i++) await Wheel(client, frame.Camera.ReferenceX, frame.Camera.ReferenceY, true);
            UiObservation close = await WaitUi(client, p => Math.Abs(p.Camera.Zoom - 3) < 0.001, "close zoom limit", token);
            Countryside(close);
            await Wheel(client, frame.Camera.ReferenceX, frame.Camera.ReferenceY, true);
            Require(SameCamera(close.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "close-limit wheel cannot shift camera");
            for (int i = 0; i < 20; i++) await Wheel(client, frame.Camera.ReferenceX, frame.Camera.ReferenceY, false);
            UiObservation wide = await WaitUi(client, p => Math.Abs(p.Camera.Zoom - 1) < 0.001, "overview zoom limit", token);
            await Wheel(client, frame.Camera.ReferenceX, frame.Camera.ReferenceY, false);
            Require(SameCamera(wide.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "overview-limit wheel cannot shift camera");
            await Click(client, "ResetView", token); await FocusWorld(client, token);
            try
            {
                await client.Send("key-down D");
                UiObservation edge = await WaitUi(client, p => Math.Abs(p.Camera.PanX - 6) < 0.001 && Math.Abs(p.Camera.PanZ + 8) < 0.001, "bounded city travel", token);
                Countryside(edge);
                Require(SameCamera(edge.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "held pan stops at travel edge");
            }
            finally { await client.Send("key-up D"); }
            await Click(client, "ResetView", token);
            UiObservation overview = await UiProtocol.Probe(client, options.StartupTimeout, token);
            string city = "City" + overview.CityIds.First(id => id != client.PlayerId);
            await CameraZoom(client, token);
            await Click(client, city, token);
            RequireOverview(await UiProtocol.Probe(client, options.StartupTimeout, token));
            await Click(client, "City" + client.PlayerId, token);
            UiObservation adjusted = await CameraZoom(client, token);
            await Click(client, "Settings", token);
            UiObservation resized = await ResizeTo1280(client, token);
            Require(Math.Abs(adjusted.Camera.Zoom - resized.Camera.Zoom) < 0.001 && Math.Abs(adjusted.Camera.PanX - resized.Camera.PanX) < 0.001
                && Math.Abs(adjusted.Camera.PanZ - resized.Camera.PanZ) < 0.001, "resize retains camera adjustments against new fit");
            await Click(client, "CloseSettings", token);
            await Checkpoint(client, "camera-resized", token);
            await Click(client, "ResetView", token);
        }
        RequireOverview(await UiProtocol.Probe(client, options.StartupTimeout, token));
        Require(Gameplay(Latest(client)) == before && client.History().Count(e => e.Type == "ack") == acknowledgements, "camera controls submit no gameplay commands or changes");
    }
    private async Task CameraDrag(Child client, CancellationToken token)
    {
        await FocusWorld(client, token);
        UiObservation before = await UiProtocol.Probe(client, options.StartupTimeout, token);
        float x = before.Camera.ReferenceX, y = before.Camera.ReferenceY;
        await client.Send(FormattableString.Invariant($"mouse-down {x} {y}"));
        await client.Send(FormattableString.Invariant($"mouse-move {x + 55} {y + 30}"));
        UiObservation dragged = await WaitUi(client, p => !SameCamera(before.Camera, p.Camera), "left drag pans camera", token);
        await client.Send(FormattableString.Invariant($"mouse-up {x + 55} {y + 30}"));
        Require(dragged.SelectedSlot == before.SelectedSlot && dragged.Camera.Zoom == before.Camera.Zoom && dragged.Camera.Rotation.SequenceEqual(before.Camera.Rotation), "drag preserves selection, zoom and angle");
        await client.Send(FormattableString.Invariant($"mouse-down {x} {y}"));
        await client.Send(FormattableString.Invariant($"mouse-move {-10000} {-10000}"));
        await client.Send("mouse-up -10000 -10000");
        UiObservation edge = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(Math.Abs(edge.Camera.PanX) <= 6 && Math.Abs(edge.Camera.PanZ) <= 8 && edge.SelectedSlot == before.SelectedSlot, "drag travel bounded and outside release suppresses selection");
        await Click(client, "ResetView", token);
        UiObservation reset = await UiProtocol.Probe(client, options.StartupTimeout, token);
        await client.Send(FormattableString.Invariant($"mouse-down {reset.Camera.ReferenceX} {reset.Camera.ReferenceY}"));
        await client.Send("key Escape");
        await WaitUi(client, p => p.SettingsOpen, "modal cancels pending drag", token);
        await client.Send("mouse-up 0 0");
        await Click(client, "CloseSettings", token);
        await client.Send("mouse-move 400 350");
        RequireOverview(await UiProtocol.Probe(client, options.StartupTimeout, token));
        foreach (string target in new[] { "ResourceTable", "UpkeepTable" })
        {
            UiTarget table = reset.Targets[target];
            await Wheel(client, table.X, table.Y, true);
            await client.Send(FormattableString.Invariant($"mouse-down {table.X} {table.Y}"));
            await client.Send(FormattableString.Invariant($"mouse-move {table.X - 70} {table.Y + 40}"));
            await client.Send(FormattableString.Invariant($"mouse-up {table.X - 70} {table.Y + 40}"));
            RequireOverview(await UiProtocol.Probe(client, options.StartupTimeout, token));
        }
        await Pick(client, 4, token);
    }
    private async Task CameraInputPriority(Child client, CancellationToken token)
    {
        UiObservation view = await CameraZoom(client, token);
        await Wheel(client, view.Width / 2f, view.Height - 15, true);
        Require(SameCamera(view.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "HUD wheel does not zoom world");
        try
        {
            await client.Send("key-down D");
            await WaitUi(client, p => p.Camera.MovementSeconds > view.Camera.MovementSeconds, "held movement before modal", token);
            await client.Send("key Escape");
            UiObservation modal = await WaitUi(client, p => p.SettingsOpen, "modal interrupts held pan", token);
            await Wheel(client, view.Camera.ReferenceX, view.Camera.ReferenceY, true);
            await client.Send("key-down W"); await client.Send("key-up W");
            await client.Send("key Space");
            UiObservation modalSpace = await UiProtocol.Probe(client, options.StartupTimeout, token);
            Require(SameCamera(modal.Camera, modalSpace.Camera), "modal suppresses wheel, held directions and Space reset");
            if (modalSpace.SettingsOpen) await Click(client, "CloseSettings", token);
            await FocusWorld(client, token);
            UiObservation returned = await UiProtocol.Probe(client, options.StartupTimeout, token);
            Require(SameCamera(modal.Camera, returned.Camera) && returned.Camera.DirectionX == 0, "closing modal cannot restart interrupted key");
        }
        finally { await client.Send("key-up D"); }
        await Click(client, "Settings", token);
        await Click(client, "CloseSettings", token);
        UiObservation focused = await UiProtocol.Probe(client, options.StartupTimeout, token);
        await client.Send("key Space");
        UiObservation consumed = await WaitUi(client, p => p.SettingsOpen, "focused Settings consumes Space", token);
        Require(SameCamera(focused.Camera, consumed.Camera), "consumed Space cannot reset world");
        await Click(client, "CloseSettings", token);
        await WaitUi(client, p => p.FocusedControl.Length != 0, "HUD focus restored", token);
        UiObservation wasd = await PanKeys(client, ["W"], false, token);
        await client.Send("key Right");
        Require(SameCamera(wasd.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "HUD consumes arrows while unhandled WASD remains available");
        await HoldPan(client, ["Right"], token);
        await FocusWorld(client, token);
        try
        {
            await client.Send("key-down A");
            UiObservation moving = await WaitUi(client, p => p.Camera.DirectionX < 0, "movement before window focus loss", token);
            using var focusRecipient = NativeWindowClose.Defocus(client, options, moving.NativeWindow);
            UiObservation away = await WaitUi(client, p => !p.WindowFocused, "owned window loses focus", token);
            Require(SameCamera(away.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "unfocused window stops camera");
            NativeWindowClose.Focus(client, options, moving.NativeWindow);
            UiObservation returned = await WaitUi(client, p => p.WindowFocused, "owned window focus restored", token);
            Require(SameCamera(away.Camera, returned.Camera) && returned.Camera.DirectionX == 0, "refocusing cannot resume held key");
            Require(moving.Camera.Zoom == returned.Camera.Zoom, "window focus preserves zoom");
        }
        finally { await client.Send("key-up A"); NativeWindowClose.Focus(client, options, view.NativeWindow); }
        await Checkpoint(client, "camera-input-priority", token);
    }
    private async Task CameraFrozenBars(Child client, CancellationToken token)
    {
        UiObservation before = await UiProtocol.Probe(client, options.StartupTimeout, token);
        await CameraZoom(client, token);
        await HoldPan(client, ["D"], token);
        UiObservation after = await UiProtocol.Probe(client, options.StartupTimeout, token);
        HealthBars(after);
        Require(before.VisualSeconds == after.VisualSeconds && JsonSerializer.Serialize(before.Units) == JsonSerializer.Serialize(after.Units), "local camera keeps paused world poses and health frozen");
        Require(after.HealthBars.Any(b => b.Visible && before.HealthBars.Any(old => old.Id == b.Id && old.Visible && (Math.Abs(old.X - b.X) > 1 || Math.Abs(old.Y - b.Y) > 1))), "paused overhead bars follow camera");
        Require(after.HealthBars.All(b => before.HealthBars.Single(old => old.Id == b.Id).Fraction == b.Fraction), "paused camera preserves health fractions");
        await Checkpoint(client, "combat-camera-paused", token);
        await Click(client, "ResetView", token);
    }
}
