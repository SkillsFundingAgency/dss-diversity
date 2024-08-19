using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Helpers;
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
        private readonly ILogger _logger;
        private readonly IValidate _validate;
        private readonly IHelper _helper;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public PostDiversityHttpTrigger(
            IPostDiversityHttpTriggerService postDiversityService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<PostDiversityHttpTrigger> logger,
            IValidate validate,
            IHelper helper,
            IDynamicHelper dynamicHelper)
        {
            _postDiversityService = postDiversityService;
            _httpRequestHelper = httpRequestHelper;
            _logger = logger;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _helper = helper;
            _dynamicHelper = dynamicHelper;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Diversity), 201)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Diversity Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Conflict, Description = "Diversity Details already exists for customer", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new diversity record for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/DiversityDetails")]
            HttpRequest req, string customerId)
        {
            // Ensure the request body can be read multiple times by enabling buffering
            req.EnableBuffering();

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");


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
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to locate 'APIM-TouchpointId' in request header.");
                return response;
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                var response = new BadRequestResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to locate 'apimurl' in request header.");
                return response;
            }

            _logger.LogInformation($"Post Diversity C# HTTP trigger function  processed a request. By Touchpoint: {touchpointId}");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid.ToString());
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}.");
                return response;
            }

            Models.Diversity diversityRequest;

            try
            {
                diversityRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(req);
                await _helper.UpdateValuesAsync(req, diversityRequest);
            }
            catch (JsonException ex)
            {
                var response = new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
                _logger.LogError($"Response Status Code: {response.StatusCode}. Unable to retrieve body from request.", ex);
                return response;
            }

            if (diversityRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Diversity patch request is null.");
                return response;
            }

            diversityRequest.SetIds(customerGuid, touchpointId);
            diversityRequest.SetDefaultValues();

            // validate the request
            _logger.LogInformation("Attempt to validate resource");
            var errors = _validate.ValidateResource(diversityRequest);

            if (errors != null && errors.Any())
            {
                var response = new UnprocessableEntityObjectResult(errors);
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Validation errors with resource.", errors);
                return response;
            }

            _logger.LogInformation($"Attempting to see if customer exists {customerGuid}");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Customer does not exist {customerGuid}.");
                return response;
            }

            _logger.LogInformation($"Attempting to see if this is a read only customer {customerGuid}");
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                };

                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Customer {customerGuid} is read only ");
                return response;
            }

            var doesDiversityDetailsExist = _postDiversityService.DoesDiversityDetailsExistForCustomer(customerGuid);

            if (doesDiversityDetailsExist)
            {
                var response = new ConflictResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Diversity details already exist for the customer {customerGuid}.");
                return response;
            }

            var diversity = await _postDiversityService.CreateAsync(diversityRequest);

            if (diversity != null)
            {
                _logger.LogInformation($"attempting to send to service bus {diversity.DiversityId}");
                await _postDiversityService.SendToServiceBusQueueAsync(diversityRequest, apimUrl, correlationGuid, _logger);
            }

            if (diversity == null)
            {
                var response = new BadRequestObjectResult(customerGuid.ToString());
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Failed to post diversity for the customer {customerGuid}.");
                return response;
            }
            else
            {
                var response = new JsonResult(diversity, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.Created,
                };

                _logger.LogInformation($"Response Status Code: {response.StatusCode}. Diversity {diversity.DiversityId} successfully created for the customer {customerGuid}");
                return response;
            }
        }
    }
}