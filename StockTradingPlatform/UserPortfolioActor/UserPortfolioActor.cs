using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using UserPortfolioActor.Interfaces;
using Aggregator;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric.Health;
using System.Fabric;

namespace UserPortfolioActor
{
    [StatePersistence(StatePersistence.Persisted)]
    public class UserPortfolioActor : Actor, IUserPortfolioActor
    {
        private const string PortfolioStateName = "portfolio";

        public UserPortfolioActor(ActorService actorService, ActorId actorId) : base(actorService, actorId) { }

        private IAggregator GetAggregatorProxy()
        {
            return ServiceProxy.Create<IAggregator>(
                new Uri("fabric:/StockTradingPlatform/Aggregator"));
        }

        public async Task AddStockAsync(string stockSymbol, int quantity)
        {
            var portfolio = await StateManager.GetOrAddStateAsync<Dictionary<string, PortfolioItem>>(PortfolioStateName, new Dictionary<string, PortfolioItem>());

            if (portfolio.ContainsKey(stockSymbol))
            {
                portfolio[stockSymbol].Quantity += quantity;
            }
            else
            {
                portfolio[stockSymbol] = new PortfolioItem
                {
                    Symbol = stockSymbol,
                    Quantity = quantity
                };
            }

            await StateManager.SetStateAsync(PortfolioStateName, portfolio);
        }

        public async Task<Dictionary<string, PortfolioItem>> GetPortfolioAsync()
        {
            return await StateManager.GetOrAddStateAsync<Dictionary<string, PortfolioItem>>(PortfolioStateName, new Dictionary<string, PortfolioItem>());
        }

        private async Task<decimal> GetCurrentStockPriceAsync(string stockSymbol)
        {
            var aggregatorProxy = GetAggregatorProxy();

            var stockPrice = await aggregatorProxy.GetStockPriceAsync(stockSymbol);

            return stockPrice?.CurrentPrice ?? 0m;
        }

        public async Task<decimal> GetPortfolioValueAsync()
        {
            var portfolio = await GetPortfolioAsync();
            decimal totalValue = 0;

            foreach (var item in portfolio.Values)
            {
                var stockPrice = await GetCurrentStockPriceAsync(item.Symbol);
                totalValue += item.Quantity * stockPrice;
            }

            return totalValue;
        }

        public async Task RemoveStockAsync(string stockSymbol, int quantity)
        {
            var portfolio = await StateManager.GetOrAddStateAsync<Dictionary<string, PortfolioItem>>(PortfolioStateName, new Dictionary<string, PortfolioItem>());

            if (portfolio.ContainsKey(stockSymbol))
            {
                portfolio[stockSymbol].Quantity -= quantity;

                if (portfolio[stockSymbol].Quantity <= 0)
                {
                    portfolio.Remove(stockSymbol);
                }

                await StateManager.SetStateAsync(PortfolioStateName, portfolio);
            }
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "UserPortfolioActor activated.");

            return this.StateManager.TryAddStateAsync("count", 0);
        }
    }
}
