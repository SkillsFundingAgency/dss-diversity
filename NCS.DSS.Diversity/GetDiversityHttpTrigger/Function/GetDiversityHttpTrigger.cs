using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Function
{
    public class GetDiversityHttpTrigger
    {

        private readonly IResourceHelper _resourceHelper;
        private readonly IGetDiversityHttpTriggerService _getDiversityService;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IGuidHelper _guidHelper;

        public GetDiversityHttpTrigger(IResourceHelper resourceHelper, IGetDiversityHttpTriggerService getDiversityService, ILoggerHelper loggerHelper, IHttpRequestHelper httpRequestHelper, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _getDiversityService = getDiversityService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _guidHelper = guidHelper;
        }
        
        [FunctionName("Get")]
        [ProducesResponseType(typeof(Models.Diversity), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Details found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity Details do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return the diversity details for a given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/DiversityDetails/")]
            HttpRequest req, ILogger log, string customerId)
        {
            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

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

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = _httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Customer does not exist {customerGuid}");
                return response;
            }

            log.LogInformation($"Attempting to get diversity for customer {customerGuid}");
            var diversityDetails = await _getDiversityService.GetDiversityDetailForCustomerAsync(customerGuid);

            if (diversityDetails ==  null)
            {
                var response = _httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Diversity for customer {customerGuid} not found.");
                return response;
            }
            else
            {
                var response = _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectsAndRenameIdProperty(diversityDetails, "id", "DiversityId"));
                log.LogInformation($"Response Status Code: {response.StatusCode}. Diversity {diversityDetails} found for customer {customerGuid}");
                return response;
            }
        }
    }
}
