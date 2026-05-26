using Robust.Shared.Audio;

namespace Content.Server._FinalHorizon.Radio;

[RegisterComponent]
public sealed partial class RadioSoundComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound;
}
