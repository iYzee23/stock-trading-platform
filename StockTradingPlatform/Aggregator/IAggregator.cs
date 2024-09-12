using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator
{
    public interface IAggregator : IService
    {
        Task<StockPriceModel> GetStockPriceAsync(string symbol);

        Task<List<StockPriceModel>> GetAllStockPricesAsync();

        Task<List<string>> GetSupportedSymbols();
    }
}
