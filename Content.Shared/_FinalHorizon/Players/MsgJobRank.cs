using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._FinalHorizon.Players;

public sealed class MsgJobRank : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public string? JobRank = null;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var rank = buffer.ReadString();

        JobRank = rank;
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(JobRank);
    }
}
