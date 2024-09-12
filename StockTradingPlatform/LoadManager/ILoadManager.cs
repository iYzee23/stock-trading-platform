using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadManager
{
    public interface ILoadManager : IService
    {
        Task<int> GetCurrentLoadAsync();

        Task SetLoadAsync(int load);
    }
}
