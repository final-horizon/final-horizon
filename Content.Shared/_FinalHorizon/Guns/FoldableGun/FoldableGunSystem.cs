using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._FinalHorizon.FoldableGun;

public sealed partial class FoldableGunSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedHandsSystem _hand = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoldableGunComponent, GetVerbsEvent<AlternativeVerb>>(AddFoldGunVerb);
        SubscribeLocalEvent<FoldableGunComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<FoldableGunComponent, ComponentInit>(OnFoldableInit);
        SubscribeLocalEvent<FoldableGunComponent, FoldableGunDoafterEvent>(TrySetFolded);
        SubscribeLocalEvent<FoldableGunComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnHandleState(EntityUid uid, FoldableGunComponent component, ref AfterAutoHandleStateEvent args)
    {
        SetFolded(uid, component, component.Folded);
    }

    private void OnFoldableInit(EntityUid uid, FoldableGunComponent component, ComponentInit args)
    {
        SetFolded(uid, component, component.Folded);
    }

    public void SetFolded(EntityUid uid, FoldableGunComponent component, bool folded, EntityUid? user = null)
    {
        component.Folded = folded;
        Dirty(uid, component);
        _appearance.SetData(uid, FoldedGunVisuals.State, folded);
        _item.SetHeldPrefix(uid, folded ? "folded" : null);

        if (!TryComp<MultiHandedItemComponent>(uid, out var multi))
            return;

        multi.HandsNeeded = folded ? 1 : 2;

        if (user == null)
            return;

        if (!folded)
        {
            _virtualItem.TrySpawnVirtualItemInHand(uid, user.Value);
        }
        else
        {
            _virtualItem.DeleteInHandsMatching(user.Value, uid);
        }
    }

    public void TrySetFolded(EntityUid uid, FoldableGunComponent comp, FoldableGunDoafterEvent args)
    {
        if (args.Cancelled)
        {
            _audioSystem.Stop(comp.AudioStream);
            return;
        }

        if (!_hand.IsHolding(args.User, uid))
            return;

        SetFolded(uid, comp, args.Folding, args.User);
        return;
    }

    private void AddFoldGunVerb(EntityUid uid, FoldableGunComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || !_hand.IsHolding(args.User, args.Target))
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                var doAfterArgs = new DoAfterArgs(
                EntityManager,
                args.User,
                component.FoldTime,
                new FoldableGunDoafterEvent() { Folding = !component.Folded },
                uid,
                uid)
                {
                    NeedHand = true,
                    BreakOnHandChange = true,
                    BreakOnDropItem = true,
                    BlockDuplicate = true,
                    Hidden = false,
                };
                var result = _doAfter.TryStartDoAfter(doAfterArgs);

                if (result)
                {
                    component.AudioStream = _audioSystem.PlayPredicted(component.FoldSound, uid, args.User)?.Entity ?? component.AudioStream;
                    if (component.Folded)
                        _popup.PopupClient("Undeploying", uid, args.User);
                    else
                        _popup.PopupClient("Deploying", uid, args.User);
                }
            },
            Text = component.Folded ? "Deploy" : "Undeploy",
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/fold.svg.192dpi.png")),
            Priority = component.Folded ? 0 : 2,
        };

        args.Verbs.Add(verb);
    }

    public static void OnShotAttempted(EntityUid uid, FoldableGunComponent comp, ref ShotAttemptedEvent args)
    {
        if (!comp.Folded)
            return;
        args.Cancel();
    }

    [Serializable, NetSerializable]
    public enum FoldedGunVisuals : byte
    {
        State
    }
}

[Serializable, NetSerializable]
public sealed partial class FoldableGunDoafterEvent : SimpleDoAfterEvent
{
    public bool Folding = true;
}
