using Content.Shared.Damage.Components;
using Content.Shared.Projectiles;
using Content.Shared.Standing;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Events;

namespace Content.Shared.Damage.Systems;

public sealed class RequireProjectileTargetSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RequireProjectileTargetComponent, PreventCollideEvent>(PreventCollide);
        SubscribeLocalEvent<RequireProjectileTargetComponent, StoodEvent>(StandingBulletHit);
        SubscribeLocalEvent<RequireProjectileTargetComponent, DownedEvent>(LayingBulletPass);
    }

    private void PreventCollide(Entity<RequireProjectileTargetComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Active)
            return;

        // done cus flyby sound used to be cancelled by this
        if (TryComp<FixturesComponent>(args.OtherEntity, out var otherFixtures) &&
            otherFixtures.Fixtures.TryGetValue("fly-by", out var flyBy) &&
            ReferenceEquals(flyBy, args.OtherFixture))
        {
            return;
        }
        var other = args.OtherEntity;
        if (TryComp(other, out ProjectileComponent? projectile) &&
            CompOrNull<TargetedProjectileComponent>(other)?.Target != ent)
        {
            // Prevents shooting out of while inside of crates
            var shooter = projectile.Shooter;
            if (!shooter.HasValue)
                return;

            // ProjectileGrenades delete the entity that's shooting the projectile,
            // so it's impossible to check if the entity is in a container
            if (TerminatingOrDeleted(shooter.Value))
                return;

            if (!_container.IsEntityOrParentInContainer(shooter.Value))
               args.Cancelled = true;
        }
    }

    private void SetActive(Entity<RequireProjectileTargetComponent> ent, bool value)
    {
        if (ent.Comp.Active == value)
            return;

        ent.Comp.Active = value;
        Dirty(ent);
    }

    private void StandingBulletHit(Entity<RequireProjectileTargetComponent> ent, ref StoodEvent args)
    {
        SetActive(ent, false);
    }

    private void LayingBulletPass(Entity<RequireProjectileTargetComponent> ent, ref DownedEvent args)
    {
        SetActive(ent, true);
    }
}
