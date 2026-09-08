using System.Numerics;
using Content.Client.Movement.Components;
using Content.Client.Movement.Systems;
using Content.Shared.Camera;
using Content.Shared.Hands;
using Content.Shared.Movement.Components;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;
using Robust.Client.Timing;

namespace Content.Client.Wieldable;

public sealed class WieldableSystem : SharedWieldableSystem
{
    //[Dependency] private readonly EyeCursorOffsetSystem _eyeOffset = default!; // FH
    [Dependency] private readonly IClientGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursorOffsetRequiresWieldComponent, ItemUnwieldedEvent>(OnEyeOffsetUnwielded);
        SubscribeLocalEvent<CursorOffsetRequiresWieldComponent, ItemWieldedEvent>(OnGetEyeOffset); // FH
    }

    public void OnEyeOffsetUnwielded(Entity<CursorOffsetRequiresWieldComponent> entity, ref ItemUnwieldedEvent args)
    {
        if (!TryComp(args.User, out EyeCursorOffsetComponent? comp)) // FH
            return;

        if (_gameTiming.IsFirstTimePredicted)
        {
            comp.UseItem = false; // FH
        }
    }

    public void OnGetEyeOffset(Entity<CursorOffsetRequiresWieldComponent> entity, ref ItemWieldedEvent args) // FH start
    {
        if (!TryComp(entity.Owner, out WieldableComponent? wieldableComp))
            return;

        if (!wieldableComp.Wielded)
            return;

        if (!TryComp(args.User, out EyeCursorOffsetComponent? comp))
            return;

        if (_gameTiming.IsFirstTimePredicted)
        {
            comp.UseItem = true;
        } // FH end
    }
}
