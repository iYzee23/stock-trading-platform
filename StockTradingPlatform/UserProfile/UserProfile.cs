using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using UserPortfolioActor.Interfaces;
using System.Fabric.Health;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Diagnostics.Metrics;
using System.Fabric.Description;

namespace UserProfile
{
    public class UserProfile : StatefulService, IUserProfile
    {
        private const string UserProfilesDictionaryName = "userProfiles";

        public UserProfile(StatefulServiceContext context) : base(context) { }

        private IInit GetTradingInitProxy(string country)
        {
            return ServiceProxy.Create<IInit>(
                new Uri("fabric:/StockTradingPlatform/TradingStateful"), new ServicePartitionKey(country));
        }

        /*
        private async Task DefineDescription()
        {
            FabricClient fabricClient = new FabricClient();
            StatefulServiceUpdateDescription updateDescription = new();

            DefineAffinity(updateDescription);

            await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, updateDescription);
        }

        private void DefineAffinity(StatefulServiceUpdateDescription updateDescription)
        {
            ServiceCorrelationDescription serviceCorrelationDescription = new();
            serviceCorrelationDescription.Scheme = ServiceCorrelationScheme.Affinity;
            serviceCorrelationDescription.ServiceName = new Uri("fabric:/StockTradingPlatform/UserPortfolioActorService");

            updateDescription.Correlations ??= new List<ServiceCorrelationDescription>();
            updateDescription.Correlations.Add(serviceCorrelationDescription);
        }

        protected override async Task OnOpenAsync(ReplicaOpenMode openMode, CancellationToken cancellationToken)
        {
            await DefineDescription();
            ServiceEventSource.Current.ServiceMessage(this.Context, $"{this.Context.NodeContext.NodeName} opened with service: UserProfile");
            await base.OnOpenAsync(openMode, cancellationToken);
        }
        */

        public async Task<UserProfileModel> GetUserProfileAsync(string username)
        {
            var profiles = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfileModel>>(UserProfilesDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                var userProfileResult = await profiles.TryGetValueAsync(tx, username);
                await tx.CommitAsync();

                return userProfileResult.HasValue ? userProfileResult.Value : null;
            }
        }

        public async Task AddOrUpdateUserProfileAsync(string username, UserProfileModel profile)
        {
            var profiles = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfileModel>>(UserProfilesDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                await profiles.AddOrUpdateAsync(tx, username, profile, (key, value) => profile);
                await tx.CommitAsync();
            }
        }

