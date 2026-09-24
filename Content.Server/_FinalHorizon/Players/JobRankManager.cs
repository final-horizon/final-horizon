using Content.Server.Database;
using Content.Shared._FinalHorizon.Players;
using Content.Shared.CCVar;
using Content.Shared.Players.JobWhitelist;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._FinalHorizon.Players;

public sealed class JobRankManager : IPostInjectInit
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    private readonly Dictionary<NetUserId, string?> _jobRanks = new();

    public void Initialize()
    {
        _net.RegisterNetMessage<MsgJobRank>();
    }

    private async Task LoadData(ICommonSession player, CancellationToken cancel)
    {
        var rank = await _db.GetJobRank(player.UserId, cancel);
        cancel.ThrowIfCancellationRequested();

        if (rank != null)
            _jobRanks[player.UserId] = rank.JobRank;
    }

    private void FinishLoad(ICommonSession player)
    {
        SendJobRank(player);
    }

    private void ClientDisconnected(ICommonSession player)
    {
        _jobRanks.Remove(player.UserId);
    }

    public async void SetJobRank(NetUserId player, ProtoId<JobRankPrototype> protoId)
    {
        if (!_prototypes.HasIndex(protoId))
            return;

        if (_jobRanks.TryGetValue(player, out _))
            _jobRanks[player] = protoId.ToString();

        await _db.SetJobRank(player, protoId.ToString());

        if (_player.TryGetSessionById(player, out var session))
            SendJobRank(session);
    }

    public bool IsAllowed(ICommonSession session, ProtoId<JobPrototype> job)
    {
        if (!_config.GetCVar(CCVars.GameRoleWhitelist))
            return true;

        if (!_prototypes.Resolve(job, out var jobPrototype) ||
            !jobPrototype.Whitelisted)
        {
            return true;
        }

        return IsWhitelisted(session.UserId, jobPrototype);
    }

    public bool IsWhitelisted(NetUserId player, JobPrototype job)
    {
        if (!_jobRanks.TryGetValue(player, out var rank))
            return false;

        if (rank == null)
            return false;

        if (!_prototypes.TryIndex<JobRankPrototype>(rank, out var rankProto))
            return false;

        List<string> ranks = new();
        ranks.Add(rank.ToString());
        if (rankProto.ParentRanks != null)
            ranks.AddRange(rankProto.ParentRanks);

        var neededRank = job.JobRank.ToString();
        if (neededRank != null && ranks.Contains(neededRank))
            return true;

        return false;
    }

    public async void RemoveWhitelist(NetUserId player)
    {
        _jobRanks[player] = null;

        await _db.SetJobRank(player, null);

        if (_player.TryGetSessionById(new NetUserId(player), out var session))
            SendJobRank(session);
    }

    private void SendJobRank(ICommonSession player)
    {
        var msg = new MsgJobRank
        {
            JobRank = _jobRanks.GetValueOrDefault(player.UserId)
        };

        _net.ServerSendMessage(msg, player.Channel);
    }

    void IPostInjectInit.PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnFinishLoad(FinishLoad);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}
