using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Function
{
    public class PostDiversityHttpTrigger
    {

        private readonly IResourceHelper _resourceHelper;
        private readonly IPostDiversityHttpTriggerService _postDiversityService;
        private readonly IValidate _validate;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IGuidHelper _guidHelper;

        public PostDiversityHttpTrigger(IResourceHelper resourceHelper, IPostDiversityHttpTriggerService postDiversityService, IValidate validate, ILoggerHelper loggerHelper, IHttpRequestHelper httpRequestHelper, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _postDiversityService = postDiversityService;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _guidHelper = guidHelper;
        }

        [FunctionName("Post")]
        [ProducesResponseType(typeof(Models.Diversity), 201)]
        [Response(HttpStatusCode = (int) HttpStatusCode.Created, Description = "Diversity Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int) HttpStatusCode.NoContent, Description = "Diversity does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int) HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int) HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int) HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Conflict, Description = "Diversity Details already exists for customer", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new diversity record for a given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/DiversityDetails")]
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
                log.LogWarning( $"Response Status Code: {response.StatusCode}. Unable to locate 'APIM-TouchpointId' in request header.");
                return response;
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                var response = _httpResponseMessageHelper.BadRequest();
                log.LogWarning( $"Response Status Code: {response.StatusCode}. Unable to locate 'apimurl' in request header.");
                return response;
            }

            log.LogInformation($"Post Diversity C# HTTP trigger function  processed a request. By Touchpoint: {touchpointId}");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = _httpResponseMessageHelper.BadRequest(customerGuid);
                log.LogWarning( $"Response Status Code: {response.StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}.");
                return response;
            }

            Models.Diversity diversityRequest;

            try
            {
                diversityRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(req);
            }
            catch (JsonException ex)
            {
                var response = _httpResponseMessageHelper.UnprocessableEntity(ex);
                log.LogError($"Response Status Code: {response.StatusCode}. Unable to retrieve body from request.", ex);
                return response;

            }

            if (diversityRequest == null)
            {
               var response = _httpResponseMessageHelper.UnprocessableEntity(req);
               log.LogWarning($"Response Status Code: {response.StatusCode}. Diversity patch request is null.");
               return response;
            }

            diversityRequest.SetIds(customerGuid, touchpointId);
            diversityRequest.SetDefaultValues();

            // validate the request
            log.LogInformation( "Attempt to validate resource");
            var errors = _validate.ValidateResource(diversityRequest);

            if (errors != null && errors.Any())
            {
                var response = _httpResponseMessageHelper.UnprocessableEntity(errors);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Validation errors with resource.", errors);
                return response;
            }

            log.LogInformation( $"Attempting to see if customer exists {customerGuid}");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = _httpResponseMessageHelper.NoContent(customerGuid);
               log.LogWarning($"Response Status Code: {response.StatusCode}. Customer does not exist {customerGuid}.");
                return response;
            }

            log.LogInformation( $"Attempting to see if this is a read only customer {customerGuid}");
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                var response = _httpResponseMessageHelper.Forbidden(customerGuid);
               log.LogWarning($"Response Status Code: {response.StatusCode}. Customer {customerGuid} is read only ");
                return response;
            }

            var doesDiversityDetailsExist = _postDiversityService.DoesDiversityDetailsExistForCustomer(customerGuid);

            if (doesDiversityDetailsExist)
            {
               var response = _httpResponseMessageHelper.Conflict();
               log.LogWarning($"Response Status Code: {response.StatusCode}. Diversity details already exist for the customer {customerGuid}.");
                return response;
            }

            var diversity = await _postDiversityService.CreateAsync(diversityRequest);

            if (diversity != null)
            {
                log.LogInformation( $"attempting to send to service bus {diversity.DiversityId}");
                await _postDiversityService.SendToServiceBusQueueAsync(diversityRequest, apimUrl, correlationGuid, log);
            }

            if (diversity == null)
            {
                var response = _httpResponseMessageHelper.BadRequest(customerGuid);
                log.LogWarning( $"Response Status Code: {response.StatusCode}. Failed to post diversity for the customer {customerGuid}.");
                return response;
            }
            else
            {
                var response = _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(diversity, "id", "DiversityId"));
                log.LogInformation( $"Response Status Code: {response.StatusCode}. Diversity {diversity.DiversityId} successfully created for the customer {customerGuid}");
                return response;
            }
        }
    }
}