using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserProfile
{
    public interface IInit : IService
    {
        Task OnUserInitializationCompletedAsync(string country);
    }
}
