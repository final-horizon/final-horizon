using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._CE.EntityEffect.Effects;

/// <summary>
/// Plays a non-positional, global sound to every listener on the same map as the effect's coordinates
/// who is within [<see cref="MinRange"/>, <see cref="MaxRange"/>] of it.
/// Meant for "heard from a distance" flavor sounds (e.g. distant thunder) that should stay reliably
/// audible regardless of positional attenuation.
/// Pair with a <see cref="PlaySound"/> effect for the close/positional variant.
/// </summary>
public sealed partial class PlayFarSound : CEEntityEffectBase<PlayFarSound>
{
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    /// <summary>
    /// Listeners closer than this (in meters) are excluded, since they're expected to already hear a
    /// closer/positional variant of the sound.
    /// </summary>
    [DataField]
    public float MinRange = 15f;

    /// <summary>
    /// Listeners farther than this (in meters) are excluded entirely.
    /// </summary>
    [DataField]
    public float MaxRange = 60f;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("ce-entity-effect-guidebook-play-far-sound");
}

public sealed partial class CEPlayFarSoundEffectSystem : CEEntityEffectSystem<PlayFarSound>
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    [Dependency] private EntityQuery<TransformComponent> _xformQuery = default!;

    protected override void Effect(ref CEEntityEffectEvent<PlayFarSound> args)
    {
        if (_net.IsClient)
            return;

        if (!TryResolveEffectCoordinates(args.Args, args.Effect.EffectTarget, out var coords))
            return;

        var sourcePos = TransformSystem.ToMapCoordinates(coords);

        if (sourcePos.MapId == MapId.Nullspace)
            return;

        var minRange = args.Effect.MinRange;
        var maxRange = args.Effect.MaxRange;

        // Only players on the same map as the source are considered, then filtered by distance.
        var filter = Filter.BroadcastMap(sourcePos.MapId).RemoveWhereAttachedEntity(uid =>
        {
            if (!_xformQuery.TryGetComponent(uid, out var xform))
                return true;

            var distance = (TransformSystem.GetWorldPosition(xform) - sourcePos.Position).Length();
            return distance < minRange || distance > maxRange;
        });

        _audio.PlayGlobal(args.Effect.Sound, filter, true);
    }
}
