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
using Authentication;
using LeaderboardStateless;
using TradingStateful;
using UserProfile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Fabric.Description;

namespace PlatformAPIV2
{
    public sealed class PlatformAPIV2 : StatelessService
    {
        public PlatformAPIV2(StatelessServiceContext context) : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddCors(options =>
                        {
                            options.AddPolicy("CorsPolicy", policy =>
                            {
                                /*
                                policy.WithOrigins(
                                    "http://localhost:8348", "http://mdcssql-prepes2:8348", "http://CPC-pesic-9JQ9J:8348",
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

                                /*
                                policy
                                    .AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                                */
                            });
                        });

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);

                        builder.Services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = "NovakDjokovicBreZnaMojaBabaCica",
                                ValidAudience = "NeOvoOnoSadJeIKulturanUssoSeKoPicca",
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("BanovoBrdoDvadesetTriJeNajjaciKraj"))
                            };
                        });

                        builder.WebHost
                            .UseKestrel()
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                            .UseUrls(url);

                        builder.Services.AddControllers();
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen(c =>
                        {
                            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "PlatformAPI", Version = "v1" });
                            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
                            {
                                Name = "Authorization",
                                Scheme = "Bearer",
                                BearerFormat = "JWT",
                                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                                Description = "\"JWT Authorization header using the Bearer scheme. \\r\\n\\r\\nEnter 'Bearer' [space] and then your token in the text input below.\\r\\n\\r\\nExample: \\\"Bearer 1safsfsdfdfd\\\"\""
                            });
                            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                            {
                                {
                                    new OpenApiSecurityScheme
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.SecurityScheme,
                                            Id = "Bearer"
                                        }
                                    }, new string[] { }
                                }
                            });
                        });

                        var app = builder.Build();

                        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                        {
                            app.UseSwagger();
                            app.UseSwaggerUI();
                        }

                        app.UseCors("CorsPolicy");
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.MapControllers();
                        
                        return app;

                    }))
            };
        }

        /*
        private async Task DefineDescription()
        {
            FabricClient fabricClient = new FabricClient();
            StatelessServiceUpdateDescription updateDescription = new();

            DefineAffinity(updateDescription);

            await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, updateDescription);
        }

        private void DefineAffinity(StatelessServiceUpdateDescription updateDescription)
        {
            ServiceCorrelationDescription serviceCorrelationDescription = new();
            serviceCorrelationDescription.Scheme = ServiceCorrelationScheme.Affinity;
            serviceCorrelationDescription.ServiceName = new Uri("fabric:/StockTradingPlatform/Authentication");

            updateDescription.Correlations ??= new List<ServiceCorrelationDescription>();
            updateDescription.Correlations.Add(serviceCorrelationDescription);
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            await DefineDescription();
            ServiceEventSource.Current.ServiceMessage(this.Context, $"{this.Context.NodeContext.NodeName} opened with service: PlatformAPIV2");
            await base.OnOpenAsync(cancellationToken);
        }
        */
    }
}
