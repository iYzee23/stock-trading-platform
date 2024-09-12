using System;
using System.Collections;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aggregator;
using LeaderboardStateless;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using UserPortfolioActor.Interfaces;
using UserProfile;

namespace TradingStateful
{
    public sealed class TradingStateful : StatefulService, ITrading, IInit
    {
        private const string OrderQueueName = "orderQueue";
        private const string TransactionHistoryDictionaryName = "transactionHistory";

        public TradingStateful(StatefulServiceContext context) : base(context) { }

        private IUserProfile GetUserProfileProxy()
        {
            return ServiceProxy.Create<IUserProfile>(
                new Uri("fabric:/StockTradingPlatform/UserProfile"), new ServicePartitionKey(0));
        }

        private IUserPortfolioActor GetUserPortfolioProxy(string username)
        {
            return ActorProxy.Create<IUserPortfolioActor>(
                new ActorId(username), new Uri("fabric:/StockTradingPlatform/UserPortfolioActorService"));
        }

        private IAggregator GetAggregatorProxy() 
        {
            return ServiceProxy.Create<IAggregator>(
                new Uri("fabric:/StockTradingPlatform/Aggregator"));
        }

        private ILeaderboard GetLeaderboardProxy()
        {
            return ServiceProxy.Create<ILeaderboard>(
                new Uri("fabric:/StockTradingPlatform/LeaderboardStateless"));
        }

        public async Task<bool> BuyStockAsync(string username, string stockSymbol, int quantity)
        {
            try
            {
                var userProfileProxy = GetUserProfileProxy();
                var userPortfolioProxy = GetUserPortfolioProxy(username);

                var userBalance = await userProfileProxy.GetUserBalanceAsync(username);
                var currentPrice = await GetCurrentStockPriceAsync(stockSymbol);
                var totalCost = currentPrice * quantity;

                if (userBalance < totalCost)
                {
                    return false;
                }

                await userProfileProxy.UpdateUserBalanceAsync(username, -totalCost);
                await userPortfolioProxy.AddStockAsync(stockSymbol, quantity);

                return true;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Buy stock failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SellStockAsync(string username, string stockSymbol, int quantity)
        {
            try
            {
                var userProfileProxy = GetUserProfileProxy();
                var userPortfolioProxy = GetUserPortfolioProxy(username);

                var portfolio = await userPortfolioProxy.GetPortfolioAsync();
                if (!portfolio.ContainsKey(stockSymbol) || portfolio[stockSymbol].Quantity < quantity)
                {
                    return false;
                }

                var currentPrice = await GetCurrentStockPriceAsync(stockSymbol);
                var totalRevenue = currentPrice * quantity;

                await userProfileProxy.UpdateUserBalanceAsync(username, totalRevenue);
                await userPortfolioProxy.RemoveStockAsync(stockSymbol, quantity);

                return true;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Sell stock failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TransactionRecord>> GetTransactionHistoryAsync(string username)
        {
            var transactionHistory = await StateManager.GetOrAddAsync<IReliableDictionary<string, List<TransactionRecord>>>(TransactionHistoryDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                var historyResult = await transactionHistory.TryGetValueAsync(tx, username);
                await tx.CommitAsync();

                return historyResult.HasValue ? historyResult.Value : new List<TransactionRecord>();
            }
        }

        private async Task<decimal> GetCurrentStockPriceAsync(string stockSymbol)
        {
            var aggregatorProxy = GetAggregatorProxy();

            var stockPrice = await aggregatorProxy.GetStockPriceAsync(stockSymbol);
            return stockPrice?.CurrentPrice ?? 0m;
        }

        private async Task RecordTransactionAsync(ITransaction tx, StockOrder order)
        {
            var transactionHistory = await StateManager.GetOrAddAsync<IReliableDictionary<string, List<TransactionRecord>>>(TransactionHistoryDictionaryName);

            var userHistory = await transactionHistory.TryGetValueAsync(tx, order.Username);
            if (!userHistory.HasValue)
            {
                userHistory = new ConditionalValue<List<TransactionRecord>>(true, new List<TransactionRecord>());
            }

            var transactionRecord = new TransactionRecord
            {
                TransactionId = order.OrderId,
                StockSymbol = order.StockSymbol,
                Quantity = order.Quantity,
                Price = await GetCurrentStockPriceAsync(order.StockSymbol),
                IsBuy = order.OrderType == "Buy",
                Timestamp = DateTime.UtcNow
            };

            userHistory.Value.Add(transactionRecord);
            await transactionHistory.SetAsync(tx, order.Username, userHistory.Value);

            var leaderboardProxy = GetLeaderboardProxy();
            await leaderboardProxy.TriggerLeaderboardUpdateAsync();
        }

        public async Task EnqueueOrderAsync(StockOrder order)
        {
            var queue = await StateManager.GetOrAddAsync<IReliableQueue<StockOrder>>(OrderQueueName);

            using (var tx = StateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, order);
                await tx.CommitAsync();
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Enqueued order: {order.OrderId} for {order.Quantity} shares of {order.StockSymbol}.");
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        private async Task CreateAndEnqueueOrdersAsync(IReliableQueue<StockOrder> queue, string country)
        {
            var orders_srb = new[]
            {
                // predrag.pesic
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "predrag.pesic", StockSymbol = "MSFT", Quantity = 2, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "predrag.pesic", StockSymbol = "MSFT", Quantity = 3, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "predrag.pesic", StockSymbol = "MSFT", Quantity = 5, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "predrag.pesic", StockSymbol = "MSFT", Quantity = 5, OrderType = "Sell" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "predrag.pesic", StockSymbol = "MSFT", Quantity = 5, OrderType = "Buy" },

                // slavica.ivanovic
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "slavica.ivanovic", StockSymbol = "NVDA", Quantity = 2, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "slavica.ivanovic", StockSymbol = "NVDA", Quantity = 3, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "slavica.ivanovic", StockSymbol = "NVDA", Quantity = 10, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "slavica.ivanovic", StockSymbol = "NVDA", Quantity = 5, OrderType = "Sell" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "slavica.ivanovic", StockSymbol = "NVDA", Quantity = 1, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "slavica.ivanovic", StockSymbol = "NVDA", Quantity = 4, OrderType = "Buy" }
            };

            var orders_mne = new[]
            {
                // milica.karic
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "milica.karic", StockSymbol = "AAPL", Quantity = 2, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "milica.karic", StockSymbol = "AAPL", Quantity = 2, OrderType = "Sell" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "milica.karic", StockSymbol = "AAPL", Quantity = 10, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "milica.karic", StockSymbol = "AAPL", Quantity = 5, OrderType = "Buy" },

                // zorka.samardzic
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "zorka.samardzic", StockSymbol = "TSLA", Quantity = 5, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "zorka.samardzic", StockSymbol = "TSLA", Quantity = 1, OrderType = "Buy" }
            };

            var orders_bih = new[]
            {
                // dragan.karic
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "dragan.karic", StockSymbol = "GOOG", Quantity = 5, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "dragan.karic", StockSymbol = "GOOG", Quantity = 4, OrderType = "Sell" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "dragan.karic", StockSymbol = "GOOG", Quantity = 1, OrderType = "Sell" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "dragan.karic", StockSymbol = "GOOG", Quantity = 5, OrderType = "Buy" },

                // ranko.pesic
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "ranko.pesic", StockSymbol = "AMZN", Quantity = 5, OrderType = "Buy" },
                new StockOrder { OrderId = Guid.NewGuid().ToString(), Username = "ranko.pesic", StockSymbol = "AMZN", Quantity = 5, OrderType = "Buy" }
            };

