using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator
{
    public class StockPriceService
    {
        private readonly Dictionary<string, StockPriceModel> _latestPrices = new Dictionary<string, StockPriceModel>();

        public void UpdatePrice(string symbol, StockPriceModel price)
        {
            _latestPrices[symbol] = price;
        }

        public Task<StockPriceModel> GetStockPriceAsync(string symbol)
        {
            _latestPrices.TryGetValue(symbol, out var price);
            return Task.FromResult(price);
        }

        public Task<List<StockPriceModel>> GetAllStockPricesAsync()
        {
            return Task.FromResult(_latestPrices.Values.ToList());
        }
    }
}
