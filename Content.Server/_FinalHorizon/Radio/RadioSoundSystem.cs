
using Content.Server.Radio;
using Robust.Server.Audio;

namespace Content.Server._FinalHorizon.Radio;

public sealed partial class RadioSoundSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RadioSoundComponent, RadioReceiveEvent>(OnRadioRecived);
    }

    private void OnRadioRecived(EntityUid uid, RadioSoundComponent comp, RadioReceiveEvent args)
    {
        _audioSystem.PlayPvs(comp.Sound, uid);
    }
}
