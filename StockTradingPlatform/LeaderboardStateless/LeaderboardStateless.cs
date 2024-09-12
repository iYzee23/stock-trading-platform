using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using UserProfile;
using UserPortfolioActor.Interfaces;
using System.Fabric.Health;
using System.Text.Json;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using TRealTimeBroadcaster;

namespace LeaderboardStateless
{
    public sealed class LeaderboardStateless : StatelessService, ILeaderboard
    {
        private ServiceProxyFactory _proxyFactory;
        private readonly SortedList<decimal, List<string>> _leaderboard = new SortedList<decimal, List<string>>(Comparer<decimal>.Create((x, y) => y.CompareTo(x)));

        public LeaderboardStateless(StatelessServiceContext context) : base(context)
        {
            _proxyFactory = new ServiceProxyFactory(c => { return new FabricTransportServiceRemotingClientFactory(); });
        }

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

        private IBroadcast GetBroadcastProxy()
        {
            return _proxyFactory.CreateServiceProxy<IBroadcast>(
                new Uri("fabric:/StockTradingPlatform/TRealTimeBroadcaster"), listenerName: "ServiceEndpoint");
        }

        public Task<List<KeyValuePair<decimal, List<string>>>> GetLeaderboardAsync()
        {
            var orderedLeaderboard = _leaderboard.ToList();
            return Task.FromResult(orderedLeaderboard);
        }

        private async Task UpdateLeaderboardAsync()
        {
            var userProfileService = GetUserProfileProxy();

            _leaderboard.Clear();

            try
            {
                var userProfiles = await userProfileService.GetAllUserProfilesAsync();

                foreach (var userProfile in userProfiles)
                {
                    try
                    {
                        var userPortfolioProxy = GetUserPortfolioProxy(userProfile.Username);
                        var portfolioValue = await userPortfolioProxy.GetPortfolioValueAsync();

                        if (_leaderboard.ContainsKey(portfolioValue))
                        {
                            _leaderboard[portfolioValue].Add(userProfile.Name);
                        }
                        else
                        {
                            _leaderboard[portfolioValue] = new List<string> { userProfile.Name };
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceEventSource.Current.ServiceMessage(this.Context, $"Error processing portfolio for {userProfile.Name}: {ex.Message}");
                    }
                }

                ServiceEventSource.Current.ServiceMessage(this.Context, "Leaderboard Updated:");
                foreach (var entry in _leaderboard)
                {
                    foreach (var name in entry.Value)
                    {
                        ServiceEventSource.Current.ServiceMessage(this.Context, $"{name}: {entry.Key:C}");
                    }
                }

                var broadcastProxy = GetBroadcastProxy();

                var aggregatedJson = JsonSerializer.Serialize(_leaderboard.ToList());
                await broadcastProxy.SendLeaderboardUpdates(aggregatedJson);
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Error updating leaderboard: {ex.Message}. Will retry in the next cycle.");
            }
        }

        public async Task TriggerLeaderboardUpdateAsync()
        {
            await UpdateLeaderboardAsync();
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }
    }
}
