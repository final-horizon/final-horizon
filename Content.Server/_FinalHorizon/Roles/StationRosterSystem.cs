
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Systems;
using Content.Shared._FinalHorizon.Roles;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Maps;
using Content.Shared.Station;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server._FinalHorizon.Roles;

public sealed partial class StationRosterSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private bool _dirtyRoster = true;
    public List<Roster> CachedRosters = new();
    public override void Initialize()
    {
        base.Initialize();
        _configurationManager.OnValueChanged(CCVars.GameMap, OnMapChange, true);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
        SubscribeLocalEvent<RosterSlotRequestEvent>(OnSlotRequest);
    }

    public void OnRoundEnd(RoundRestartCleanupEvent args)
    {
        _dirtyRoster = true;
    }

    public void OnMapChange(string _)
    {
        if (_gameTicker.RunLevel != 0)
            return;
        _dirtyRoster = true;
    }

    public void GetRosters()
    {
        CachedRosters.Clear();
        string stationString = _configurationManager.GetCVar(CCVars.GameMap);

        if (stationString != null && _protoMan.TryIndex<GameMapPrototype>(stationString, out var stations))
        {
            foreach (var station in stations.Stations)
            {
                if (station.Value.StationComponentOverrides.TryGetComponent<StationRosterComponent>(_componentFactory, out var comp))
                {
                    foreach (var roster in comp.StartingRosters)
                    {
                        var rosterProto = _protoMan.Index<RosterPrototype>(roster);
                        if (rosterProto != null)
                            CachedRosters.Add(rosterProto.Roster);
                    }
                }
            }
        }

        var rosterIndex = 0;
        foreach (var roster in CachedRosters)
        {
            var squadIndex = 0;
            foreach (var squad in roster.Squads)
            {
                var slotIndex = 0;
                foreach (var slot in squad.Slots)
                {
                    slot.RosterId = rosterIndex;
                    slot.SquadId = squadIndex;
                    slot.SlotId = slotIndex;
                    slotIndex++;
                }
                squadIndex++;
            }
            rosterIndex++;
        }

        _dirtyRoster = true;
    }

    public void UpdateRoster()
    {
        var rosters = CachedRosters;
        _dirtyRoster = false;
        RaiseNetworkEvent(new RosterUpdateEvent(rosters));
    }

    private void OnSlotRequest(RosterSlotRequestEvent args)
    {
        var targetSlot = CachedRosters[args.RosterIndex].Squads[args.SquadIndex].Slots[args.SlotIndex];


        if (targetSlot.User == null)
        {
            targetSlot.User = args.Id;

            foreach (var roster in CachedRosters)
            {
                foreach (var squad in roster.Squads)
                {
                    foreach (var slot in squad.Slots)
                    {
                        if (slot.User == args.Id)
                        {
                            slot.User = null;
                        }
                    }
                }
            }
            _dirtyRoster = true;
            return;
        }

        if (targetSlot.User == args.Id)
        {
            targetSlot.User = null;
            _dirtyRoster = true;
            return;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_dirtyRoster)
        {
            UpdateRoster();
        }
    }

}
