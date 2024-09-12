using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStateful
{
    public interface ITrading : IService
    {
        Task<bool> BuyStockAsync(string userId, string stockSymbol, int quantity);
        
        Task<bool> SellStockAsync(string userId, string stockSymbol, int quantity);
        
        Task<List<TransactionRecord>> GetTransactionHistoryAsync(string userId);
        
        Task EnqueueOrderAsync(StockOrder order);
    }
}
