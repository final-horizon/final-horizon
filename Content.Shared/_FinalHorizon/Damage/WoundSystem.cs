using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.HealthExaminable;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared_FinalHorizon.Damage;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;
using System.Security.Cryptography;

namespace Content.Shared._FinalHorizon.Damage;

public sealed partial class WoundSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public Dictionary<TargetableBodyParts, ProtoId<OrganCategoryPrototype>> TargetableOrgans = new()
    {
        [TargetableBodyParts.Head] = "Head",
        [TargetableBodyParts.Torso] = "Torso",
        [TargetableBodyParts.LeftArm] = "ArmLeft",
        [TargetableBodyParts.LeftHand] = "HandLeft",
        [TargetableBodyParts.RightArm] = "ArmRight",
        [TargetableBodyParts.RightHand] = "HandRight",
        [TargetableBodyParts.LeftLeg] = "LegLeft",
        [TargetableBodyParts.LeftFoot] = "FootLeft",
        [TargetableBodyParts.RightLeg] = "LegRight",
        [TargetableBodyParts.RightFoot] = "FootRight",
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodyComponent, HealthBeingExaminedEvent>(OnBodyExamine);
        SubscribeLocalEvent<WoundTopicalComponent, AfterInteractEvent>(OnTryToUseWoundTopical);
    }
    public bool HandleGunshotDamage(EntityUid ent, DamageSpecifier damage)
    {
        // predict this system later
        if (_net.IsClient)
            return false;

        if (!_bodySystem.TryGetOrgansWithComponent<WoundableComponent>(ent, out var organs))
            return false;

        // Select the organ to apply the wound to here
        var organ = _rand.Pick(organs);

        // Simulate weaker and stronger attacks here until a better system is made
        var realDamage = damage.GetTotal() * _rand.NextFloat(0.2f, 3f);

        ProtoId<WoundTypePrototype> woundType = "BleedLight";
        switch (realDamage.Float())
        {
            case > 150:
                // delimb
                _popup.PopupPredicted($"Delimbing organ {organ} with damage {realDamage}", ent, ent);
                return true;

            case > 80:
                woundType = "BleedVeryHeavy";
                break;

            case > 50:
                woundType = "BleedHeavy";
                break;

            case > 25:
                woundType = "BleedMedium";
                break;

            case < 25:
                woundType = "BleedLight";
                break;
        }
        ;
        CreateWound(organ, ent, realDamage, woundType);
        if (TryComp<OrganComponent>(organ.Owner, out var organComp))
            _popup.PopupPredicted($"{organComp.Category} | {woundType} | {realDamage}", ent, ent);
        return true;
    }
    public void CreateWound(EntityUid ent, EntityUid bodyEnt, FixedPoint2 damage, ProtoId<WoundTypePrototype> woundType)
    {
        if (TryComp<WoundableComponent>(ent, out var woundable) && _proto.TryIndex<WoundTypePrototype>(woundType, out _))
        {
            var wound = new Wound
            {
                Damage = damage,
                WoundType = woundType
            };
            woundable.Wounds.Add(wound);
            UpdateBleedingFromWounds(bodyEnt);
            DirtyField<WoundableComponent>(ent, "Wounds");
            if (_net.IsServer)
                EnsureComp<BodyHasWoundsComponent>(bodyEnt);
        }
    }

    public void RemoveWound(EntityUid bodyEnt, WoundableComponent comp, int woundIndex)
    {
        comp.Wounds.RemoveAt(woundIndex);
        UpdateBleedingFromWounds(bodyEnt);
        DirtyField<WoundableComponent>(comp.Owner, "Wounds");
        if (_net.IsServer)
        {
            if (_bodySystem.TryGetOrgansWithComponent<WoundableComponent>(bodyEnt, out var organs))
            {
                var hasWounds = false;
                foreach (var organ in organs)
                {
                    if (organ.Comp.Wounds.Count > 0)
                    {
                        hasWounds = true;
                        break;
                    }
                }

                if (!hasWounds)
                    RemCompDeferred<BodyHasWoundsComponent>(bodyEnt);
            }
        }
    }

    public void TendWound(EntityUid bodyEnt, WoundableComponent comp, int woundIndex)
    {
        comp.Wounds[woundIndex].Tended = true;
        UpdateBleedingFromWounds(bodyEnt);
        DirtyField<WoundableComponent>(comp.Owner, "Wounds");
    }

    // Updates bleed caused by wounds
    private void UpdateBleedingFromWounds(EntityUid uid)
    {
        if (!TryComp<BodyComponent>(uid, out var body) ||
            !_bodySystem.TryGetOrgansWithComponent<WoundableComponent>(uid, out var organs) ||
            !TryComp<BloodstreamComponent>(uid, out var bloodStream))
            return;

        var bleed = 0f;
        foreach (var organ in organs)
        {
            foreach (var wound in organ.Comp.Wounds)
            {
                if (_proto.TryIndex(wound.WoundType, out var woundProto) && !wound.Tended)
                    bleed += woundProto.BleedRate;
            }
        }

        bloodStream.BleedFromWounds = bleed;
        DirtyField<BloodstreamComponent>(uid, "BleedFromWounds");
        _bloodstream.TryModifyBleedAmount(uid, 0);
    }

    public void OnBodyExamine(EntityUid ent, BodyComponent comp, ref HealthBeingExaminedEvent args)
    {
        var msg = new FormattedMessage();
        msg.PushNewline();
        msg.AddText("Current wounds are:");
        if (!_bodySystem.TryGetOrgansWithComponent<WoundableComponent>(ent, out var organs))
            return;

        foreach (var organ in organs)
        {
            msg.PushNewline();
            msg.AddText($"- {Name(organ.Owner)}");

            if (organ.Comp.Wounds.Count > 0)
            {
                msg.AddText(" has:");
                foreach (var wound in organ.Comp.Wounds)
                {
                    if (_proto.TryIndex<WoundTypePrototype>(wound.WoundType, out var woundType))
                    {
                        msg.PushNewline();
                        if (wound.Tended)
                        {
                            msg.PushColor(Color.LightBlue);
                            msg.AddText($"       - {wound.WoundType} (Tended)");
                        }
                        else
                        {
                            msg.PushColor(woundType.Color);
                            msg.AddText($"       - {wound.WoundType}");
                        }
                        msg.Pop();
                    }
                }
            }
            else
                msg.AddText(" is ok");
        }
        msg.PushNewline();
        args.Message.AddMessage(msg);
    }

    public void OnTryToUseWoundTopical(EntityUid uid, WoundTopicalComponent comp, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (!TryComp<BodyComponent>(args.Target, out var body) ||
            !_bodySystem.TryGetOrgansWithComponent<WoundableComponent>(args.Target.Value, out var organs))
            return;

        var targetedOrganCatagory = "Torso";
        if (TryComp<BodyPartTargetingComponent>(args.User, out var targetingComp))
        {
            targetedOrganCatagory = TargetableOrgans[targetingComp.TargetedPart];
        }

        foreach (var organ in organs)
        {
            if (TryComp<OrganComponent>(organ, out var organComp) && organComp.Category == targetedOrganCatagory)
            {
                for (var i = organ.Comp.Wounds.Count - 1; i >= 0; i--)
                {
                    var wound = organ.Comp.Wounds[i];
                    if (wound != null && !wound.Tended && comp.UsableWounds.Contains(wound.WoundType))
                    {
                        var woundType = wound.WoundType;
                        TendWound(args.Target.Value, organ.Comp, i);
                        _popup.PopupPredicted($"Healed the {woundType.Id} wound on the {targetedOrganCatagory}", args.User, args.User);
                        return;
                    }
                }
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var curTime = _gameTiming.CurTime;

        var query = EntityQuery<BodyHasWoundsComponent>();
        foreach (var ent in query)
        {
            if (ent.NextCheck > curTime)
                continue;

            if (ent.NextCheck == TimeSpan.Zero)
            {
                ent.NextCheck = curTime + ent.CheckInverval;
                continue;
            }

            if (!_bodySystem.TryGetOrgansWithComponent<WoundableComponent>(ent.Owner, out var organs))
                continue;

            foreach (var organ in organs)
            {
                for (var i = organ.Comp.Wounds.Count - 1; i >= 0; i--)
                {
                    var wound = organ.Comp.Wounds[i];

                    if (wound == null || !_proto.TryIndex<WoundTypePrototype>(wound.WoundType, out var woundType))
                        continue;

                    if (wound.Tended)
                    {
                        if (_rand.NextFloat() < woundType.TendedHealChance)
                            RemoveWound(ent.Owner, organ, i);
                    }
                    else
                    {
                        if (_rand.NextFloat() < woundType.SelfTendChance)
                            TendWound(ent.Owner, organ, i);
                    }
                }
            }
            ent.NextCheck = curTime + ent.CheckInverval;
        }
    }
}
