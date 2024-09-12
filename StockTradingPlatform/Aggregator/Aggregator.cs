using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using TRealTimeBroadcaster;

namespace Aggregator
{
    public sealed class Aggregator : StatelessService, IAggregator
    {
        private readonly FinnhubService _finnhubService;
        private readonly StockPriceService _stockPriceService;
        private ServiceProxyFactory _proxyFactory;

        private readonly List<string> _popularSymbols = new List<string>
        {
            "AAPL", "MSFT", "GOOG", "AMZN", "CSCO", "TSLA", "NVDA", "BRK.B", "JNJ", "V",
            "WMT", "PG", "JPM", "UNH", "MA", "DIS", "PYPL", "VZ", "ADBE", "NFLX"
        };

        public Aggregator(StatelessServiceContext context, FinnhubService finnhubService, StockPriceService stockPriceService) : base(context)
        {
            _finnhubService = finnhubService;
            _stockPriceService = stockPriceService;
            _proxyFactory = new ServiceProxyFactory(c => { return new FabricTransportServiceRemotingClientFactory(); });
        }

        private IBroadcast GetBroadcastProxy()
        {
            return _proxyFactory.CreateServiceProxy<IBroadcast>(
                new Uri("fabric:/StockTradingPlatform/TRealTimeBroadcaster"), listenerName: "ServiceEndpoint");
        }

        /*
        private async Task DefineDescription()
        {
            FabricClient fabricClient = new FabricClient();
            StatelessServiceUpdateDescription updateDescription = new();

            DefineAntiAffinity(updateDescription);

            await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, updateDescription);
        }

        private void DefineAntiAffinity(StatelessServiceUpdateDescription updateDescription)
        {
            ServiceCorrelationDescription serviceCorrelationDescription = new();
            serviceCorrelationDescription.Scheme = ServiceCorrelationScheme.NonAlignedAffinity;
            serviceCorrelationDescription.ServiceName = new Uri("fabric:/StockTradingPlatform/TradingStateful");

            updateDescription.Correlations ??= new List<ServiceCorrelationDescription>();
            updateDescription.Correlations.Add(serviceCorrelationDescription);
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            await DefineDescription();
            ServiceEventSource.Current.ServiceMessage(this.Context, $"{this.Context.NodeContext.NodeName} opened with service: Aggregator");
            await base.OnOpenAsync(cancellationToken);
        }
        */

        public async Task<StockPriceModel> GetStockPriceAsync(string symbol)
        {
            return await _stockPriceService.GetStockPriceAsync(symbol);
        }

        public async Task<List<StockPriceModel>> GetAllStockPricesAsync()
        {
            return await _stockPriceService.GetAllStockPricesAsync();
        }

        public Task<List<string>> GetSupportedSymbols()
        {
            return Task.FromResult(_popularSymbols);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var broadcastProxy = GetBroadcastProxy();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var stockPrices = new Dictionary<string, StockPriceModel>();

                    foreach (var symbol in _popularSymbols)
                    {
                        try
                        {
                            var price = await _finnhubService.GetStockPriceAsync(symbol);
                            _stockPriceService.UpdatePrice(symbol, price);

                            ServiceEventSource.Current.ServiceMessage(this.Context, $"The latest price for {symbol} is {price.CurrentPrice:C}");
                            stockPrices[symbol] = price;
                        }
                        catch (Exception ex)
                        {
                            ServiceEventSource.Current.ServiceMessage(this.Context, $"Error fetching price for {symbol}: {ex.Message}");
                        }
                    }

                    var aggregatedJson = JsonSerializer.Serialize(stockPrices);
                    await broadcastProxy.SendStockUpdates(aggregatedJson);

                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"RunAsync encountered an unexpected error: {ex.Message}");
            }
        }
    }
}
