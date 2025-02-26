using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Function
{
    public class GetDiversityHttpTrigger
    {

        private readonly IGetDiversityHttpTriggerService _getDiversityService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<GetDiversityHttpTrigger> _logger;

        public GetDiversityHttpTrigger(
            IGetDiversityHttpTriggerService getDiversityService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<GetDiversityHttpTrigger> logger)
        {
            _getDiversityService = getDiversityService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _logger = logger;
        }

        [Function("Get")]
        [ProducesResponseType(typeof(Models.Diversity), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Details found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return the diversity details for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/DiversityDetails/")]
            HttpRequest req, string customerId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetDiversityHttpTrigger));

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
                _logger.LogError("Unable to locate 'TouchpointId' in request header");
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header");
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogError("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult($"Unable to parse 'customerId' to a GUID. Customer GUID: {customerId}");
            }

            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogError("Customer does not exist. Customer GUID: {CustomerGuid}.", customerGuid);
                return new NotFoundObjectResult($"Customer does not exist. Customer GUID: {customerGuid}.");
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);


            _logger.LogInformation("Attempting to get Diversity for Customer. Customer GUID: {CustomerId}.", customerGuid);
            var diversityDetails = await _getDiversityService.GetDiversityDetailForCustomerAsync(customerGuid);

            if (diversityDetails == null)
            {
                _logger.LogError("Diversity not found. Customer GUID: {CustomerId}.", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetDiversityHttpTrigger));
                return new NotFoundObjectResult($"Diversity not found. Customer GUID: {customerGuid}.");
            }

            if (diversityDetails.Count == 1)
            {
                _logger.LogInformation("1 Diversity found for Customer with ID: {CustomerId}.", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetDiversityHttpTrigger));
                return new JsonResult(diversityDetails[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            _logger.LogInformation("{Count} Diversty record(s) retrieved for Customer GUID: {CustomerId}.", diversityDetails.Count, customerGuid);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetDiversityHttpTrigger));
            return new JsonResult(diversityDetails, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
