using Aero.Protocol;
using GameServer.Packets;
using Serilog;

namespace GameServer.Controllers.Character;

[Typecode(GssCharacterView.MissionAndMarkerController)]
public class MissionAndMarkerController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
    }

    [MessageID(GssCharacterCommand.RequestAllAchievements)]
    public void RequestAllAchievements(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID(GssCharacterCommand.TryResumeTutorialChain)]
    public void TryResumeTutorialChain(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }
}