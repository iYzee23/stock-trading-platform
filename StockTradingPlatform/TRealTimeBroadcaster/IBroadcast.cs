using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

namespace TRealTimeBroadcaster
{
    public interface IBroadcast : IService
    {
        public Task SendStockUpdates(string aggregatedJson);

        public Task SendInitialStockUpdates(string connectionId);

        public Task SendLeaderboardUpdates(string aggregatedJson);

        public Task SendInitialLeaderboardUpdates(string aggregatedJson);
    }
}
