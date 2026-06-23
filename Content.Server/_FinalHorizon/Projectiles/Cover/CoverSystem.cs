using Robust.Server.GameObjects;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Server._FinalHorizon.Projectiles.Cover;

public sealed partial class CoverSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoverComponent, CoverHitAttemptEvent>(OnCoverHitAttempt);
    }

    private void OnCoverHitAttempt(EntityUid target, CoverComponent comp, ref CoverHitAttemptEvent args)
    {
        if (args.ProjectileComp.Shooter != null)
        {
            var targetPos = _transform.GetWorldPosition(target);
            var shooterPos = _transform.GetWorldPosition(args.ProjectileComp.Shooter.Value);
            var distance = Vector2.Distance(targetPos, shooterPos);

            if (distance < comp.CloseMissDistance)
            {
                args.Missed = true;
                return;
            }
        }

        var roll = _random.NextFloat();
        if (roll > comp.HitChance)
        {
            args.Missed = true;
        }
    }
};