        public async Task UpdateUserBalanceAsync(string username, decimal amount)
        {
            var profiles = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfileModel>>(UserProfilesDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                var userProfileResult = await profiles.TryGetValueAsync(tx, username);
                if (userProfileResult.HasValue)
                {
                    var userProfile = userProfileResult.Value;
                    userProfile.Balance += amount;

                    await profiles.SetAsync(tx, username, userProfile);
                    await tx.CommitAsync();
                }
                else
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"User profile for {username} not found.");
                    await tx.CommitAsync();
                }
            }
        }

        public async Task<decimal> GetUserBalanceAsync(string username)
        {
            var profiles = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfileModel>>(UserProfilesDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                var userProfileResult = await profiles.TryGetValueAsync(tx, username);
                await tx.CommitAsync();

                return userProfileResult.HasValue ? userProfileResult.Value.Balance : 0m;
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        private async Task ShowUserProfilesAsync()
        {
            var profiles = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfileModel>>(UserProfilesDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                var allProfiles = await profiles.CreateEnumerableAsync(tx);
                using (var enumerator = allProfiles.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var currentProfile = enumerator.Current.Value;
                        ServiceEventSource.Current.ServiceMessage(this.Context, $"User '{currentProfile.Name}' with email '{currentProfile.Email}' has {currentProfile.Balance:C} credit.");
                    }
                }
                await tx.CommitAsync();
            }
        }

        public async Task<List<UserProfileModel>> GetAllUserProfilesAsync()
        {
            var profiles = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfileModel>>(UserProfilesDictionaryName);

            var allProfiles = new List<UserProfileModel>();

            using (var tx = StateManager.CreateTransaction())
            {
                var enumerable = await profiles.CreateEnumerableAsync(tx);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        allProfiles.Add(enumerator.Current.Value);
                    }
                }
                await tx.CommitAsync();
            }

            return allProfiles;
        }

        private async Task AddInitialUsersAsync()
        {
            var profiles = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfileModel>>(UserProfilesDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                // Password: password123
                var user1 = new UserProfileModel { Username = "predrag.pesic", Name = "Predrag Pesic", Email = "predrag.pesic@microsoft.com", Balance = 10000m, Country = "SRB", HashedPassword = "$2b$12$74hBzFAXasCNiXTG9YEpXuEgAv7Pu9QRCQ1RgLYum/GfTa3aaJLXi" };
                var user2 = new UserProfileModel { Username = "milica.karic", Name = "Milica Karic", Email = "milica.karic@microsoft.com", Balance = 10000m, Country = "MNE", HashedPassword = "$2b$12$74hBzFAXasCNiXTG9YEpXuEgAv7Pu9QRCQ1RgLYum/GfTa3aaJLXi" };
                var user3 = new UserProfileModel { Username = "dragan.karic", Name = "Dragan Karic", Email = "dragan.karic@microsoft.com", Balance = 10000m, Country = "BIH", HashedPassword = "$2b$12$74hBzFAXasCNiXTG9YEpXuEgAv7Pu9QRCQ1RgLYum/GfTa3aaJLXi" };
                var user4 = new UserProfileModel { Username = "ranko.pesic", Name = "Ranko Pesic", Email = "ranko.pesic@microsoft.com", Balance = 10000m, Country = "BIH", HashedPassword = "$2b$12$74hBzFAXasCNiXTG9YEpXuEgAv7Pu9QRCQ1RgLYum/GfTa3aaJLXi" };
                var user5 = new UserProfileModel { Username = "slavica.ivanovic", Name = "Slavica Ivanovic", Email = "slavica.ivanovic@microsoft.com", Balance = 10000m, Country = "SRB", HashedPassword = "$2b$12$74hBzFAXasCNiXTG9YEpXuEgAv7Pu9QRCQ1RgLYum/GfTa3aaJLXi" };
                var user6 = new UserProfileModel { Username = "zorka.samardzic", Name = "Zorka Samardzic", Email = "zorka.samardzic@microsoft.com", Balance = 10000m, Country = "MNE", HashedPassword = "$2b$12$74hBzFAXasCNiXTG9YEpXuEgAv7Pu9QRCQ1RgLYum/GfTa3aaJLXi" };

                await profiles.AddOrUpdateAsync(tx, user1.Username, user1, (key, value) => user1);
                await profiles.AddOrUpdateAsync(tx, user2.Username, user2, (key, value) => user2);
                await profiles.AddOrUpdateAsync(tx, user3.Username, user3, (key, value) => user3);
                await profiles.AddOrUpdateAsync(tx, user4.Username, user4, (key, value) => user4);
                await profiles.AddOrUpdateAsync(tx, user5.Username, user5, (key, value) => user5);
                await profiles.AddOrUpdateAsync(tx, user6.Username, user6, (key, value) => user6);
                await tx.CommitAsync();

                ServiceEventSource.Current.ServiceMessage(this.Context, $"Initial user '{user1.Name}' added with credit {user1.Balance:C}");
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Initial user '{user2.Name}' added with credit {user2.Balance:C}");
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Initial user '{user3.Name}' added with credit {user3.Balance:C}");
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Initial user '{user4.Name}' added with credit {user4.Balance:C}");
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Initial user '{user5.Name}' added with credit {user5.Balance:C}");
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Initial user '{user6.Name}' added with credit {user6.Balance:C}");
            }
        }

        private async Task AddInitialPortfoliosAsync()
        {
            var tradingProxySrb = GetTradingInitProxy("SRB");
            await tradingProxySrb.OnUserInitializationCompletedAsync("SRB");

            var tradingProxyMne = GetTradingInitProxy("MNE");
            await tradingProxySrb.OnUserInitializationCompletedAsync("MNE");

            var tradingProxyBih = GetTradingInitProxy("BIH");
            await tradingProxySrb.OnUserInitializationCompletedAsync("BIH");
        }

        private async Task LogUserProfilesAsync()
        {
            try
            {
                await ShowUserProfilesAsync();
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Error logging user profiles: {ex.Message}");
            }
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await AddInitialUsersAsync();
            await AddInitialPortfoliosAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                await LogUserProfilesAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }
}
