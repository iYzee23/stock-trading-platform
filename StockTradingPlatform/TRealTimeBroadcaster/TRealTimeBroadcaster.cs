using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;

namespace TRealTimeBroadcaster
{
    public sealed class TRealTimeBroadcaster : StatelessService, IBroadcast
    {
        private IHubContext<StockHub> _stockHub;
        private string aggregatedStockJson;
        private string aggregatedLeaderboardJson;

        public TRealTimeBroadcaster(StatelessServiceContext context) : base(context) { }

        public async Task SendStockUpdates(string aggregatedJson)
        {
            aggregatedStockJson = aggregatedJson;
            await _stockHub.Clients.All.SendAsync("StockUpdates", aggregatedJson);
        }

        public async Task SendInitialStockUpdates(string connectionId)
        {
            await _stockHub.Clients.Client(connectionId).SendAsync("InitialStockUpdates", aggregatedStockJson);
        }

        public async Task SendLeaderboardUpdates(string aggregatedJson)
        {
            aggregatedLeaderboardJson = aggregatedJson;
            await _stockHub.Clients.All.SendAsync("LeaderboardUpdates", aggregatedJson);
        }

        public async Task SendInitialLeaderboardUpdates(string connectionId)
        {
            await _stockHub.Clients.Client(connectionId).SendAsync("InitialLeaderboardUpdates", aggregatedLeaderboardJson);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new FabricTransportServiceRemotingListener(serviceContext, this,
                    new FabricTransportRemotingListenerSettings() { EndpointResourceName = "ServiceEndpoint" }),
                    "ServiceEndpoint"),

                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "SignalREndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddCors(options =>
                        {
                            options.AddDefaultPolicy(policy =>
                            {
                                /*
                                policy.WithOrigins(
                                    "http://localhost:19081", "http://mdcssql-prepes2:19081", "http://CPC-pesic-9JQ9J:19081",
                                    "http://localhost:4200", "http://mdcssql-prepes2:4200", "http://CPC-pesic-9JQ9J:4200"
                                    )
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials();
                                */

                                policy.SetIsOriginAllowed(origin =>
                                    {
                                        var allowedOrigins = new[] { "localhost", "mdcssql-prepes2", "CPC-pesic-9JQ9J" };
                                        var requestHost = new Uri(origin).Host;
                                        return allowedOrigins.Contains(requestHost);
                                    })
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials();
                            });
                        });

                        builder.Services.AddSignalR();
                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);

                        var app = builder.Build();

                        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                        {
                            app.UseDeveloperExceptionPage();
                        }

                        app.UseCors();

                        app.UseStaticFiles();
                        app.UseRouting();
                        app.MapHub<StockHub>("/stockHub");
                        app.UseAuthorization();

                        _stockHub = app.Services.GetService<IHubContext<StockHub>>();

                        return app;
                    }), "SignalREndpoint")
            };
        }
    }
}
