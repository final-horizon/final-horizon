using Content.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._FinalHorizon.Aim;

public sealed partial class SharedAimSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
    .Bind(ContentKeyFunctions.ToggleAim, InputCmdHandler.FromDelegate(HandleAimToggle, handle: false))
    .Register<SharedAimSystem>();
    }

    private void HandleAimToggle(ICommonSession? session)
    {
        if (session is not { } playerSession)
            return;

        if (playerSession.AttachedEntity is not { Valid: true } playerEnt || !Exists(playerEnt))
            return;

        ToggleAim(playerEnt);
    }

    public void ToggleAim(EntityUid player)
    {
        if (!TryComp<AimComponent>(player, out var comp))
            return;

        if (_gameTiming.IsFirstTimePredicted || _net.IsServer)
            comp.Aiming = !comp.Aiming;
    }
}
