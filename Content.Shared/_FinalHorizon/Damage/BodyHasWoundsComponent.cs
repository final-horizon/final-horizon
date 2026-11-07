using Robust.Shared.GameStates;

namespace Content.Shared_FinalHorizon.Damage;

/// <summary>
/// Added to bodys which have wounds to be checked in the update loop.
/// </summary>
[RegisterComponent]
public sealed partial class BodyHasWoundsComponent : Component
{
    [DataField]
    public TimeSpan NextCheck = TimeSpan.Zero;

    [DataField]
    public TimeSpan CheckInverval = TimeSpan.FromSeconds(15);
}
