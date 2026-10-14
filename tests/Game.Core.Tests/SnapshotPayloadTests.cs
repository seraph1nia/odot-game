using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class SnapshotPayloadTests
{
    [Fact]
    public void FullFourCityCatalogAndPausedStateRoundTripWithoutChangingAnyFields()
    {
        using var match = new Match(combatSeed: 1);
        for (int city = 0; city < 4; city++) match.Join();
        MatchSnapshot snapshot = match.Snapshot() with { Paused = true, Wave = 20 };
        string json = JsonSerializer.Serialize(snapshot, WireJson.Options);
        string payload = SnapshotPayload.Encode(snapshot);
        Assert.True(payload.Length < json.Length / 4, "The complete repeated catalogs must fit well below their uncompressed reliable message size.");
        Assert.Equal(json, JsonSerializer.Serialize(SnapshotPayload.Decode(payload), WireJson.Options));
    }
    [Theory]
    [InlineData("admissions")]
    [InlineData("size")]
    [InlineData("member-size")]
    [InlineData("cap")]
    [InlineData("radius")]
    [InlineData("mage-cap")]
    [InlineData("mage-radius")]
    [InlineData("weapon-size")]
    [InlineData("tower-radius")]
    [InlineData("missing-version")]
    [InlineData("old-version")]
    [InlineData("future-version")]
    [InlineData("null-forecast-profile")]
    public void ReceivingClearlyRefusesRetiredOrUnversionedWireContracts(string defect)
    {
        // Tier A: actual compressed payload into the same receiving boundary as
        // Welcome/ack/snapshot. No silent unknown-field/default reinterpretation,
        // no credentials consumed and no engine/socket required.
        using var match = new Match(combatSeed: 1); match.Join();
        match.Combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        JsonObject json = JsonSerializer.SerializeToNode(match.Snapshot(), WireJson.Options)!.AsObject();
        JsonObject combat = json["Rules"]!["Combat"]!.AsObject();
        switch (defect)
        {
            case "admissions": json["Admissions"] = new JsonArray(); break;
            case "size": combat["Swordsman"]!["Size"] = 2; break;
            case "member-size": json["Players"]![0]!["Soldiers"]![0]!["Size"] = 2; break;
            case "cap": combat["ArrowTower"]!["VictimCap"] = 3; break;
            case "radius": combat["CatapultTower"]!["SplashHexRadius"] = 1; break;
            case "mage-cap": combat["MageVictimCap"] = 2; break;
            case "mage-radius": combat["MageSplashHexRadius"] = 1; break;
            case "weapon-size": json["UnitCatalog"]![0]!["Profile"]!["Size"] = 6; break;
            case "tower-radius": json["TowerCatalog"]![0]!["SplashHexRadius"] = 1; break;
            case "null-forecast-profile":
                json["NextWave"] = new JsonArray(JsonSerializer.SerializeToNode(new UnitState(-1, 0) { Faction = Faction.Skeletons }, WireJson.Options));
                json["NextWave"]![0]!["Profile"] = null;
                break;
            case "missing-version": combat.Remove("RulesVersion"); break;
            case "old-version": combat["RulesVersion"] = 6; json["CombatRulesVersion"] = 6; break;
            default: combat["RulesVersion"] = 8; json["CombatRulesVersion"] = 8; break;
        }
        InvalidDataException error = Assert.Throws<InvalidDataException>(() => SnapshotPayload.DecodeForReceiving(Compress(json)));
        Assert.Contains("Rejoin a compatible authority or start a fresh match", error.Message);
        Assert.NotNull(error.InnerException);
        SnapshotCompatibility.RequireSharedArena(SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(match.Snapshot())));
    }
    [Theory]
    [InlineData("member-duplicate")]
    [InlineData("squad-duplicate")]
    [InlineData("formation")]
    [InlineData("missing-squad")]
    [InlineData("null-members")]
    [InlineData("null-cell")]
    [InlineData("null-footprint")]
    [InlineData("full-fingerprint")]
    [InlineData("null-forecast")]
    [InlineData("forecast-profile")]
    [InlineData("member-cell")]
    [InlineData("member-anchor")]
    [InlineData("member-hex")]
    [InlineData("member-action")]
    [InlineData("member-statuses")]
    [InlineData("member-poison")]
    [InlineData("member-decision")]
    [InlineData("null-body")]
    [InlineData("body-duplicate")]
    [InlineData("body-member-collision")]
    [InlineData("body-cell")]
    [InlineData("body-aim")]
    [InlineData("actor-transit")]
    [InlineData("actor-action")]
    [InlineData("null-event")]
    [InlineData("event-cell")]
    [InlineData("event-pose")]
    [InlineData("event-actor")]
    [InlineData("structure-duplicate")]
    [InlineData("member-keep")]
    [InlineData("member-watchpost")]
    [InlineData("member-owned-target")]
    [InlineData("body-keep")]
    [InlineData("event-keep")]
    [InlineData("actor-keep")]
    [InlineData("actor-watchpost")]
    [InlineData("forecast-origin")]
    [InlineData("forecast-destination")]
    [InlineData("forecast-statuses")]
    [InlineData("member-profile")]
    [InlineData("member-pending")]
    public void MalformedSharedIdentitiesAreRefusedAfterRealCodecDelivery(string defect)
    {
        using var match = new Match(combatSeed: 1); match.Join();
        match.Combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        match.Combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        match.Combat.RestoreHomes();
        MatchSnapshot valid = match.Snapshot();
        UnitState[] soldiers = valid.Players[0].Soldiers;
        MatchSnapshot WithMember(UnitState member) => valid with { Players = [valid.Players[0] with { Soldiers = [member, soldiers[1]] }] };
        UnitState body = soldiers[0] with { Id = 999, Health = 0, Hex = soldiers[0].Hex! with { Id = 999, Lifecycle = UnitLifecycle.Dying, DeathStartTick = 1, DeathEndTick = 49 } };
        MatchSnapshot invalid = defect switch
        {
            "member-duplicate" => valid with { Players = [valid.Players[0] with { Soldiers = [soldiers[0], soldiers[0]] }] },
            "member-keep" => WithMember(soldiers[0] with { TargetCity = true, TargetKind = SquadTargetKind.Keep, TargetId = 999 }),
            "member-watchpost" => WithMember(soldiers[0] with { TargetCity = true, TargetKind = SquadTargetKind.Watchpost, TargetId = 999 }),
            "member-owned-target" => WithMember(soldiers[0] with { TargetCity = true, TargetKind = SquadTargetKind.Keep, TargetId = 1 }),
            "body-keep" => valid with { DyingBodies = [body with { TargetCity = true, TargetKind = SquadTargetKind.Keep, TargetId = 999 }] },
            "event-keep" => valid with { CombatEvents = [new(1, 0, CombatEventType.AttackStarted, soldiers[0], 999, true)] },
            "actor-keep" => valid with { Squads = [valid.Squads[0] with { TargetKind = SquadTargetKind.Keep, TargetId = 999 }] },
            "actor-watchpost" => valid with { Squads = [valid.Squads[0] with { TargetKind = SquadTargetKind.Watchpost, TargetId = 999 }] },
            "event-actor" => valid with { CombatEvents = [new(1, 0, CombatEventType.AttackStarted, null) { ImpactPose = new(soldiers[0].Hex!.Position) }] },
            "structure-duplicate" => valid with { Structures = valid.Structures.Append(valid.Structures[0]).ToArray() },
            "forecast-origin" => valid with { NextWave = [new(-1, soldiers[0].Profile.Health, Origin: 999, Destination: 1) { Faction = Faction.Skeletons, Profile = soldiers[0].Profile }] },
            "forecast-destination" => valid with { NextWave = [new(-1, soldiers[0].Profile.Health, Origin: 1, Destination: 999) { Faction = Faction.Skeletons, Profile = soldiers[0].Profile }] },
            "forecast-statuses" => valid with { NextWave = [new(-1, soldiers[0].Profile.Health, Origin: 1, Destination: 1) { Faction = Faction.Skeletons, Profile = soldiers[0].Profile }] },
            "member-profile" => WithMember(soldiers[0] with { Profile = soldiers[0].Profile with { Damage = soldiers[0].Profile.Damage + 1 } }),
            "member-pending" => WithMember(soldiers[0] with { PendingImpact = true }),
            "member-cell" => WithMember(soldiers[0] with { Hex = soldiers[0].Hex! with { Position = new(int.MaxValue, 1) } }),
            "member-anchor" => WithMember(soldiers[0] with { Hex = soldiers[0].Hex! with { Position = new(soldiers[0].Hex!.Position.Cell, 2) } }),
            "member-hex" => WithMember(soldiers[0] with { Hex = null }),
            "member-action" => WithMember(soldiers[0] with { Hex = soldiers[0].Hex! with { Action = UnitActionKind.Recovery } }),
            "member-statuses" => WithMember(soldiers[0] with { Statuses = null! }),
            "member-poison" => WithMember(soldiers[0] with { Statuses = new() { Poison = [null!] } }),
            "member-decision" => WithMember(soldiers[0] with { Decision = soldiers[0].Decision! with { Route = null! } }),
            "null-body" => valid with { DyingBodies = [null!] },
            "body-duplicate" => valid with { DyingBodies = [body, body] },
            "body-member-collision" => valid with { DyingBodies = [body with { Id = soldiers[0].Id, Hex = body.Hex! with { Id = soldiers[0].Id } }] },
            "body-cell" => valid with { DyingBodies = [body with { Hex = body.Hex! with { Position = new(int.MaxValue, 1) } }] },
            "body-aim" => valid with { DyingBodies = [body with { Hex = body.Hex! with { FrozenAim = new HexPosePoint(new(int.MaxValue, 1)) } }] },
            "actor-transit" => valid with { Squads = [valid.Squads[0] with { Hex = valid.Squads[0].Hex with { Action = UnitActionKind.Moving, ActionSequence = 1, EndTick = 30, Destination = new(int.MaxValue, 1) } }] },
            "actor-action" => valid with { Squads = [valid.Squads[0] with { Hex = valid.Squads[0].Hex with { Action = UnitActionKind.Windup } }] },
            "null-event" => valid with { CombatEvents = [null!] },
            "event-cell" => valid with { CombatEvents = [new(1, 0, CombatEventType.Hit, soldiers[0] with { Hex = soldiers[0].Hex! with { Position = new(int.MaxValue, 1) } })] },
            "event-pose" => valid with { CombatEvents = [new(1, 0, CombatEventType.Hit, soldiers[0]) { ImpactPose = new(new(int.MaxValue, 1)) }] },
            "squad-duplicate" => valid with { Squads = [valid.Squads[0], valid.Squads[0]] },
            "formation" => valid with { Players = [valid.Players[0] with { Soldiers = [soldiers[0], soldiers[1] with { FormationSlot = soldiers[0].FormationSlot }] }] },
            "missing-squad" => valid with { Squads = [] },
            "null-cell" => valid with { Arena = valid.Arena! with { Board = valid.Arena.Board with { Cells = [null!] } } },
            "null-footprint" => valid with { Arena = valid.Arena! with { Footprints = [null!] } },
            // Integrated identity covers phase/rules, not colliding numeric14
            // alone. These use the actual production codec, no socket required.
            "full-fingerprint" => valid with { ConfigurationFingerprint = default },
            "null-forecast" => valid with { NextWave = null! },
            "forecast-profile" => valid with { NextWave = [new(-1, 10) { Faction = Faction.Skeletons, Profile = new() { Health = 10 } }] },
            _ => valid with { Squads = [valid.Squads[0] with { Members = null! }] }
        };
        string before = JsonSerializer.Serialize(valid, WireJson.Options);
        JsonObject wire = JsonSerializer.SerializeToNode(invalid, WireJson.Options)!.AsObject();
        if (defect == "member-statuses") wire["Players"]![0]!["Soldiers"]![0]!["Statuses"] = null;
        if (defect == "member-decision") wire["Players"]![0]!["Soldiers"]![0]!["Decision"]!["Route"] = null;
        if (defect == "forecast-statuses") wire["NextWave"]![0]!["Statuses"] = null;
        Assert.Throws<InvalidDataException>(() => SnapshotPayload.DecodeForReceiving(Compress(wire)));
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        MatchSnapshot received = SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(valid));
        Assert.Equal(JsonSerializer.Serialize(valid, WireJson.Options), JsonSerializer.Serialize(received, WireJson.Options));
    }
    [Theory]
    [InlineData("Rules")]
    [InlineData("Combat")]
    [InlineData("Arena")]
    [InlineData("Combat.Swordsman")]
    [InlineData("Combat.Berserker")]
    [InlineData("Combat.Crossbowman")]
    [InlineData("Combat.Mage")]
    [InlineData("Combat.Defender")]
    [InlineData("Combat.ArrowTower")]
    [InlineData("Combat.RapidArrowTower")]
    [InlineData("Combat.CatapultTower")]
    [InlineData("Combat.BombardTower")]
    [InlineData("Combat.Statuses")]
    [InlineData("Combat.Board")]
    [InlineData("Combat.Board.Cells")]
    [InlineData("Combat.Board.Anchors")]
    [InlineData("Combat.Board.Entries")]
    [InlineData("Combat.Board.SiegeCells")]
    [InlineData("Army")]
    [InlineData("Army.HomePrices")]
    [InlineData("Research")]
    [InlineData("Campaign")]
    [InlineData("Campaign.Waves")]
    public void NullNestedRulesAreRecoverablyRefusedThroughCompressedReceiving(string field)
    {
        using var match = new Match(combatSeed: 1); match.Join();
        MatchSnapshot valid = match.Snapshot();
        string before = JsonSerializer.Serialize(valid, WireJson.Options);
        JsonObject wire = JsonSerializer.SerializeToNode(valid, WireJson.Options)!.AsObject();
        string[] path = field.Split('.');
        JsonNode parent = field == "Rules" ? wire : wire["Rules"]!;
        foreach (string part in path[..^1]) parent = parent[part]!;
        parent[path[^1]] = null;
        InvalidDataException refused = Assert.Throws<InvalidDataException>(() => SnapshotPayload.DecodeForReceiving(Compress(wire)));
        Assert.IsAssignableFrom<ArgumentException>(refused.InnerException);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Assert.Equal(before, JsonSerializer.Serialize(SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(valid)), WireJson.Options));
    }

    private static string Compress(JsonObject json)
    {
        using var output = new MemoryStream();
        using (var compressor = new BrotliStream(output, CompressionLevel.Fastest, leaveOpen: true))
            compressor.Write(JsonSerializer.SerializeToUtf8Bytes(json, WireJson.Options));
        return Convert.ToBase64String(output.ToArray());
    }
    [Fact]
    public void CompressedExpansionCannotExceedTheTransportBound()
    {
        using var output = new MemoryStream();
        using (var compressor = new BrotliStream(output, CompressionLevel.Fastest, leaveOpen: true)) compressor.Write(new byte[16 * 1024 * 1024 + 1]);
        var error = Assert.Throws<InvalidDataException>(() => SnapshotPayload.Decode(Convert.ToBase64String(output.ToArray())));
        Assert.Contains("transport bound", error.Message);
        Assert.Throws<FormatException>(() => SnapshotPayload.Decode("invalid base64"));
    }
}
