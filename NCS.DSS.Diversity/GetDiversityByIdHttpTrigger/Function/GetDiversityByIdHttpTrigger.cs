using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service;

namespace NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Function
{
    public class GetDiversityByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetDiversityByIdHttpTriggerService _getDiversityService;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IGuidHelper _guidHelper;

        public GetDiversityByIdHttpTrigger(IResourceHelper resourceHelper, IGetDiversityByIdHttpTriggerService getDiversityService, ILoggerHelper loggerHelper, IHttpRequestHelper httpRequestHelper, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _getDiversityService = getDiversityService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _guidHelper = guidHelper;
        }

        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.Diversity), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Details found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity Details do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return the diversity details for a given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/DiversityDetails/{diversityId}")]
            HttpRequest req, ILogger log, string customerId, string diversityId)
        {

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
            {
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }
            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }
            log.LogInformation($"DssCorrelationId: {correlationGuid}");

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = _httpResponseMessageHelper.BadRequest();
                log.LogWarning($"Response Status Code: {response.StatusCode}. Unable to locate 'APIM-TouchpointId' in request header");
                return response;
            }

            log.LogInformation("C# HTTP trigger function GetDiversityHttpTrigger processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = _httpResponseMessageHelper.BadRequest(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}");
                return response;
            }

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                var response = _httpResponseMessageHelper.BadRequest(diversityId);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'diversityId' to a Guid: {diversityId}");
                return response;
            }

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = _httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Customer does not exist {customerGuid}");
                return response;
            }

            log.LogInformation($"Attempting to get diversity {diversityGuid} for customer {customerGuid}");
            var diversity = await _getDiversityService.GetDiversityDetailByIdAsync(customerGuid, diversityGuid);

            if (diversity ==  null)
            {
                var response = _httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Diversity {diversityGuid} for customer {customerGuid} not found.");
                return response;
            }
            else
            {
                var response = _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(diversity, "id", "DiversityId"));
                log.LogInformation($"Response Status Code: {response.StatusCode}. Diversity {diversity} found for customer {customerGuid}");
                return response;
            }
        }
    }
}
