using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class ArmyCommandTests
{
    [Fact]
    public void CompletedPaidStoredVeteranSurvivesWireResumeAndReadyRetriesWithoutExtraRecovery()
    {
        using var authority = new AuthoritySession(AuthorityPolicy.PlayingHost,
            new Rules { StartingGold = 100, StartingWood = 100, Campaign = CampaignFixture.Three(1, 1, 1) }, combatSeed: 1);
        AdmissionResult guest = authority.Admit(2, WireJson.ProtocolVersion, "", 0); Assert.True(guest.Accepted);
        long[] sequences = [0, 0, 0]; int peer = 2; ulong time = 0;
        Command Request(int player, string action, int slot = -1, Building building = Building.Empty, int id = 0)
        {
            MatchSnapshot state = authority.Snapshot(); CityState city = state.Players.Single(p => p.Id == player);
            return new(++sequences[player], state.MatchId, state.Phase, state.TurnSerial, action, player, slot, building,
                ExpectedGeneration: slot >= 0 ? city.Slots[slot].Generation : 0, UnitId: id);
        }
        CommandResult Send(Command request) => request.City == 1 ? authority.RequestLocal(JsonSerializer.Serialize(request, WireJson.Options))
            : authority.Request(peer, JsonSerializer.Serialize(request, WireJson.Options), time += 1000)!;
        Command Act(int player, string action, int slot = -1, Building building = Building.Empty, int id = 0)
        { Command request = Request(player, action, slot, building, id); Assert.True(Send(request).Accepted); return request; }
        void Ready() { Act(1, "ready"); Act(2, "ready"); }
        Act(1, "start");
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, "build", 0, Building.Barracks); Act(player, "build", 2, Building.MetalMine);
            Act(player, "build", 3, Building.Stonecutter); Act(player, "build", 4, Building.Farm);
        }
        Ready(); Ready(); Act(2, "build", 1, Building.TownHall); Ready();
        foreach (int player in new[] { 1, 2 }) for (int i = 0; i < 6; i++) Act(player, "recruit", 0);
        Ready();
        for (int tick = 0; tick < 2000 && (authority.Phase == Phase.Combat || authority.Snapshot().DyingBodies.Length > 0); tick++) authority.Step();
        Assert.Equal(Phase.Building, authority.Phase);
        CityState Guest() => authority.Snapshot().Players.Single(p => p.Id == 2);
        UnitState veteran = Guest().Soldiers.First(u => u.Health < u.Profile.Health);
        Assert.True(veteran.RecoveryEligible); Act(2, "store", 1, id: veteran.Id);
        for (int i = 0; i < 3; i++) { Act(2, "ready"); Act(2, "unready"); Assert.Equal(veteran.Health, Guest().Soldiers.Single(u => u.Id == veteran.Id).Health); }
        Assert.Equal(3, authority.Snapshot().ProductionCount);
        Act(1, "pause"); authority.Disconnect(peer); peer = 3;
        AdmissionResult resumed = authority.Admit(peer, WireJson.ProtocolVersion, guest.Credential, time + 1000);
        Assert.True(resumed.Accepted); Assert.Equal(2, resumed.PlayerId);
        MatchSnapshot wire = JsonSerializer.Deserialize<MatchSnapshot>(JsonSerializer.Serialize(resumed.State, WireJson.Options), WireJson.Options)!;
        UnitState stored = wire.Players.Single(p => p.Id == 2).Soldiers.Single(u => u.Id == veteran.Id);
        Assert.Equal(veteran.Health, stored.Health); Assert.Equal(veteran.Level, stored.Level); Assert.Equal(veteran.Profile, stored.Profile);
        Assert.True(stored.Assignment!.Stored); Assert.True(stored.RecoveryEligible);
        Assert.Contains(stored.Id, wire.Players.Single(p => p.Id == 2).LastUpkeep!.Funded);
        for (int i = 0; i < 10; i++) authority.Step(); Assert.Equal(stored.Health, Guest().Soldiers.Single(u => u.Id == stored.Id).Health);
        Act(1, "resume"); Act(1, "ready"); Command ready = Act(2, "ready");
        Assert.Equal(stored.Health + 200, Guest().Soldiers.Single(u => u.Id == stored.Id).Health);
        string produced = JsonSerializer.Serialize(authority.Snapshot(), WireJson.Options);
        Assert.True(Send(ready).Accepted); Assert.Equal(produced, JsonSerializer.Serialize(authority.Snapshot(), WireJson.Options));
        Assert.Equal(4, authority.Snapshot().ProductionCount);
    }

    [Fact]
    public void SerializedOwnedRosterCommandsRetainRetryPauseReadyForeignAndGenerationGuards()
    {
        using var authority = new AuthoritySession(AuthorityPolicy.PlayingHost, new Rules { StartingGold = 100, StartingWood = 100 });
        AdmissionResult guest = authority.Admit(2, WireJson.ProtocolVersion, "", 0); Assert.True(guest.Accepted);
        long sequence = 0, guestSequence = 0;
        Command Request(string action, int slot = -1, Building building = Building.Empty, int unit = 0)
        {
            MatchSnapshot state = authority.Snapshot(); CityState city = state.Players[0];
            return new(++sequence, state.MatchId, state.Phase, state.TurnSerial, action, 1, slot, building,
                ExpectedGeneration: slot >= 0 ? city.Slots[slot].Generation : 0, UnitId: unit, ExpectedHomeCount: city.Army!.PurchasedHomes,
                ExpectedTrackLevel: slot >= 0 ? action == "upgrade-healing" ? city.Slots[slot].HealingLevel : action == "upgrade-capacity" ? city.Slots[slot].CapacityLevel : -1 : -1);
        }
        CommandResult Send(Command command) => authority.RequestLocal(JsonSerializer.Serialize(command, WireJson.Options));
        void Act(string action, int slot = -1, Building building = Building.Empty, int unit = 0) => Assert.True(Send(Request(action, slot, building, unit)).Accepted);
        string State() => JsonSerializer.Serialize(authority.Snapshot(), WireJson.Options);
        void Reject(Command command) { string before = State(); Assert.False(Send(command).Accepted); Assert.Equal(before, State()); }
        void Produce()
        {
            Act("ready"); MatchSnapshot state = authority.Snapshot();
            var ready = new Command(++guestSequence, state.MatchId, state.Phase, state.TurnSerial, "ready", 2);
            Assert.True(authority.Request(2, JsonSerializer.Serialize(ready, WireJson.Options), 0)!.Accepted);
        }
        Act("start"); Act("build", 0, Building.Barracks); Act("build", 2, Building.MetalMine); Act("build", 3, Building.Stonecutter); Act("build", 4, Building.Farm);
        Produce(); Produce(); Act("build", 1, Building.TownHall); Act("recruit", 0);
        int unit = authority.Snapshot().Players[0].Soldiers.Single().Id;
        Command store = Request("store", 1, unit: unit); Assert.True(Send(store).Accepted);
        string stored = State(); Assert.True(Send(store).Accepted); Assert.Equal(stored, State());
        Assert.True(authority.Snapshot().Players[0].Soldiers.Single().Assignment!.Stored);
        Reject(Request("send", 1, unit: unit) with { ExpectedGeneration = store.ExpectedGeneration + 1 });
        Reject(Request("send", 1, unit: unit) with { TurnSerial = store.TurnSerial - 1 });
        Reject(Request("retire", unit: unit) with { City = 2 });
        Reject(Request("retire", unit: unit + 100));
        Reject(Request("buy-home") with { ExpectedHomeCount = 3 });
        Act("pause"); Reject(Request("send", 1, unit: unit));
        string frozen = State(); Assert.True(Send(store).Accepted); Assert.Equal(frozen, State()); Act("resume");
        Act("ready"); Reject(Request("retire", unit: unit)); Act("unready");
        authority.Disconnect(2); AdmissionResult resumed = authority.Admit(3, WireJson.ProtocolVersion, guest.Credential, 1000);
        Assert.True(resumed.Accepted); Assert.Equal(guest.PlayerId, resumed.PlayerId);
        Assert.True(authority.Snapshot().Players[0].Soldiers.Single().Assignment!.Stored);
        Act("send", 1, unit: unit);
        Assert.Equal(unit, authority.Snapshot().Players[0].Soldiers.Single().Id);
        Assert.False(authority.Snapshot().Players[0].Soldiers.Single().RecoveryEligible);
        Command retire = Request("retire", unit: unit); Assert.True(Send(retire).Accepted);
        string retired = State(); Assert.True(Send(retire).Accepted); Assert.Equal(retired, State());
        Assert.Empty(authority.Snapshot().Players[0].Soldiers);
        Assert.Empty(authority.Snapshot().DyingBodies);
    }
}
