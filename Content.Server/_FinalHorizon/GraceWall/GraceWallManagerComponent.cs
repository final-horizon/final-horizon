using Robust.Shared.Audio;

namespace Content.Server._FinalHorizon.GraceWall;

[RegisterComponent]
public sealed partial class GraceWallManagerComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromMinutes(10);

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_FinalHorizon/Effects/Siren/siren2.ogg");

    [DataField]
    public TimeSpan EndTime;
}
