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
        private readonly ILogger<GetDiversityByIdHttpTrigger> _logger;

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
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetDiversityByIdHttpTrigger));

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }
            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid. CorrelationId: {CorrelationId}", correlationId);
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult(customerId);
            }

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                _logger.LogWarning("Unable to parse 'diversityId' to a GUID. Diversity GUID: {DiversityID}", diversityId);
                return new BadRequestObjectResult(diversityId);
            }

            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}.", customerGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);


            _logger.LogInformation("Attempting to get Diversity for Customer. Customer GUID: {CustomerId}. Diversity GUID: {DiversityId}.", customerGuid, diversityGuid);
            var diversity = await _getDiversityService.GetDiversityDetailByIdAsync(customerGuid, diversityGuid);

            if (diversity == null)
            {
                _logger.LogWarning("Diversity not found. Customer GUID: {CustomerId}. Diversity GUID: {DiversityId}.", customerGuid, diversityGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetDiversityByIdHttpTrigger));
                return new NoContentResult();
            }


            _logger.LogInformation("Diversity successfully retrieved. Diversity GUID: {DiversityId}", diversity.DiversityId);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetDiversityByIdHttpTrigger));
            return new JsonResult(diversity, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
