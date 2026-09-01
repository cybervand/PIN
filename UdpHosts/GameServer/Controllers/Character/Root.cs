using Aero.Protocol;
using Serilog;

namespace GameServer.Controllers.Character;

[Typecode(GssTables.Ns.Character)]
public class Root : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
    }
}