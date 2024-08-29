using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Function
{
    public class GetDiversityByIdHttpTrigger
    {
        private readonly IGetDiversityByIdHttpTriggerService _getDiversityService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger _logger;

        public GetDiversityByIdHttpTrigger(
            IGetDiversityByIdHttpTriggerService getDiversityService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<GetDiversityByIdHttpTrigger> logger)
        {
            _getDiversityService = getDiversityService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _logger = logger;
        }

        [Function("GetById")]
        [ProducesResponseType(typeof(Models.Diversity), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Details found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity Details do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return the diversity details for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/DiversityDetails/{diversityId}")]
            HttpRequest req, string customerId, string diversityId)
        {

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }
            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }
            _logger.LogInformation($"DssCorrelationId: {correlationGuid}");

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = new BadRequestResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to locate 'APIM-TouchpointId' in request header");
                return response;
            }

            _logger.LogInformation("C# HTTP trigger function GetDiversityByIdHttpTrigger processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid.ToString());
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}");
                return response;
            }

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                var response = new BadRequestObjectResult(diversityId);
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'diversityId' to a Guid: {diversityId}");
                return response;
            }

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Customer does not exist {customerGuid}");
                return response;
            }

            _logger.LogInformation($"Attempting to get diversity {diversityGuid} for customer {customerGuid}");
            var diversity = await _getDiversityService.GetDiversityDetailByIdAsync(customerGuid, diversityGuid);

            if (diversity == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Diversity {diversityGuid} for customer {customerGuid} not found.");
                return response;
            }
            else
            {
                var response = new JsonResult(diversity, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };

                _logger.LogInformation($"Response Status Code: {response.StatusCode}. Diversity {diversity} found for customer {customerGuid}");
                return response;
            }
        }
    }
}
