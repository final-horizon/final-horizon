using Content.Shared._FinalHorizon.Roles;
using Robust.Shared.Network;

namespace Content.Client._FinalHorizon.Roles.UI;

public sealed partial class RosterSystem : EntitySystem
{
    public List<Roster> CachedRosters = new();
    public Action<List<Roster>>? RosterUpdateAction;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RosterUpdateEvent>(OnRosterUpdated);
    }

    private void OnRosterUpdated(RosterUpdateEvent args)
    {
        CachedRosters = args.Rosters;
        RosterUpdateAction?.Invoke(CachedRosters);
    }

    public void RequestSlot(int rosterIndex, int squadIndex, int slotIndex)
    {
        var ev = new RosterSlotRequestEvent(rosterIndex, squadIndex, slotIndex);
        RaiseNetworkEvent(ev);
    }
}
