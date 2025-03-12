using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ServiceBus;
using NCS.DSS.Diversity.Validation;

namespace NCS.DSS.Diversity
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<DiversityConfigurationSettings>()
                        .Bind(configuration);

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddLogging();
                    services.AddSingleton<IResourceHelper, ResourceHelper>();
                    services.AddSingleton<IValidate, Validate>();
                    services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddSingleton<IJsonHelper, JsonHelper>();
                    services.AddSingleton<ICosmosDbProvider, CosmosDbProvider>();
                    services.AddSingleton<IDiversityServiceBusClient, DiversityServiceBusClient>();
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();
                    services.AddSingleton<IGetDiversityByIdHttpTriggerService, GetDiversityByIdHttpTriggerService>();
                    services.AddSingleton<IGetDiversityHttpTriggerService, GetDiversityHttpTriggerService>();
                    services.AddSingleton<IPostDiversityHttpTriggerService, PostDiversityHttpTriggerService>();
                    services.AddTransient<IPatchDiversityHttpTriggerService, PatchDiversityHttpTriggerService>();
                    services.AddSingleton<IDiversityPatchService, DiversityPatchService>();
                    services.AddSingleton<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();

                    services.AddSingleton(s =>
                    {
                        var cosmosDbEndpoint = configuration["CosmosDbEndpoint"];
                        if (string.IsNullOrEmpty(cosmosDbEndpoint))
                        {
                            throw new InvalidOperationException("CosmosDbEndpoint is not configured.");
                        }

                        var options = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway };
                        return new CosmosClient(cosmosDbEndpoint, new DefaultAzureCredential(), options);
                    });

                    services.AddSingleton(s =>
                    {
                        var settings = s.GetRequiredService<IOptions<DiversityConfigurationSettings>>().Value;

                        return new ServiceBusClient(settings.ServiceBusConnectionString);
                    });

                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
