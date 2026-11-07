using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared_FinalHorizon.Damage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WoundTopicalComponent : Component
{
    /// <summary>
    /// Which wound types can this item be used on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<WoundTypePrototype>> UsableWounds { get; set; } = [];

    [DataField, AutoNetworkedField]
    public float UseTimeMultiplier = 1;
}
