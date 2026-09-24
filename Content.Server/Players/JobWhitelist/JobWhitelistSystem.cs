using Content.Server._FinalHorizon.Players;
using Content.Server.GameTicking.Events;
using Content.Server.Station.Events;
using Content.Shared.CCVar;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Collections.Immutable;

namespace Content.Server.Players.JobWhitelist;

public sealed class JobWhitelistSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly JobWhitelistManager _manager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly JobRankManager _ranks = default!; // FH

    private ImmutableArray<ProtoId<JobPrototype>> _whitelistedJobs = [];

    public override void Initialize()
    {
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        SubscribeLocalEvent<StationJobsGetCandidatesEvent>(OnStationJobsGetCandidates);
        SubscribeLocalEvent<IsRoleAllowedEvent>(OnIsRoleAllowed);
        SubscribeLocalEvent<GetDisallowedJobsEvent>(OnGetDisallowedJobs);

        CacheJobs();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (ev.WasModified<JobPrototype>())
            CacheJobs();
    }

    private void OnStationJobsGetCandidates(ref StationJobsGetCandidatesEvent ev)
    {
        if (!_config.GetCVar(CCVars.GameRoleWhitelist))
            return;

        for (var i = ev.Jobs.Count - 1; i >= 0; i--)
        {
            var jobId = ev.Jobs[i];
            if (_player.TryGetSessionById(ev.Player, out var player)) // FH start
            {
                if (!_manager.IsAllowed(player, jobId) && !_ranks.IsAllowed(player, jobId))
                {
                    ev.Jobs.RemoveSwap(i);
                } // FH end
            }
        }
    }

    private void OnIsRoleAllowed(ref IsRoleAllowedEvent ev)
    {
        if (ev.Jobs is null)
            return;

        foreach (var proto in ev.Jobs)
        {
            if (!_manager.IsAllowed(ev.Player, proto) && !_ranks.IsAllowed(ev.Player, proto)) // FH
                ev.Cancelled = true;
        }
    }
    //TODO: Antagonist role whitelists?

    private void OnGetDisallowedJobs(ref GetDisallowedJobsEvent ev)
    {
        if (!_config.GetCVar(CCVars.GameRoleWhitelist))
            return;

        foreach (var job in _whitelistedJobs)
        {
            if (!_manager.IsAllowed(ev.Player, job) && !_ranks.IsAllowed(ev.Player, job)) // FH
                ev.Jobs.Add(job);
        }
    }

    private void CacheJobs()
    {
        var builder = ImmutableArray.CreateBuilder<ProtoId<JobPrototype>>();
        foreach (var job in _prototypes.EnumeratePrototypes<JobPrototype>())
        {
            if (job.Whitelisted)
                builder.Add(job.ID);
        }

        _whitelistedJobs = builder.ToImmutable();
    }
}
