using Content.Shared._FinalHorizon.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._FinalHorizon.Roles;

[RegisterComponent]
public sealed partial class StationRosterComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<RosterPrototype>> StartingRosters = default!;
}
