using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace NCS.DSS.Diversity.Tests.FunctionTests
{

   
    public class PostDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidCustomerId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private static readonly Guid CustomerGuid = Guid.NewGuid();
        private static readonly Guid DiversityGuid = Guid.NewGuid();

        private readonly IPostDiversityHttpTriggerService _postDiversityHttpTriggerService;
        private readonly ILogger _log;
        private readonly HttpRequest _request;
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IValidate _validate;
        private readonly IGuidHelper _guidHelper;

        private readonly Models.Diversity _diversity;
        private readonly PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger _postDiversityHttpTrigger;

        public PostDiversityHttpTriggerTests()
        {
            _diversity = Substitute.For<Models.Diversity>();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _validate = Substitute.For<IValidate>();
            _postDiversityHttpTriggerService = Substitute.For<IPostDiversityHttpTriggerService>();

            var loggerHelper = Substitute.For<ILoggerHelper>();
            var jsonHelper = Substitute.For<IJsonHelper>();
            _guidHelper = Substitute.For<IGuidHelper>();

            _postDiversityHttpTrigger = Substitute.For<PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger>(
                _resourceHelper,
                _postDiversityHttpTriggerService,
                _validate,
                loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                jsonHelper,
                _guidHelper);

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns("9999999999");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:7071/");
            _guidHelper.ValidateGuid(ValidCustomerId).Returns(CustomerGuid);

            _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(_request).Returns(Task.FromResult(_diversity).Result);
            _resourceHelper.DoesCustomerExist(CustomerGuid).Returns(true);
            _postDiversityHttpTriggerService.DoesDiversityDetailsExistForCustomer(DiversityGuid).Returns(false);

            SetUpHttpResponseMessageHelper();
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenSubcontractorIdIsNotProvided()
        {
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _guidHelper.ValidateGuid(ValidCustomerId).Returns(Guid.Empty);

            var result = await RunFunction(InValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityHasFailedValidation()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.ValidateResource(_diversity).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(CustomerGuid).Returns(false);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeConflict_WhenDiversityDetailsForCustomerExists()
        {
            _postDiversityHttpTriggerService.DoesDiversityDetailsExistForCustomer(CustomerGuid).Returns(true);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateDiversityDetailRecord()
        {
            _postDiversityHttpTriggerService.CreateAsync(_diversity).Returns(Task.FromResult<Models.Diversity>(null).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _postDiversityHttpTriggerService.CreateAsync(_diversity).Returns(Task.FromResult(_diversity).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        }

        public async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await _postDiversityHttpTrigger.Run(
                _request,
                _log,
                customerId).ConfigureAwait(false);
        }

        private void SetUpHttpResponseMessageHelper()
        {
            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>())
                .Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<HttpRequest>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            _httpResponseMessageHelper.Forbidden().Returns(x => new HttpResponseMessage(HttpStatusCode.Forbidden));

            _httpResponseMessageHelper.Conflict().Returns(x => new HttpResponseMessage(HttpStatusCode.Conflict));
            
            _httpResponseMessageHelper
                .Created(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.Created));

        }

    }
}
