using DFC.Common.Standard.CosmosDocumentClient;
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
using NCS.DSS.Diversity.Validation;

[assembly: FunctionsStartup(typeof(FunctionStartupExtension))]

namespace NCS.DSS.Diversity.Ioc
{
    public class FunctionStartupExtension : FunctionsStartup
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

            builder.Services.AddTransient<IGetDiversityByIdHttpTriggerService, GetDiversityByIdHttpTriggerService>();
            builder.Services.AddTransient<IGetDiversityHttpTriggerService, GetDiversityHttpTriggerService>();
            builder.Services.AddTransient<IPostDiversityHttpTriggerService, PostDiversityHttpTriggerService>();
            builder.Services.AddTransient<IPatchDiversityHttpTriggerService, PatchDiversityHttpTriggerService>();
            builder.Services.AddSingleton<IDiversityPatchService, DiversityPatchService>();
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();


        }
    }
}
