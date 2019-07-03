using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
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
    public static class PostDiversityHttpTrigger
    {
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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/DiversityDetails")] HttpRequest req, ILogger log, string customerId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IPostDiversityHttpTriggerService postDiversityService,
            [Inject]IValidate validate,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {
            loggerHelper.LogMethodEnter(log);

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'TouchpointId' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Patch Diversity C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return httpResponseMessageHelper.BadRequest(customerGuid);
            }
            
            Models.Diversity diversityRequest;

            try
            {
                diversityRequest = await httpRequestHelper.GetResourceFromRequest<Models.Diversity>(req);
            }
            catch (JsonException ex)
            {
                loggerHelper.LogException(log, correlationGuid, "Unable to retrieve body from req", ex);
                return httpResponseMessageHelper.UnprocessableEntity(req);
            }

            if (diversityRequest == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Diversity patch request is null");
                return httpResponseMessageHelper.UnprocessableEntity(req);
            }

            diversityRequest.SetIds(customerGuid, touchpointId);

            // validate the request
            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = validate.ValidateResource(diversityRequest);

            if (errors != null && errors.Any())
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return httpResponseMessageHelper.Forbidden(customerGuid);
            }

            var doesDiversityDetailsExist = postDiversityService.DoesDiversityDetailsExistForCustomer(customerGuid);

            if (doesDiversityDetailsExist)
                return httpResponseMessageHelper.Conflict();

            var diversity = await postDiversityService.CreateAsync(diversityRequest);

            return diversity == null
                ? httpResponseMessageHelper.BadRequest(customerGuid)
                : httpResponseMessageHelper.Created(jsonHelper.SerializeObjectAndRenameIdProperty(diversity, "id", "DiversityId"));
        }
    }
}