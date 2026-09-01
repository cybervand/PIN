using Aero.Protocol;
using AeroMessages.GSS.Character.Command;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;

namespace GameServer.Controllers.Character;

[Typecode(GssCharacterView.SpectatorController)]
public class SpectatorController : Base
{
    private ILogger _logger;

    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        _logger = logger;
    }

    [MessageID(GssCharacterCommand.PerformTextChat)]
    public void PerformTextChat(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<PerformTextChat>();
        var character = player.CharacterEntity;
        var shard = player.CharacterEntity.Shard;
        shard.Chat.CharacterPerformTextChat(client, character, query);
    }
}