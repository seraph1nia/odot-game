using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B strengthens the existing receiving component. Core cannot prove
    // actual native Buttons/session bridge or modal closure neutrality. This is
    // controlled numerical node proof, not rendering or real transport.
    private async Task RunProgression()
    {
        using Theme theme = ApplicationTheme.Create();
        var host = new Control { Theme = theme }; AddChild(host); host.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        using var authority = new Match(combatSeed: 1); authority.Join();
        var session = new ReceivingSession(authority); var hud = new Tabletop(session);
        try
        {
            hud.InitializeHud(host, dialog => host.AddChild(dialog), () => false, () => { });
            hud.RefreshHud(1, -1);
            var open = host.FindChildren("RunProgression", "Button", true, false).OfType<Button>().Single();
            open.EmitSignal(BaseButton.SignalName.Pressed);
            var dialog = host.GetNode<RunDialog>("RunDialog");
            Check(dialog.Visible && dialog.Choices.Count == 6, "native lobby has all six doctrine cards");
            dialog.Choices[1].EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Commands.Last().Action == "doctrine:merchant" && session.Results.Last().Accepted, "actual doctrine button creates authoritative owned command");
            Check(session.State.Players[0].Run.Doctrine == "warden" && authority.Players[1].Run.Doctrine == "merchant", "native selection is accepted-only, not optimistic");
            session.Acknowledge(); hud.RefreshHud(1, -1);
            Check(dialog.Choices[1].Text == "Selected", "accepted snapshot projects selected crest/card");
            dialog.Hide(); session.SendAction("start"); session.Acknowledge(); hud.RefreshHud(1, -1);
            for (int stage = 0; stage < 3; stage++) { session.SendAction("ready"); session.Acknowledge(); }
            foreach (UnitState enemy in authority.Enemies.ToArray()) authority.Combat.Remove(enemy.Id);
            authority.Step(); session.Acknowledge(); hud.RefreshHud(1, -1);
            Check(dialog.Visible && dialog.Choices.Count == 3, "real successful-clear state opens three native reward choices");
            Check(dialog.Content.FindChildren("*", "Label", true, false).OfType<Label>().Any(l => l.Text.Contains("Next wave 2", StringComparison.Ordinal)), "native draft displays canonical next forecast");
            int count = session.Commands.Count; dialog.Hide();
            Check(session.Commands.Count == count && authority.Phase == Phase.RewardDraft, "closing cannot choose or advance draft");
            open.EmitSignal(BaseButton.SignalName.Pressed);
            string offered = authority.Players[1].Run.Offers[0]; dialog.Choices[0].EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Commands.Last().Action == "reward:" + offered && session.Results.Last().Accepted, "real native reward selection uses normal command bridge");
            Check(authority.Players[1].Run.Stacks(offered) == 1 && authority.ProductionCount == 3, "reward applies once without a fourth economy window");
            session.Acknowledge(); hud.RefreshHud(1, -1); Check(!dialog.Visible, "accepted resolved snapshot dismisses draft");
            open.EmitSignal(BaseButton.SignalName.Pressed);
            Check(dialog.Choices.All(b => b.Disabled), "doctrine inspection is locked after start");
        }
        finally { RemoveChild(host); host.Free(); hud.Free(); }
        await Frames();
    }
}
