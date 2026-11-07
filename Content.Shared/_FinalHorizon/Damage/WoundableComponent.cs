using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared_FinalHorizon.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._FinalHorizon.Damage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class WoundableComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<Wound> Wounds { get; set; } = [];
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class Wound
{
    public FixedPoint2 Damage = 0;
    public ProtoId<WoundTypePrototype>? WoundType;
}
