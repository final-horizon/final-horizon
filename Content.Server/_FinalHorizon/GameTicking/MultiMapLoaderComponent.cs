using Robust.Shared.Utility;

namespace Content.Server._FinalHorizon.GameTicking;

[RegisterComponent]
public sealed partial class MultiMapLoaderComponent : Component
{
    [DataField]
    public List<ResPath> Maps = [];
}
