using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Starfall.Particles;

/// <summary>
/// Base class for particle-on-event components that cannot be replaced by
/// <see cref="Effects.SpawnParticleEffect"/>
/// </summary>
public abstract partial class ParticleOnEventBase : Component
{
    /// <summary>The particle effect to spawn.</summary>
    [DataField(required: true)]
    public ProtoId<ParticleEffectPrototype> Effect;

    /// <summary>Optional color tint applied to the spawned particles.</summary>
    [DataField]
    public Color? ColorOverride;
}

/// <summary>
/// Spawns a particle effect while this entity is in flight after being thrown.
/// The emitter is automatically stopped when the entity lands or is deleted.
/// Infinite-duration effects can be used the emitter is destroyed with the projectile.
/// </summary>
[RegisterComponent]
public sealed partial class ParticleOnThrownComponent : ParticleOnEventBase
{
}

/// <summary>
/// Spawns a particle effect on each projectile fired by this gun.
/// Infinite-duration effects can be used the emitter is destroyed with the projectile.
/// </summary>
[RegisterComponent]
public sealed partial class ParticleOnGunShotProjectileComponent : ParticleOnEventBase
{
}

// FH start
/// <summary>
/// Spawns a particle on this entity when a projectile hits it.
/// </summary>
[RegisterComponent]
public sealed partial class ParticleOnHitByProjectileComponent : ParticleOnEventBase
{
}

/// <summary>
/// A component that must be on a projectile to trigger the ParticleOnHitByProjectileComponent.
/// </summary>
[RegisterComponent]
public sealed partial class ParticleOnHitByProjectileCauserComponent : Component
{
    /// <summary>
    /// The intensity of the effect.
    /// </summary>
    public float EffectIntensity = 1;
}

[Serializable, NetSerializable]
public sealed class ParticleOnHitByProjectileEvent : EntityEventArgs
{
    public ProtoId<ParticleEffectPrototype> Effect;
    public float EffectIntensity;
    public Angle Angle;
    public MapCoordinates Pos;

    public ParticleOnHitByProjectileEvent(ProtoId<ParticleEffectPrototype> effect, float effectIntensity, Angle angle, MapCoordinates pos)
    {
        Effect = effect;
        EffectIntensity = effectIntensity;
        Angle = angle;
        Pos = pos;
    }
}
// FH end
