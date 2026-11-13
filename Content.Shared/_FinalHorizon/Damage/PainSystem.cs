
using Content.Shared.Popups;
using Content.Shared_FinalHorizon.Damage;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Security.Cryptography;

namespace Content.Shared._FinalHorizon.Damage;

public sealed partial class PainSystem : EntitySystem
{
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    public void CalculatePain(EntityUid uid)
    {
        if (TryComp<BodyPainComponent>(uid, out var painComp))
            CalculatePain(painComp);
    }

    public void CalculatePain(BodyPainComponent painComp)
    {
        var organs = _wound.GetWoundableOrgans(painComp.Owner);
        var pain = 0f;
        foreach (var organ in organs)
        {
            foreach (var wound in organ.Comp.Wounds)
            {
                if (_proto.TryIndex(wound.WoundType, out var woundType))
                    pain += wound.Tended ? woundType.TendedPainMultiplier * (float)wound.Damage : woundType.PainMultiplier * (float)wound.Damage;
            }
        }

        // scales the pain and caps it
        pain = Math.Min(MathF.Log(1f + pain) * painComp.PainScaling, painComp.MaxPain);
        if (pain != painComp.Pain)
        {
            painComp.Pain = pain;
            _popup.PopupPredicted($"Pain: {pain}", painComp.Owner, painComp.Owner, PopupType.MediumCaution);
            DirtyField<BodyPainComponent>(painComp.Owner, "Pain");
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _gameTiming.CurTime;

        var query = EntityQuery<BodyPainComponent>();
        foreach (var painComp in query)
        {
            CalculatePain(painComp);
        }
    }
}
