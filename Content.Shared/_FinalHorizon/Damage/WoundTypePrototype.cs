using Robust.Shared.Prototypes;

namespace Content.Shared_FinalHorizon.Damage;

[Prototype]
public sealed partial class WoundTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Rate of bleed from this wound.
    /// </summary>
    [DataField]
    public float BleedRate = 0;

    // A multiplier to the constant pain recieved when having this wound. Unimplemented
    [DataField]
    public float PainMultiplier = 0;

    // A multiplier to the constant pain recieved when having this wound while its tended. Unimplemented
    [DataField]
    public float TendedPainMultiplier = 0;

    [DataField]
    public Color Color = Color.Red;

    [DataField]
    public float SelfTendChance = 0.02f;

    [DataField]
    public float TendedHealChance = 0.10f;
}
