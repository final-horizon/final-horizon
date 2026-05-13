using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Stalker.PullDoAfter;

[RegisterComponent, NetworkedComponent] // FH - make it networked
public sealed partial class PullDoAfterComponent : Component
{
    [DataField]
    public float PullTime = 2f;

    // If this doAfter bar should be hidden
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Hidden = true; // FH
}

[Serializable, NetSerializable]
public sealed partial class PullDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity StorageEnt;
    public bool Interact;

    public PullDoAfterEvent(NetEntity storageEnt, bool interact = false)
    {
        StorageEnt = storageEnt;
        Interact = interact;
    }
}
