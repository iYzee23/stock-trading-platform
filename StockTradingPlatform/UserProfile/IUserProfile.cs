using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserProfile
{
    public interface IUserProfile : IService
    {
        Task<UserProfileModel> GetUserProfileAsync(string username);

        Task AddOrUpdateUserProfileAsync(string username, UserProfileModel profile);
        
        Task UpdateUserBalanceAsync(string username, decimal amount);

        Task<decimal> GetUserBalanceAsync(string username);

        Task<List<UserProfileModel>> GetAllUserProfilesAsync();
    }
}
