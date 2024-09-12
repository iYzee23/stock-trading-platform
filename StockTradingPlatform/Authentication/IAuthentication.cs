using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication
{
    public interface IAuthentication : IService
    {
        Task<string> RegisterUserAsync(string username, String name, string password, string email, string country);

        Task<string> LoginUserAsync(string username, string password);
        
        Task<bool> ValidateTokenAsync(string token);
    }
}
