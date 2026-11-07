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

    // The type of wound this turns into when healed with the correct item. Null will remove the wound when healed. Unimplemented
    [DataField]
    public ProtoId<WoundTypePrototype>? MendedWoundType { get; set; }
}
