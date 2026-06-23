using Content.Shared.Projectiles;

namespace Content.Server._FinalHorizon.Projectiles.Cover;

[RegisterComponent]
public sealed partial class CoverComponent : Component
{
    [DataField]
    public float HitChance = 0.5f;

    [DataField]
    public float CloseMissDistance = 4f;
};

[ByRefEvent]
public record struct CoverHitAttemptEvent
{
    public bool Missed = false;

    public EntityUid Projectile { get; set; }

    public ProjectileComponent ProjectileComp { get; set; }

    public CoverHitAttemptEvent(EntityUid projectile, ProjectileComponent projectileComp)
    {
        Projectile = projectile;
        ProjectileComp = projectileComp;
    }

}
