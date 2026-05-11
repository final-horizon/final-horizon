using Robust.Shared.GameStates;

namespace Content.Shared._FinalHorizon.FoldableGun;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FoldableGunComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Folded = true;

    [DataField, AutoNetworkedField]
    public TimeSpan FoldTime = TimeSpan.FromSeconds(1);
}
