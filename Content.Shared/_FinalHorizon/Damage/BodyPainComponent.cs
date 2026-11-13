using Robust.Shared.GameStates;

namespace Content.Shared._FinalHorizon.Damage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class BodyPainComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Pain = 0;

    [DataField, AutoNetworkedField]
    public float MaxPain = 100;

    [DataField, AutoNetworkedField]
    public float PainDecay = 5;

    [DataField, AutoNetworkedField]
    public float PainScaling = 20;
}
