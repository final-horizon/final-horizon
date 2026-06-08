using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using YamlDotNet.Serialization;

namespace Content.Shared._FinalHorizon.Roles;

[Prototype]
public sealed partial class RosterPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public Roster Roster = default!;

}

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class Roster
{
    [DataField]
    public string? Name;

    [DataField]
    public List<Squad> Squads = default!;
}

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class Squad
{
    [DataField]
    public string? Name;

    [DataField]
    public List<JobSlot> Slots { get; private set; } = default!;
}

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class JobSlot
{
    [DataField]
    public NetUserId? User;

    [DataField]
    public ProtoId<JobPrototype> Job { get; private set; } = default!;

    [DataField]
    public int? RosterId;

    [DataField]
    public int? SquadId;

    [DataField]
    public int? SlotId;
}
