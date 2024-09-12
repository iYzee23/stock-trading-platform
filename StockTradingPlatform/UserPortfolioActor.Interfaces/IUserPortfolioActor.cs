using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace UserPortfolioActor.Interfaces
{
    public interface IUserPortfolioActor : IActor
    {
        Task AddStockAsync(string stockSymbol, int quantity);

        Task RemoveStockAsync(string stockSymbol, int quantity);
        
        Task<Dictionary<string, PortfolioItem>> GetPortfolioAsync();
        
        Task<decimal> GetPortfolioValueAsync();
    }
}
