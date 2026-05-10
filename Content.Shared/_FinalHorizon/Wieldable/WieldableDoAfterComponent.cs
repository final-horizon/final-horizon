using Content.Shared.DoAfter;
using Content.Shared.Wieldable.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._FinalHorizon.Wieldable;

[RegisterComponent, NetworkedComponent]
public sealed partial class WieldableDoAfterComponent : Component
{
    /// <summary>
    /// Time set on the doafter.
    /// </summary>
    [DataField]
    public TimeSpan Timer = TimeSpan.FromSeconds(0.5);

    [DataField]
    public bool CachedWieldState = false;
}

[Serializable, NetSerializable]
public sealed partial class WieldableDoAfterEvent : SimpleDoAfterEvent
{
}
