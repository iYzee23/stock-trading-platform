using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using UserProfile;

namespace Authentication
{
    public sealed class Authentication : StatelessService, IAuthentication
    {
        private readonly SymmetricSecurityKey _signingKey;
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;
        
        private readonly int _increment = 15;
        private int _initLoad = 15;

        public Authentication(StatelessServiceContext context) : base(context)
        {
            _secret = "BanovoBrdoDvadesetTriJeNajjaciKraj";
            _issuer = "NovakDjokovicBreZnaMojaBabaCica";
            _audience = "NeOvoOnoSadJeIKulturanUssoSeKoPicca";
            _expirationMinutes = 60;
            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secret));
        }

        private IUserProfile GetUserProfileProxy()
        {
            return ServiceProxy.Create<IUserProfile>(
                new Uri("fabric:/StockTradingPlatform/UserProfile"), new ServicePartitionKey(0));
        }

        /*
        private async Task DefineDescription()
        {
            FabricClient fabricClient = new FabricClient();
            StatelessServiceUpdateDescription updateDescription = new();

            DefineCustomMetrics(updateDescription);
            DefineAutoScaling(updateDescription);

            await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, updateDescription);
        }

        private void DefineCustomMetrics(StatelessServiceUpdateDescription updateDescription)
        {
            var userLoadMetric = new StatelessServiceLoadMetricDescription
            {
                Name = "UserRegistrationLoad",
                DefaultLoad = _initLoad,
                Weight = ServiceLoadMetricWeight.High
            };

            updateDescription.Metrics ??= new MetricsCollection();
            updateDescription.Metrics.Add(userLoadMetric);
        }

        private void DefineAutoScaling(StatelessServiceUpdateDescription updateDescription)
        {
            PartitionInstanceCountScaleMechanism partitionInstanceCountScaleMechanism = new PartitionInstanceCountScaleMechanism
            {
                MinInstanceCount = 1,
                MaxInstanceCount = 4,
                ScaleIncrement = 1
            };

            AveragePartitionLoadScalingTrigger averagePartitionLoadScalingTrigger = new AveragePartitionLoadScalingTrigger
            {
                MetricName = "UserRegistrationLoad",
                LowerLoadThreshold = 10.0,
                UpperLoadThreshold = 20.0,
                ScaleInterval = TimeSpan.FromSeconds(60)
            };

            ScalingPolicyDescription scalingPolicyDescription = new ScalingPolicyDescription(partitionInstanceCountScaleMechanism, averagePartitionLoadScalingTrigger);

            updateDescription.ScalingPolicies ??= new List<ScalingPolicyDescription>();
            updateDescription.ScalingPolicies.Add(scalingPolicyDescription);
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            await DefineDescription();
            ServiceEventSource.Current.ServiceMessage(this.Context, $"{this.Context.NodeContext.NodeName} opened with service: Authentication");
            await base.OnOpenAsync(cancellationToken);
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
        */

        public async Task<string> RegisterUserAsync(string username, String name, string password, string email, string country)
        {
            var userProfileProxy = GetUserProfileProxy();

            var existingProfileByUsername = await userProfileProxy.GetUserProfileAsync(username);
            if (existingProfileByUsername != null)
            {
                return "User already exist";
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var profile = new UserProfileModel { Username = username, Name = name, Email = email, Country = country, Balance = 10000, HashedPassword = hashedPassword };
            await userProfileProxy.AddOrUpdateUserProfileAsync(username, profile);

            // _initLoad += _increment;
            // ReportCustomMetric(_initLoad);

            return "Registration successful";
        }

        public async Task<string> LoginUserAsync(string username, string password)
        {
            var userProfileProxy = GetUserProfileProxy();

            var userProfile = await userProfileProxy.GetUserProfileAsync(username);
            if (userProfile != null && BCrypt.Net.BCrypt.Verify(password, userProfile.HashedPassword))
            {
                // _initLoad -= _increment;
                // ReportCustomMetric(_initLoad);
                return GenerateJwtToken(username, userProfile.Country);
            }

            return null;
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            return Task.FromResult(ValidateJwtToken(token));
        }

        private string GenerateJwtToken(string username, string country)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Country, country)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _issuer,
                Audience = _audience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }
    }
}
