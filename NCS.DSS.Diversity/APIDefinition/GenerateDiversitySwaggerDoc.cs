using System.Net;
using System.Net.Http;
using System.Reflection;
using DFC.Functions.DI.Standard.Attributes;
using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace NCS.DSS.Diversity.APIDefinition
{
    public static class GenerateDiversitySwaggerDoc
    {
        public const string ApiTitle = "DiversityDetails";
        public const string ApiDefinitionName = "API-Definition";
        public const string ApiDefRoute = ApiTitle + "/" + ApiDefinitionName;
        public const string ApiDescription = "Basic details of a National Careers Service " + ApiTitle + " Resource";
        public const string ApiVersion = "2.0.0";

        [FunctionName(ApiDefinitionName)]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiDefRoute)]HttpRequest req,
            [Inject]ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            var swagger = swaggerDocumentGenerator.GenerateSwaggerDocument(req, ApiTitle, ApiDescription,
                ApiDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
                return new HttpResponseMessage(HttpStatusCode.NoContent);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swagger)
            };
        }
    }
}