            foreach (var order in (country == "SRB" ? orders_srb : (country == "MNE" ? orders_mne : orders_bih)))
            {
                await EnqueueOrderAsync(order);
            }
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var queue = await StateManager.GetOrAddAsync<IReliableQueue<StockOrder>>(OrderQueueName);

            while (!cancellationToken.IsCancellationRequested)
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var result = await queue.TryDequeueAsync(tx);
                    if (result.HasValue)
                    {
                        var order = result.Value;
                        bool operationSuccess = false;

                        if (order.OrderType == "Buy")
                        {
                            operationSuccess = await BuyStockAsync(order.Username, order.StockSymbol, order.Quantity);
                        }
                        else if (order.OrderType == "Sell")
                        {
                            operationSuccess = await SellStockAsync(order.Username, order.StockSymbol, order.Quantity);
                        }

                        ServiceEventSource.Current.ServiceMessage(this.Context, $"{order.OrderType} operation successful: {operationSuccess}");

                        if (operationSuccess)
                        {
                            await RecordTransactionAsync(tx, order);
                        }
                    }
                    await tx.CommitAsync();
                }
                // await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public async Task OnUserInitializationCompletedAsync(string country)
        {
            var queue = await StateManager.GetOrAddAsync<IReliableQueue<StockOrder>>(OrderQueueName);
            await CreateAndEnqueueOrdersAsync(queue, country);
        }
    }
}
