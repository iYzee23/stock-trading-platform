using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderboardStateless
{
    public interface ILeaderboard : IService
    {
        Task<List<KeyValuePair<decimal, List<string>>>> GetLeaderboardAsync();

        Task TriggerLeaderboardUpdateAsync();
    }
}
