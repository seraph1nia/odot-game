namespace DevRunner;

internal sealed record InspectorObservation(int Id, string Name, string Description, int Level, int Health, int MaximumHealth, int Damage, int Size, bool IsBoss, string Preview)
{
    public string StatsText { get; init; } = "";
    public double Fraction { get; init; }
}
internal sealed record HomeHealthObservation
{
    public int Current { get; init; }
    public int Maximum { get; init; }
    public bool PercentageInside { get; init; }
    public int Percent { get; init; }
    public bool Visible { get; init; }
    public bool InputIgnored { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
}
internal sealed partial class Runner
{
    private async Task<UiObservation> OpenUnitInspector(Child client, CancellationToken token)
    {
        UiObservation frame = await WaitUi(client, p => p.Targets.Any(pair => pair.Key.StartsWith("Unit", StringComparison.Ordinal) && pair.Key != "UnitInspector" && pair.Value.Visible), "inspectable living unit", token);
        UiTarget target = frame.Targets.First(pair => pair.Key.StartsWith("Unit", StringComparison.Ordinal) && pair.Key != "UnitInspector" && pair.Value.Visible).Value;
        await ClickPoint(client, target);
        return await WaitUi(client, p => p.InspectedUnit is not null, "unit inspector opens", token);
    }
    private async Task UnitInspection(Child client, string frameName, CancellationToken token)
    {
        UiObservation before = await WaitUi(client, p => p.Targets.Any(pair => pair.Key.StartsWith("Unit", StringComparison.Ordinal) && pair.Key != "UnitInspector" && pair.Value.Visible), "living unit is clickable", token);
        long acknowledgements = AckSequence(client);
        UiTarget target = before.Targets.First(pair => pair.Key.StartsWith("Unit", StringComparison.Ordinal) && pair.Key != "UnitInspector" && pair.Value.Visible).Value;
        await ClickPoint(client, target);
        UiObservation opened = await WaitUi(client, p => p.InspectedUnit is not null, "short unit click opens inspector", token);
        InspectorObservation inspected = opened.InspectedUnit!;
        Game.Core.MatchSnapshot snapshot = Latest(client);
        Game.Core.UnitState unit = snapshot.Players.SelectMany(city => city.Soldiers).Concat(snapshot.Enemies).Single(unit => unit.Id == inspected.Id);
        Require(inspected.Health == unit.Health && inspected.MaximumHealth == unit.Profile.Health && inspected.Damage == unit.Profile.Damage && inspected.Level == unit.Level && inspected.Size == unit.Size && inspected.IsBoss == unit.IsBoss && inspected.Description.Length > 0 && inspected.Preview.Length > 0, "inspector uses selected unit's authoritative profile and bundled preview");
        Require(inspected.StatsText.Contains(Game.Core.HealthPoints.Format(unit.Health) + "/" + Game.Core.HealthPoints.Format(unit.Profile.Health), StringComparison.Ordinal) && Math.Abs(inspected.Fraction - Game.Core.PresentationLimits.HealthFraction(unit.Health, unit.Profile.Health)) < .00001, "actual inspection label and health fill match authoritative values");
        Require(opened.SelectedSlot == before.SelectedSlot && AckSequence(client) == acknowledgements, "unit selection retains plot and submits no command");
        await Click(client, "UnitInspector", token);
        UiObservation interior = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(interior.InspectedUnit == inspected && interior.SelectedSlot == before.SelectedSlot, "popup interior blocks underlying selection and keeps frozen stats");
        await Checkpoint(client, frameName, token);
        foreach (var candidate in interior.Targets.Where(pair => pair.Key.StartsWith("Unit", StringComparison.Ordinal) && pair.Key != "UnitInspector" && pair.Value.Visible && pair.Key != "Unit" + inspected.Id))
        {
            await ClickPoint(client, candidate.Value);
            UiObservation switched = await UiProtocol.Probe(client, options.StartupTimeout, token);
            if (switched.InspectedUnit is { } other && other.Id != inspected.Id) { Require(switched.SelectedSlot == before.SelectedSlot, "direct unit replacement retains plot"); break; }
        }
        await Click(client, "Plot0", token);
        UiObservation dismissed = await WaitUi(client, p => p.InspectedUnit is null && p.SelectedSlot == 0, "outside plot click dismisses inspector and selects once", token);
        Require(AckSequence(client) == acknowledgements, "inspection and dismissal remain cosmetic");
        UiTarget unitTarget = dismissed.Targets.First(pair => pair.Key.StartsWith("Unit", StringComparison.Ordinal) && pair.Key != "UnitInspector" && pair.Value.Visible).Value;
        await client.Send(FormattableString.Invariant($"mouse-down {unitTarget.X} {unitTarget.Y}"));
        await client.Send(FormattableString.Invariant($"mouse-move {unitTarget.X + 40} {unitTarget.Y + 20}"));
        await client.Send(FormattableString.Invariant($"mouse-up {unitTarget.X + 40} {unitTarget.Y + 20}"));
        UiObservation dragged = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(dragged.InspectedUnit is null && dragged.SelectedSlot == 0, "drag over units suppresses unit and plot selection");
        await Click(client, "ResetView", token);
    }
    private static long AckSequence(Child client) => client.History().Where(entry => entry.Type == "ack").Select(entry => entry.Result!.Sequence).DefaultIfEmpty().Max();

    private static string RomanLevel(int level)
    {
        string result = "";
        foreach (var (value, numeral) in new[] { (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I") })
            while (level >= value) { result += numeral; level -= value; }
        return result;
    }
}
