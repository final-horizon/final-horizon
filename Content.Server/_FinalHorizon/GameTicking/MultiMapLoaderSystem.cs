using Content.Server._FinalHorizon.GameTicking;
using Content.Server.GameTicking.Events;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;

public sealed partial class MultiMapLoaderSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent args)
    {
        var comps = EntityQuery<MultiMapLoaderComponent>();

        foreach (var comp in comps)
        {
            foreach (var map in comp.Maps)
            {
                _loader.TryLoadMap(map, out var loadedMap, out var _);
                if (loadedMap != null)
                    _mapSystem.InitializeMap(loadedMap.Value.Comp.MapId);
            }
        }
    }
}
