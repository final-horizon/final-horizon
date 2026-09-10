

using Content.Shared._ES.Viewcone.Components;
using System.Numerics;

namespace Content.Client._FinalHorizon.Viewcone;

public sealed partial class FHViewconeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESViewconeComponent, ESViewconeGetAngleModifierEvent>(OnGetModifiedAngle);
    }

    private void OnGetModifiedAngle(EntityUid uid, ESViewconeComponent comp, ref ESViewconeGetAngleModifierEvent args)
    {
        if (!TryComp<EyeComponent>(uid, out var eyeComp))
            return;

        var offset = eyeComp.Offset;
        args.LowerBoundClamp = comp.ConeAngleFloor;
        args.ModifyAngle(-offset.Length() * comp.ConeOffsetTightness);
    }
}
