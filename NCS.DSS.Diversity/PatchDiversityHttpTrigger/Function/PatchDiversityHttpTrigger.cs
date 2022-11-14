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
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Function
{
    public class PatchDiversityHttpTrigger
    {

        private readonly IResourceHelper _resourceHelper;
        private readonly IPatchDiversityHttpTriggerService _patchDiversityService;
        private readonly IValidate _validate;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IGuidHelper _guidHelper;

        public PatchDiversityHttpTrigger(IResourceHelper resourceHelper, IPatchDiversityHttpTriggerService patchDiversityService, IValidate validate, ILoggerHelper loggerHelper, IHttpRequestHelper httpRequestHelper, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _patchDiversityService = patchDiversityService;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _guidHelper = guidHelper;
        }


        [FunctionName("Patch")]
        [ProducesResponseType(typeof(Models.Diversity), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Detail Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an diversity detail record.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/DiversityDetails/{diversityId}")]HttpRequest req, ILogger log, string customerId, string diversityId)
        {
            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

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
                string.Format("Get PatchDiversityHttpTrigger C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'diversityId' to a Guid: {0}", diversityId));
                return _httpResponseMessageHelper.BadRequest(diversityId);
            }

            Models.DiversityPatch diversityPatchRequest;

            try
            {
                diversityPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.DiversityPatch>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogException(log, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (diversityPatchRequest == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Diversity patch request is null");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            diversityPatchRequest.LastModifiedBy = touchpointId;

            // validate the request
            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(diversityPatchRequest);

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

            var diversity = await _patchDiversityService.GetDiversityForCustomerAsync(customerGuid, diversityGuid);

            if (diversity == null)
            {
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            var patchedDiversity = _patchDiversityService.PatchResource(diversity, diversityPatchRequest);

            if (patchedDiversity == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to patch Diversity {0}", diversityGuid));
                return _httpResponseMessageHelper.NoContent(diversityGuid);
            }
            
            var updatedDiversity = await _patchDiversityService.UpdateCosmosAsync(patchedDiversity, diversityGuid);

            if (updatedDiversity != null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", updatedDiversity.DiversityId));
                await _patchDiversityService.SendToServiceBusQueueAsync(diversityPatchRequest, customerGuid, apimUrl);
            }

            return updatedDiversity == null ?
                _httpResponseMessageHelper.BadRequest(customerGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedDiversity, "id", "DiversityId"));
        }
    }
}