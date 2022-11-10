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

            var correlationGuid = _guidHelper.ValidateGuid(correlationId);

            if (correlationGuid == Guid.Empty)
                correlationGuid = _guidHelper.GenerateGuid();

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'APIM-TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Patch Diversity C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            var customerGuid = _guidHelper.ValidateGuid(customerId);
            if (customerGuid == Guid.Empty)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return _httpResponseMessageHelper.BadRequest(customerId);
            }

            Models.Diversity diversityRequest; 

            try
            {
                diversityRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogException(log, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            if (diversityRequest == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Diversity patch request is null");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            diversityRequest.SetIds(customerGuid, touchpointId);
            diversityRequest.SetDefaultValues();

            // validate the request
            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(diversityRequest);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return _httpResponseMessageHelper.Forbidden(customerGuid);
            }

            var doesDiversityDetailsExist = _postDiversityService.DoesDiversityDetailsExistForCustomer(customerGuid);

            if (doesDiversityDetailsExist)
                return _httpResponseMessageHelper.Conflict();

            var diversity = await _postDiversityService.CreateAsync(diversityRequest);

            if (diversity != null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", diversity.DiversityId));
                await _postDiversityService.SendToServiceBusQueueAsync(diversityRequest, apimUrl);
            }

            return diversity == null
                ? _httpResponseMessageHelper.BadRequest(customerGuid)
                : _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(diversity, "id", "DiversityId"));
        }
    }
}