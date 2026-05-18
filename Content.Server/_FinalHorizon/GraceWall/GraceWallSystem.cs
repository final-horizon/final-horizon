using Content.Server.Audio;
using Content.Server.Chat.Managers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Player;

namespace Content.Server._FinalHorizon.GraceWall;

public sealed partial class GraceWallSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private GraceWallManagerComponent _graceManager = null!;
    private bool _hasWarned = true;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GraceWallManagerComponent, ComponentInit>(OnManagerInit);
        SubscribeLocalEvent<GraceWallManagerComponent, ComponentRemove>(OnManagerRemove);
    }

    private void OnManagerInit(EntityUid uid, GraceWallManagerComponent comp, ComponentInit args)
    {
        _graceManager = comp;
        _graceManager.EndTime = _timing.CurTime + _graceManager.Delay;
        if (_graceManager.EndTime > _timing.CurTime + TimeSpan.FromMinutes(1))
            _hasWarned = false;
    }

    private void OnManagerRemove(EntityUid uid, GraceWallManagerComponent comp, ComponentRemove args)
    {
        _graceManager = null!;
        _hasWarned = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_graceManager == null)
            return;

        if (!_hasWarned && _graceManager.EndTime < _timing.CurTime + TimeSpan.FromMinutes(1))
        {
            _chat.DispatchServerAnnouncement("One minute until grace wall falls!", Color.OrangeRed);
            _hasWarned = true;
        }

        if (_graceManager.EndTime < _timing.CurTime)
        {
            var query = EntityQuery<GraceWallComponent>();
            foreach (var ent in query)
                QueueDel(ent.Owner);
            _chat.DispatchServerAnnouncement("Grace wall falling!", Color.DarkRed);
            if (_graceManager.Sound != null)
                _audio.PlayGlobal(_graceManager.Sound, Filter.Broadcast(), false);
            _graceManager = null!;
        }


    }
}
