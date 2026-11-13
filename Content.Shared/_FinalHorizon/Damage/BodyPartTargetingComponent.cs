using Robust.Shared.GameStates;

namespace Content.Shared_FinalHorizon.Damage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyPartTargetingComponent : Component
{
    [DataField, AutoNetworkedField]
    public TargetableBodyParts TargetedPart { get; set; }
}

public enum TargetableBodyParts
{
    Invalid,
    Head,
    Torso,
    LeftArm,
    LeftHand,
    RightArm,
    RightHand,
    LeftLeg,
    LeftFoot,
    RightLeg,
    RightFoot,
}
