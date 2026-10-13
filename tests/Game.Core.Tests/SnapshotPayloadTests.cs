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
    public void MalformedSharedIdentitiesAreRefusedAfterRealCodecDelivery(string defect)
    {
        using var match = new Match(combatSeed: 1); match.Join();
        match.Combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        match.Combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        MatchSnapshot valid = match.Snapshot();
        UnitState[] soldiers = valid.Players[0].Soldiers;
        MatchSnapshot invalid = defect switch
        {
            "member-duplicate" => valid with { Players = [valid.Players[0] with { Soldiers = [soldiers[0], soldiers[0]] }] },
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
        Assert.Throws<InvalidDataException>(() => SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(invalid)));
        MatchSnapshot received = SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(valid));
        Assert.Equal(JsonSerializer.Serialize(valid, WireJson.Options), JsonSerializer.Serialize(received, WireJson.Options));
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
