using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Helpers;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Diversity.Tests
{

    [TestFixture]
    public class PostDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidCustomerId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IPostDiversityHttpTriggerService _postDiversityHttpTriggerService;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private Models.Diversity _diversity;

        [SetUp]
        public void Setup()
        {
            _diversity = Substitute.For<Models.Diversity>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/DiversityDetails/")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _postDiversityHttpTriggerService = Substitute.For<IPostDiversityHttpTriggerService>();
            _validate = Substitute.For<IValidate>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
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
            _httpRequestMessageHelper.GetDiversityFromRequest(_request).Returns(Task.FromResult(_diversity).Result);

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
            var validationResults = new List<ValidationResult> {new ValidationResult("Customer Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Diversity>()).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetDiversityFromRequest(_request).Returns(Task.FromResult(_diversity).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateDiversityDetailRecord()
        {
            _httpRequestMessageHelper.GetDiversityFromRequest(_request).Returns(Task.FromResult(_diversity).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _postDiversityHttpTriggerService.CreateAsync(Arg.Any<Models.Diversity>()).Returns(Task.FromResult<Models.Diversity>(null).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetDiversityFromRequest(_request).Returns(Task.FromResult(_diversity).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _postDiversityHttpTriggerService.CreateAsync(Arg.Any<Models.Diversity>()).Returns(Task.FromResult<Models.Diversity>(_diversity).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        public async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return  await PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger.Run(
                _request, _log, customerId, _resourceHelper, _httpRequestMessageHelper, _validate, _postDiversityHttpTriggerService).ConfigureAwait(false);

        }
    }
}
