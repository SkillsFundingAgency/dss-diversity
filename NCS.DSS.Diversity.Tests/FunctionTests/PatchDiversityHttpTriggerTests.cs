using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace NCS.DSS.Diversity.Tests.FunctionTests
{
    
    public class PatchDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string DiversityId = "111a3e1c-2516-4a6b-9fc1-c85e578ac099";

        private readonly IPatchDiversityHttpTriggerService _patchDiversityHttpTriggerService;
        private readonly ILogger _log;
        private readonly HttpRequest _request;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IValidate _validate;
        private readonly Models.Diversity _diversity;
        private readonly DiversityPatch _diversityPatch;
        private readonly PatchDiversityHttpTrigger.Function.PatchDiversityHttpTrigger _patchDiversityHttpTrigger;

        public PatchDiversityHttpTriggerTests()
        {
            _diversity = Substitute.For<Models.Diversity>();
            _diversityPatch = Substitute.For<Models.DiversityPatch>();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _validate = Substitute.For<IValidate>();
            _patchDiversityHttpTriggerService = Substitute.For<IPatchDiversityHttpTriggerService>();

            _patchDiversityHttpTrigger = Substitute.For<PatchDiversityHttpTrigger.Function.PatchDiversityHttpTrigger>(
                _resourceHelper,
                _patchDiversityHttpTriggerService,
                _validate,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper);

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:7071/");

            _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(_request).Returns(Task.FromResult(_diversity).Result);
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _httpRequestHelper.GetResourceFromRequest<Models.DiversityPatch>(_request).Returns(Task.FromResult(_diversityPatch).Result);

            SetUpHttpResponseMessageHelper();
        }

        [Fact]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            var result = await RunFunction(InValidId, DiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenDiversityIdIsInvalid()
        {
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityHasFailedValidation()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.DiversityPatch>()).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
        }

        [Fact]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.DiversityPatch>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
        }

        [Fact]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }


        [Fact]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateAddressRecord()
        {
            _patchDiversityHttpTriggerService.GetDiversityForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_diversity.ToString()).Result);

            _patchDiversityHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Diversity>(null).Result);

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }


        [Fact]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeOk_WhenRequestIsValid()
        {
            _patchDiversityHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult(_diversity).Result);

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        public async Task<HttpResponseMessage> RunFunction(string customerId, string diversityId)
        {
            return await _patchDiversityHttpTrigger.Run(
                _request,
                _log,
                customerId,
                diversityId).ConfigureAwait(false);
        }

        private void SetUpHttpResponseMessageHelper()
        {
            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>())
                .Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<HttpRequest>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<JsonException>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            _httpResponseMessageHelper.Forbidden().Returns(x => new HttpResponseMessage(HttpStatusCode.Forbidden));

            _httpResponseMessageHelper.Conflict().Returns(x => new HttpResponseMessage(HttpStatusCode.Conflict));
            
            _httpResponseMessageHelper
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

        }

    }
}
