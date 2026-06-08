using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._FinalHorizon.Roles;

[Serializable, NetSerializable]
public sealed class RosterUpdateEvent : EntityEventArgs
{
    public List<Roster> Rosters { get; }

    public RosterUpdateEvent(List<Roster> roster)
    {
        Rosters = roster;
    }
}

[Serializable, NetSerializable]
public sealed class RosterSlotRequestEvent : EntityEventArgs
{
    public int RosterIndex { get; }
    public int SquadIndex { get; }
    public int SlotIndex { get; }

    public RosterSlotRequestEvent(int rosterIndex, int squadIndex, int slotIndex)
    {
        RosterIndex = rosterIndex;
        SquadIndex = squadIndex;
        SlotIndex = slotIndex;
    }
}
