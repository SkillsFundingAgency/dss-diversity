using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ServiceBus;
using NCS.DSS.Diversity.Validation;

namespace NCS.DSS.Address
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices(services =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddLogging();
                    services.AddSingleton<IResourceHelper, ResourceHelper>();
                    services.AddSingleton<IValidate, Validate>();                    
                    services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddSingleton<IJsonHelper, JsonHelper>();
                    services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
                    services.AddSingleton<IServiceBusClient, ServiceBusClient>();
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();
                    services.AddSingleton<IGetDiversityByIdHttpTriggerService, GetDiversityByIdHttpTriggerService>();
                    services.AddSingleton<IGetDiversityHttpTriggerService, GetDiversityHttpTriggerService>();
                    services.AddSingleton<IPostDiversityHttpTriggerService, PostDiversityHttpTriggerService>();
                    services.AddTransient<IPatchDiversityHttpTriggerService, PatchDiversityHttpTriggerService>();
                    services.AddSingleton<IDiversityPatchService, DiversityPatchService>();
                    services.AddSingleton<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();

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
