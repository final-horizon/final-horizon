using System.Numerics;
using Content.Shared.DoAfter;
using Content.Client.UserInterface.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Containers;

namespace Content.Client.DoAfter;

public sealed class DoAfterOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";

    private readonly IEntityManager _entManager;
    private readonly IGameTiming _timing;
    private readonly IPlayerManager _player;
    private readonly SharedTransformSystem _transform;
    private readonly MetaDataSystem _meta;
    private readonly ProgressColorSystem _progressColor;
    private readonly SharedContainerSystem _container;
    private readonly SpriteSystem _sprite;

    private readonly Texture _barTexture;
    private readonly SpriteSpecifier _barSprite; // FH start - overhaul the bar to use the fully animated sprite instead
    private readonly ShaderInstance _unshadedShader;

    /// <summary>
    ///     Flash time for cancelled DoAfters
    /// </summary>
    private const float FlashTime = 0.125f;

    // Hardcoded width of the progress bar because it doesn't match the texture.
    private const float StartX = 2;
    private const float EndX = 22f;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public DoAfterOverlay(IEntityManager entManager, IPrototypeManager protoManager, IGameTiming timing, IPlayerManager player)
    {
        _entManager = entManager;
        _timing = timing;
        _player = player;
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
        _meta = _entManager.EntitySysManager.GetEntitySystem<MetaDataSystem>();
        _container = _entManager.EntitySysManager.GetEntitySystem<SharedContainerSystem>();
        _progressColor = _entManager.System<ProgressColorSystem>();
        _sprite = _entManager.System<SpriteSystem>();
        _barSprite = new SpriteSpecifier.Rsi(new("/Textures/_FinalHorizon/Interface/Misc/progress_bar.rsi"), "progress_bar");
        _barTexture = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(_barSprite);

        _unshadedShader = protoManager.Index(UnshadedShader).Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

        // If you use the display UI scale then need to set max(1f, displayscale) because 0 is valid.
        const float scale = 1f;
        var scaleMatrix = Matrix3Helpers.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3Helpers.CreateRotation(-rotation);

        var curTime = _timing.CurTime;

        var bounds = args.WorldAABB.Enlarged(5f);
        var localEnt = _player.LocalSession?.AttachedEntity;

        var metaQuery = _entManager.GetEntityQuery<MetaDataComponent>();
        var enumerator = _entManager.AllEntityQueryEnumerator<ActiveDoAfterComponent, DoAfterComponent, SpriteComponent, TransformComponent>();
        while (enumerator.MoveNext(out var uid, out _, out var comp, out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            if (comp.DoAfters.Count == 0)
                continue;

            var worldPosition = _transform.GetWorldPosition(xform, xformQuery);
            if (!bounds.Contains(worldPosition))
                continue;

            handle.UseShader(null);

            // If the entity is paused, we will draw the do-after as it was when the entity got paused.
            var meta = metaQuery.GetComponent(uid);
            var time = meta.EntityPaused
                ? curTime - _meta.GetPauseTime(uid, meta)
                : curTime;

            var worldMatrix = Matrix3Helpers.CreateTranslation(worldPosition);
            var scaledWorld = Matrix3x2.Multiply(scaleMatrix, worldMatrix);
            var matty = Matrix3x2.Multiply(rotationMatrix, scaledWorld);
            handle.SetTransform(matty);

            var offset = 0f;

            var isInContainer = _container.IsEntityOrParentInContainer(uid, meta, xform);

            foreach (var doAfter in comp.DoAfters.Values)
            {

                // Use the sprite itself if we know its bounds. This means short or tall sprites don't get overlapped
                // by the bar.
                var yOffset = _sprite.GetLocalBounds((uid, sprite)).Height / 2f + 0.05f;

                // Position above the entity (we've already applied the matrix transform to the entity itself)
                // Offset by the texture size for every do_after we have.
                var position = new Vector2(-_barTexture.Width / 2f / EyeManager.PixelsPerMeter,
                    yOffset / scale + offset / EyeManager.PixelsPerMeter * scale * 0.25f);



                var elapsed = time - doAfter.StartTime;
                var elapsedRatio = (float)Math.Min(1, elapsed.TotalSeconds / doAfter.Args.Delay.TotalSeconds);

                var spriteBar = _sprite.GetFrame(_barSprite, TimeSpan.FromSeconds(elapsedRatio * 21), false);
                handle.DrawTexture(spriteBar, position);
                 // FH end
                offset += _barTexture.Height / scale;
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }

    public Color GetProgressColor(float progress, float alpha = 1f)
    {
        return _progressColor.GetProgressColor(progress).WithAlpha(alpha);
    }
}
