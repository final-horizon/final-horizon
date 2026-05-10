using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;

namespace Content.Shared._FinalHorizon.Wieldable;

public sealed partial class WieldableDoAfterSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedWieldableSystem _wieldable = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WieldableDoAfterComponent, WieldableDoAfterEvent>(OnWieldDoAfterEnd);
        SubscribeLocalEvent<WieldableDoAfterComponent, UseInHandEvent>(OnUseInHand, before: [typeof(SharedWieldableSystem)]);
    }

    public void TryWieldDoAfter(EntityUid used, EntityUid user, WieldableDoAfterComponent comp, WieldableComponent wieldable)
    {
        var doAfterArgs = new DoAfterArgs(
        EntityManager,
        user,
        comp.Timer,
        new WieldableDoAfterEvent(),
        used,
        used: used)
        {
            NeedHand = true,
            BreakOnHandChange = true,
            BreakOnDropItem = true,
            BlockDuplicate = true,
            Hidden = false,
        };
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    public void OnWieldDoAfterEnd(EntityUid uid, WieldableDoAfterComponent comp, WieldableDoAfterEvent args)
    {
        if (!args.DoAfter.Completed || !TryComp<WieldableComponent>(uid, out var wieldable))
            return;
        _wieldable.TryWield(uid, wieldable, args.User);
    }

    public void OnUseInHand(EntityUid uid, WieldableDoAfterComponent comp, UseInHandEvent args)
    {
        if (TryComp<WieldableComponent>(uid, out var wieldable))
            comp.CachedWieldState = wieldable.Wielded;
    }
}
