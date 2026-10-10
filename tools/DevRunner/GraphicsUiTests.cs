using System.Globalization;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task ScrollGraphicsControl(Child client, string name, CancellationToken token)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token);
            if (frame.Targets.TryGetValue(name, out UiTarget? target) && target.Visible)
            {
                Require(target.Enabled, "supported graphics control enabled: " + name);
                return;
            }
            UiTarget scroll = UiProtocol.Target(frame, "GraphicsScroll");
            bool upward = target is not null && target.Y < scroll.Y;
            for (int tick = 0; tick < 5; tick++) await client.Send(string.Create(CultureInfo.InvariantCulture, $"wheel {scroll.X} {scroll.Y} {(upward ? "up" : "down")}"));
        }
        throw new InvalidOperationException("Graphics control not reachable through real owned scrolling: " + name);
    }
    private async Task GraphicsOptionInput(Child client, string key, int index, CancellationToken token)
    {
        string name = "Graphics_" + key;
        await ScrollGraphicsControl(client, name, token);
        await Click(client, name, token);
        // Native PopupMenu receives focus on a subsequent frame. Sending keys in
        // the click's dispatch batch reaches the parent instead of its choices.
        UiObservation popup = await WaitUi(client, p => p.GraphicsDropdownOpen, "actual graphics popup owns input before keyboard choice", token);
        Require(popup.GraphicsPopupFocus[key] == -1, "pointer-opened native graphics menu has no focused item, matching existing resolution input path");
        // Godot PopupMenu does not focus its first item with Home. The first Down
        // focuses item zero, exactly as the proven Resolution helper does.
        for (int step = 0; step <= index; step++) await client.Send("key Down");
        await WaitUi(client, p => p.GraphicsDropdownOpen && p.GraphicsPopupFocus[key] == index, "requested native graphics menu item really focused before Enter", token);
        await client.Send("key Enter");
        await WaitUi(client, p => !p.GraphicsDropdownOpen && p.GraphicsDraft[key] == index, "native graphics choice changes the requested draft without applying", token);
    }
    private string OwnedSettingsPath(UiObservation owned)
    {
        string path = Path.GetFullPath(Path.Combine(owned.UserDataPath, "settings.cfg"));
        string relative = Path.GetRelativePath(_scope!.Directory, path);
        Require(!relative.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relative), "settings file remains within this client's owned data");
        return path;
    }
    private void RequireSavedWindowSize(UiObservation owned, int width, int height)
    {
        Dictionary<string, string> graphics = File.ReadLines(OwnedSettingsPath(owned))
            .SkipWhile(line => line.Trim() != "[graphics]").Skip(1)
            .TakeWhile(line => !line.TrimStart().StartsWith('['))
            .Where(line => line.Contains('='))
            .Select(line => line.Split('=', 2))
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
        Require(graphics["mode"] == "\"windowed\""
            && int.Parse(graphics["window_width"], CultureInfo.InvariantCulture) == width
            && int.Parse(graphics["window_height"], CultureInfo.InvariantCulture) == height,
            "owned settings.cfg persists the committed window mode and both size dimensions");
    }
    private void InvalidGraphicsFixture(UiObservation owned)
    {
        string path = OwnedSettingsPath(owned);
        string text = File.ReadAllText(path);
        var aa = new System.Text.RegularExpressions.Regex(@"^aa=.*$", System.Text.RegularExpressions.RegexOptions.Multiline);
        var exposure = new System.Text.RegularExpressions.Regex(@"^exposure=.*$", System.Text.RegularExpressions.RegexOptions.Multiline);
        Require(aa.Count(text) == 1 && exposure.Count(text) == 1, "owned saved graphics fixture has exactly one generated AA/exposure entry");
        File.WriteAllText(path, exposure.Replace(aa.Replace(text, "aa=\"invalid\""), "exposure=999"));
    }
    private async Task GraphicsSettingsInput(Child client, CancellationToken token)
    {
        UiObservation initial = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(initial.Graphics.Method == "gl_compatibility" && initial.Graphics.DesiredRenderer == "forward_plus", "CI launch override is actual Compatibility, not a saved desired renderer change");
        Require(initial.Graphics.FpsLimit == 30 && initial.Graphics.Settings["fps"] == 1 && initial.Graphics.Vsync == "Disabled" && initial.Graphics.Settings["vsync"] == 1,
            "original FPS and VSync launch overrides survive initial settings application without becoming preferences");
        await Click(client, "CloseSettings", token);
        await WaitUi(client, p => !p.SettingsOpen && p.Width == 1280 && p.Height == 720, "Apply & close commits the live resolution preview before hiding", token);
        await Click(client, "Settings", token);
        await WaitUi(client, p => p.SettingsOpen, "reopen the committed display baseline", token);
        await GraphicsOptionInput(client, "quality", 0, token);
        UiObservation draft = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(draft.Graphics.Settings["quality"] == initial.Graphics.Settings["quality"], "unapplied graphics controls do not mutate current rendering or preferences");
        await Click(client, "CancelGraphics", token);
        Require((await UiProtocol.Probe(client, options.StartupTimeout, token)).Graphics.Settings["quality"] == initial.Graphics.Settings["quality"], "Cancel preserves committed graphics");
        await GraphicsOptionInput(client, "quality", 0, token);
        await Click(client, "CloseSettings", token);
        UiObservation low = await WaitUi(client, p => !p.SettingsOpen && p.Width == 1280 && p.Height == 720 && p.Graphics.Settings["quality"] == 0 && Math.Abs(p.Graphics.RenderScale - .85f) < .001f,
            "Apply & close commits Low graphics and preserves the committed resolution before hiding", token);
        foreach (string cancellation in new[] { "Escape", "window-close" })
        {
            await Click(client, "Settings", token);
            await WaitUi(client, p => p.SettingsOpen && p.GraphicsDraft["quality"] == 0, "reopen retains applied Low baseline", token);
            await GraphicsOptionInput(client, "quality", 2, token);
            NativeWindowClose.Resize(client, options, low.NativeWindow, 1100, 820);
            await WaitUi(client, p => p.SettingsOpen && p.Width == 1100 && p.Height == 820, "owned display resize remains an uncommitted preview", token);
            if (cancellation == "Escape") await client.Send("key Escape");
            else await Click(client, "CancelSettings", token);
            await WaitUi(client, p => !p.SettingsOpen && p.Width == 1280 && p.Height == 720 && p.Graphics.Settings["quality"] == 0,
                cancellation + " cancels the graphics draft and restores the committed display preview", token);
            RequireSavedWindowSize(low, 1280, 720);
        }
        await Click(client, "Settings", token);
        await WaitUi(client, p => p.SettingsOpen && p.GraphicsDraft["quality"] == 0, "cancelled drafts do not survive reopening", token);
        Require(!low.Graphics.VolumetricFogEnabled && !low.Graphics.SsilEnabled && !low.Graphics.SsaoEnabled && !low.Graphics.Taa && low.Graphics.Shadows && low.Graphics.Effective["aa"] == 0 && low.Graphics.Fxaa == "Disabled" && low.Graphics.Msaa == "Disabled", "Low produces simple supported rendering with unsupported Compatibility FXAA honestly Off");
        await GraphicsOptionInput(client, "fps", 2, token); await Click(client, "ApplyGraphics", token);
        UiObservation custom = await WaitUi(client, p => p.Graphics.Settings["quality"] == 4, "individual edit selects Custom", token);
        Require(custom.Graphics.Settings["scale"] == low.Graphics.Settings["scale"] && custom.Graphics.Settings["shadows"] == low.Graphics.Settings["shadows"], "Custom keeps other current settings");
        await GraphicsOptionInput(client, "quality", 2, token); await Click(client, "ApplyGraphics", token);
        UiObservation high = await WaitUi(client, p => p.Graphics.Settings["quality"] == 2 && p.Graphics.Msaa == "Msaa4X" && p.Graphics.SsaoEnabled, "requested High retains supported Compatibility MSAA/shadows/AO rather than silently becoming Medium", token);
        Require(high.Graphics.Shadows && high.Graphics.Settings["shadows"] == 3 && !high.Graphics.VolumetricFogEnabled && !high.Graphics.SsilEnabled, "High preserves native supported cost choices and capability fallback");
        await GraphicsOptionInput(client, "quality", 1, token); await Click(client, "ApplyGraphics", token);
        await GraphicsOptionInput(client, "weather", 1, token); await Click(client, "ApplyGraphics", token);
        UiObservation overcast = await WaitUi(client, p => p.Graphics.Settings["weather"] == 1 && p.Graphics.SunEnergy < .52f && p.Graphics.SunEnergy > .3f, "manual overcast blends real lighting instead of snapping or remaining Clear", token);
        Require(overcast.Graphics.SkyUploads > initial.Graphics.SkyUploads, "changed transition colors reach native sky material");
        Require(overcast.Graphics.Settings["aa"] == 3 && overcast.Graphics.Effective["aa"] == 0 && overcast.Graphics.Fxaa == "Disabled" && overcast.Graphics.Msaa == "Disabled" && !overcast.Graphics.Taa,
            "Medium preserves desired MSAA 2x while actual Compatibility default-cost fallback is honestly Off, never unsupported screen-space AA");
        await GraphicsOptionInput(client, "weather", 3, token);
        await GraphicsOptionInput(client, "reduce_motion", 1, token);
        await GraphicsOptionInput(client, "reduce_weather", 1, token);
        await GraphicsOptionInput(client, "reduce_effects", 1, token);
        await Click(client, "ApplyGraphics", token);
        UiObservation comfort = await WaitUi(client, p => p.Graphics.Rain && p.Graphics.Wind == 0 && !p.Graphics.Leaves && !p.Graphics.GlowEnabled, "actual rain, motion and reduced effects accessibility policy", token);
        Require(comfort.Graphics.RainRatio <= .088f && !comfort.Graphics.VolumetricFogEnabled && comfort.Graphics.Settings["volumetric"] == 1,
            "weather reduction is bounded and unsupported volumetric preference retained without claiming it active");
        Require(comfort.Graphics.FpsLimit == 30 && comfort.Graphics.Settings["fps"] == 2 && comfort.Graphics.Vsync == "Disabled" && comfort.Graphics.Settings["vsync"] == 1,
            "original FPS and VSync overrides survive repeated Apply while player preferences remain independent");
        await client.Send("key Tab");
        await WaitUi(client, p => p.GraphicsFocus == "CancelGraphics", "native Tab traverses Apply/Cancel focus without moving the camera", token);
        await ScrollGraphicsControl(client, "ResetGraphics", token); await Click(client, "ResetGraphics", token);
        UiObservation reset = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(reset.GraphicsDraft["quality"] == 1 && reset.GraphicsDraft["reduce_motion"] == 0 && reset.Graphics.Settings["reduce_motion"] == 1, "Reset restores complete safe defaults as a cancellable draft");
        await Click(client, "CancelGraphics", token);
        Require((await UiProtocol.Probe(client, options.StartupTimeout, token)).GraphicsDraft["reduce_motion"] == 1, "Cancel reset retains comfort preferences");
        Require(comfort.SettingsOpen && comfort.Connected, "graphics changes neither close the settings session nor restart live play");
    }
}
