using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Function
{
    public class PostDiversityHttpTrigger
    {

        private readonly IPostDiversityHttpTriggerService _postDiversityService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<PostDiversityHttpTrigger> _logger;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public PostDiversityHttpTrigger(
            IPostDiversityHttpTriggerService postDiversityService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<PostDiversityHttpTrigger> logger,
            IValidate validate,
            IDynamicHelper dynamicHelper)
        {
            _postDiversityService = postDiversityService;
            _httpRequestHelper = httpRequestHelper;
            _logger = logger;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Diversity), 201)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Diversity Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Conflict, Description = "Diversity Details already exists for customer", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new diversity record for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/DiversityDetails")]
            HttpRequest req, string customerId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PostDiversityHttpTrigger));
            // Ensure the request body can be read multiple times by enabling buffering
            req.EnableBuffering();

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
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header");
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogWarning("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult($"Unable to locate 'apimURL' in request header. Correlation GUID: {correlationGuid}");
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult($"Unable to parse 'customerId' to a GUID. Customer GUID: {customerId}");
            }
            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            Models.Diversity diversityRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request. Correlation GUID: {CorrelationGuid}", correlationGuid);
                diversityRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(req);

                // Fix for bug AD-157065 (Oct '23)
                if (diversityRequest.ConsentToCollectEthnicity == null)
                    diversityRequest.ConsentToCollectEthnicity = false;

                if (diversityRequest.ConsentToCollectLLDDHealth == null)
                    diversityRequest.ConsentToCollectLLDDHealth = false;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to parse {diversityRequest} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(diversityRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult($"Unable to parse Diversity Details from request body. Exception: {ex.Message}");
            }

            if (diversityRequest == null)
            {
                _logger.LogWarning("{diversityRequest} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(diversityRequest), correlationGuid);
                return new UnprocessableEntityObjectResult($"Diversity Details in request body are NULL. Please supply this data.");
            }

            diversityRequest.SetIds(customerGuid, touchpointId);
            diversityRequest.SetDefaultValues();

            // validate the request
            _logger.LogInformation("Attempting to validate {diversityRequest} object", nameof(diversityRequest));
            var errors = _validate.ValidateResource(diversityRequest);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {diversityRequest} object", nameof(diversityRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {diversityRequest} object", nameof(diversityRequest));

            _logger.LogInformation("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer not found. Customer ID: {CustomerId}.", customerGuid);
                return new NotFoundObjectResult($"Customer not found. Customer ID: {customerGuid}.");
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Checking if customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult($"Customer is read-only. Customer GUID: {customerGuid}.")
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                };

                _logger.LogWarning("Customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
                return response;
            }

            _logger.LogInformation("Checking if Diversity record alread exists for customer with ID: {CustomerId}.", customerGuid);
            var doesDiversityDetailsExist = await _postDiversityService.DoesDiversityDetailsExistForCustomer(customerGuid);

            if (doesDiversityDetailsExist)
            {
                _logger.LogWarning("Diversity record already exist for customer with ID: {customerGuid}", customerGuid);
                return new ConflictObjectResult($"Diversity record already exist for customer with ID: {customerGuid}");
            }
            _logger.LogInformation("Diversity record does not exists for customer with ID: {customerGuid}", customerGuid);

            _logger.LogInformation("Attempting to create Diversity in Cosmos DB. Diversity GUID: {DiversityId}", diversityRequest.DiversityId);
            var diversity = await _postDiversityService.CreateAsync(diversityRequest);

            if (diversity == null)
            {
                _logger.LogWarning("Failed to create Diversity in Cosmos DB. Diversity GUID: {DiversityId}", diversityRequest.DiversityId);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostDiversityHttpTrigger));
                return new BadRequestObjectResult($"Failed to create Diversity in Cosmos DB. Diversity GUID: {diversityRequest.DiversityId}");
            }

            _logger.LogInformation("Diversity created successfully in Cosmos DB. Diversity GUID: {DiversityId}", diversity.DiversityId);


            _logger.LogInformation("Attempting to send message to Service Bus Namespace. Diversity GUID: {DiversityId}", diversity.DiversityId);
            await _postDiversityService.SendToServiceBusQueueAsync(diversityRequest, apimUrl, correlationGuid);
            _logger.LogInformation("Successfully sent message to Service Bus. Diversity GUID: {DiversityId}", diversity.DiversityId);


            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostDiversityHttpTrigger));
            return new JsonResult(diversity, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created,
            };
        }
    }
}