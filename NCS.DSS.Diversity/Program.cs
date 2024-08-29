using DFC.Common.Standard.Logging;
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

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();
        services.AddSingleton<IResourceHelper, ResourceHelper>();
        services.AddSingleton<IValidate, Validate>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
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
    })
    .ConfigureLogging(logging =>
    {
        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
        // For more information, see https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#application-insights
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();

host.Run();
