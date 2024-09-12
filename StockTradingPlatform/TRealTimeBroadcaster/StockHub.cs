using Microsoft.AspNetCore.SignalR;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System.Text.Json;

namespace TRealTimeBroadcaster
{
    public class StockHub : Hub
    {
        private ServiceProxyFactory _proxyFactory;

        public StockHub()
        {
            _proxyFactory = new ServiceProxyFactory(c => { return new FabricTransportServiceRemotingClientFactory(); });
        }

        private IBroadcast GetBroadcastProxy()
        {
            return _proxyFactory.CreateServiceProxy<IBroadcast>(
                new Uri("fabric:/StockTradingPlatform/TRealTimeBroadcaster"), listenerName: "ServiceEndpoint");
        }

        public override async Task OnConnectedAsync()
        {
            var broadcastProxy = GetBroadcastProxy();

            var connectionId = Context.ConnectionId;
            await broadcastProxy.SendInitialStockUpdates(connectionId);
            await broadcastProxy.SendInitialLeaderboardUpdates(connectionId);
            await base.OnConnectedAsync();
        }
    }
}
