using Robust.Shared.Prototypes;

namespace Content.Shared._FinalHorizon.Players;

[Prototype]
public sealed partial class JobRankPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<string>? ParentRanks { get; set; } = default!;
}
