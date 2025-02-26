using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

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
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILogger<PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger>> _log;
        private Mock<IValidate> _validate;
        private Mock<IDynamicHelper> _dynamicHelper;

        private HttpRequest _request;
        private Models.Diversity _diversity;
        private PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpContext().Request;
            _diversity = new Models.Diversity();

            _postDiversityHttpTriggerService = new Mock<IPostDiversityHttpTriggerService>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _resourceHelper = new Mock<IResourceHelper>();
            _log = new Mock<ILogger<PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger>>();
            _validate = new Mock<IValidate>();
            _dynamicHelper = new Mock<IDynamicHelper>();

            _function = new PostDiversityHttpTrigger.Function.PostDiversityHttpTrigger(
                _postDiversityHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                _log.Object,
                _validate.Object,
                _dynamicHelper.Object);

            _httpRequestHelper.Setup(x => x.GetDssCorrelationId(_request)).Returns(ValidDssCorrelationId);
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Diversity>(_request)).Returns(Task.FromResult(_diversity));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postDiversityHttpTriggerService.Setup(x => x.DoesDiversityDetailsExistForCustomer(DiversityGuid)).Returns(Task.FromResult(false));
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityHasFailedValidation()
        {
            // Arrange
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.Setup(x => x.ValidateResource(_diversity)).Returns(validationResults);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Diversity>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenDiversityDetailsForCustomerExists()
        {
            // Arrange
            _postDiversityHttpTriggerService.Setup(x => x.DoesDiversityDetailsExistForCustomer(CustomerGuid)).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateDiversityDetailRecord()
        {
            // Arrange
            _postDiversityHttpTriggerService.Setup(x => x.CreateAsync(_diversity)).Returns(Task.FromResult<Models.Diversity>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostDiversityHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _postDiversityHttpTriggerService.Setup(x => x.CreateAsync(_diversity)).Returns(Task.FromResult<Models.Diversity>(_diversity));

            // Act
            var result = await RunFunction(ValidCustomerId);
            var resultResponse = result as JsonResult;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(resultResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        public async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(
                _request,
                customerId).ConfigureAwait(false);
        }
    }
}
