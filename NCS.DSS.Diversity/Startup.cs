using DFC.Common.Standard.CosmosDocumentClient;
using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Ioc;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ServiceBus;
using NCS.DSS.Diversity.Validation;

[assembly: FunctionsStartup(typeof(Startup))]

namespace NCS.DSS.Diversity.Ioc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IResourceHelper, ResourceHelper>();
            builder.Services.AddSingleton<IValidate, Validate>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
            builder.Services.AddSingleton<ICosmosDocumentClient, CosmosDocumentClient>();
            builder.Services.AddSingleton<IServiceBusClient, ServiceBusClient>();
            builder.Services.AddSingleton<IGuidHelper, GuidHelper>();

            builder.Services.AddSingleton<IGetDiversityByIdHttpTriggerService, GetDiversityByIdHttpTriggerService>();
            builder.Services.AddSingleton<IGetDiversityHttpTriggerService, GetDiversityHttpTriggerService>();
            builder.Services.AddSingleton<IPostDiversityHttpTriggerService, PostDiversityHttpTriggerService>();
            builder.Services.AddTransient<IPatchDiversityHttpTriggerService, PatchDiversityHttpTriggerService>();
            builder.Services.AddSingleton<IDiversityPatchService, DiversityPatchService>();
            builder.Services.AddSingleton<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();


        }
    }
}
