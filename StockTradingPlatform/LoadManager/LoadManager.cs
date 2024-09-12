using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace LoadManager
{
    public sealed class LoadManager : StatefulService, ILoadManager
    {
        private const string LoadKey = "CurrentLoad";

        public LoadManager(StatefulServiceContext context) : base(context) { }

        public async Task<int> GetCurrentLoadAsync()
        {
            var loadDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(LoadKey);
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await loadDictionary.TryGetValueAsync(tx, LoadKey);
                return result.HasValue ? result.Value : 0;
            }
        }

        public async Task SetLoadAsync(int load)
        {
            var loadDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(LoadKey);
            using (var tx = StateManager.CreateTransaction())
            {
                await loadDictionary.SetAsync(tx, LoadKey, load);
                await tx.CommitAsync();
            }

            ReportCustomMetric(load);

            if (load > 15)
            {
                await Task.Delay(TimeSpan.FromSeconds(20));
                await SetLoadAsync(5);
            }
        }

        private void ReportCustomMetric(int loadValue)
        {
            var loadMetrics = new List<LoadMetric>
            {
                new LoadMetric("UserRegistrationLoad", loadValue)
            };

            ServiceEventSource.Current.ServiceMessage(this.Context, $"Reported custom metric: UserRegistrationLoad with value {loadValue}");
            Partition.ReportLoad(loadMetrics);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
