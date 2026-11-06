using Content.Shared.Body;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared_FinalHorizon.Damage;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._FinalHorizon.Damage;

public sealed partial class WoundSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodyComponent, ExaminedEvent>(OnBodyExamine);
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
        var realDamage = damage.GetTotal() * _rand.NextFloat(1, 3f);

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
        CreateWound(organ, realDamage, woundType);
        _popup.PopupPredicted($"Applied wound to organ {organ.Owner.ToString()} with type {woundType} and damage {realDamage}", ent, ent);
        return true;
    }
    public void CreateWound(EntityUid ent, FixedPoint2 damage, ProtoId<WoundTypePrototype> woundType)
    {
        if (TryComp<WoundableComponent>(ent, out var woundable))
        {
            var wound = new Wound
            {
                Damage = damage,
                WoundType = woundType
            };
            woundable.Wounds.Add(wound);
            woundable.Dirty();
        }
    }

    public void OnBodyExamine(EntityUid ent, BodyComponent comp, ref ExaminedEvent args)
    {
        var msg = new FormattedMessage();
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
                    msg.PushNewline();
                    msg.PushColor(Color.Red);
                    msg.AddText($"       - {wound.WoundType}");
                    msg.Pop();
                }
            }
            else
                msg.AddText(" is ok");
        }
        msg.PushNewline();
        args.AddMessage(msg);
    }
}
