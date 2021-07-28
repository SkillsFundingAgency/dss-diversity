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
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;

namespace NCS.DSS.Diversity.Tests.FunctionTests
{

    [TestFixture]
    public class PostDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidCustomerId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private static readonly Guid CustomerGuid = Guid.NewGuid();
        private static readonly Guid DiversityGuid = Guid.NewGuid();

        private Mock<IPostDiversityHttpTriggerService> _postDiversityHttpTriggerService;
        private Mock<ILogger> _log;
        private DefaultHttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private HttpResponseMessageHelper _httpResponseMessageHelper;
        private Mock<IGuidHelper> _guidHelper;
        private Mock<IValidate> _validate;
        private IJsonHelper _jsonHelper;
        private Mock<ILoggerHelper> _loggerHelper;

        private Models.Diversity _diversity;
        private PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger function;

        [SetUp]
        public void Setup()
        {
            _diversity = new Models.Diversity();

            _request = null;

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();

            _diversity = new Models.Diversity();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _validate = new Mock<IValidate>();
            _postDiversityHttpTriggerService = new Mock<IPostDiversityHttpTriggerService>();


            _loggerHelper = new Mock<ILoggerHelper>();
            _jsonHelper = new JsonHelper();
            _guidHelper = new Mock<IGuidHelper>();

            function = new PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger(
                _resourceHelper.Object,
                _postDiversityHttpTriggerService.Object,
                _validate.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper.Object);

            _httpRequestHelper.Setup(x => x.GetDssCorrelationId(_request)).Returns(ValidDssCorrelationId);
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _guidHelper.Setup(x => x.ValidateGuid(ValidCustomerId)).Returns(CustomerGuid);

            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Diversity>(_request)).Returns(Task.FromResult(_diversity));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postDiversityHttpTriggerService.Setup(x => x.DoesDiversityDetailsExistForCustomer(DiversityGuid)).Returns(false);

            //SetUpHttpResponseMessageHelper();
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
           _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _guidHelper.Setup(x => x.ValidateGuid(ValidCustomerId)).Returns(Guid.Empty);

            var result = await RunFunction(InValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityHasFailedValidation()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.Setup(x => x.ValidateResource(_diversity)).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityRequestIsInvalid()
        {
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Diversity>(_request)).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeConflict_WhenDiversityDetailsForCustomerExists()
        {
            _postDiversityHttpTriggerService.Setup(x => x.DoesDiversityDetailsExistForCustomer(CustomerGuid)).Returns(true);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateDiversityDetailRecord()
        {
            _postDiversityHttpTriggerService.Setup(x => x.CreateAsync(_diversity)).Returns(Task.FromResult<Models.Diversity>(null));

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _postDiversityHttpTriggerService.Setup(x => x.CreateAsync(_diversity)).Returns(Task.FromResult<Models.Diversity>(_diversity));

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        public async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await function.Run(
                _request,
                _log.Object,
                customerId).ConfigureAwait(false);
        }

        /*private void SetUpHttpResponseMessageHelper()
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

        }*/

    }
}
