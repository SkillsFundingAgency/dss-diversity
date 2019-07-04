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
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.Diversity.Tests
{

    [TestFixture]
    public class PostDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidCustomerId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";

        private IPostDiversityHttpTriggerService _postDiversityHttpTriggerService;
        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IValidate _validate;
        private Models.Diversity _diversity;

        [SetUp]
        public void Setup()
        {
            _diversity = Substitute.For<Models.Diversity>();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _validate = Substitute.For<IValidate>();
            _postDiversityHttpTriggerService = Substitute.For<IPostDiversityHttpTriggerService>();

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(_request).Returns(Task.FromResult(_diversity).Result);
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _postDiversityHttpTriggerService.DoesDiversityDetailsExistForCustomer(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            SetUpHttpResponseMessageHelper();
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            var result = await RunFunction(InValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityHasFailedValidation()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Diversity>()).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeConflict_WhenDiversityDetailsForCustomerExists()
        {
            _postDiversityHttpTriggerService.DoesDiversityDetailsExistForCustomer(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateDiversityDetailRecord()
        {
            _postDiversityHttpTriggerService.CreateAsync(Arg.Any<Models.Diversity>()).Returns(Task.FromResult<Models.Diversity>(null).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _postDiversityHttpTriggerService.CreateAsync(Arg.Any<Models.Diversity>()).Returns(Task.FromResult(_diversity).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        public async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger.Run(
                _request,
                _log,
                customerId,
                _resourceHelper,
                _postDiversityHttpTriggerService,
                _validate,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);

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

            _httpResponseMessageHelper.Forbidden().Returns(x => new HttpResponseMessage(HttpStatusCode.Forbidden));

            _httpResponseMessageHelper.Conflict().Returns(x => new HttpResponseMessage(HttpStatusCode.Conflict));
            
            _httpResponseMessageHelper
                .Created(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.Created));

        }

    }
}
