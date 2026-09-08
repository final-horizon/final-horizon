using Robust.Shared.GameStates;

namespace Content.Shared._FinalHorizon.Aim;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AimComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Aiming = false;
}
