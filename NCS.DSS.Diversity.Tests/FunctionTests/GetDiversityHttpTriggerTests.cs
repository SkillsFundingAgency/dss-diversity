using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;


namespace NCS.DSS.Diversity.Tests.FunctionTests
{
    [TestFixture]
    public class GetDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidCustomerId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private static readonly Guid CustomerGuid = Guid.NewGuid();

        private Mock<IGetDiversityHttpTriggerService> _getDiversityHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILogger<GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger>> _log;

        private HttpRequest _request;
        private GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpContext().Request;

            _getDiversityHttpTriggerService = new Mock<IGetDiversityHttpTriggerService>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _resourceHelper = new Mock<IResourceHelper>();
            _log = new Mock<ILogger<GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger>>();

            _function = new GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger(
                _getDiversityHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                _log.Object);

            var diversityList = new List<Models.Diversity>();

            _httpRequestHelper.Setup(x => x.GetDssCorrelationId(_request)).Returns(ValidDssCorrelationId);
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getDiversityHttpTriggerService.Setup(x => x.GetDiversityDetailForCustomerAsync(It.IsAny<Guid>())).Returns(Task.FromResult(diversityList));
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeOK_WhenCustomerDoesntExist()
        {
            // Arrange
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);
            var resultResponse = result as JsonResult;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(resultResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeOK_WhenDiversityDetailDoesntExist()
        {
            // Arrange
            _getDiversityHttpTriggerService.Setup(x => x.GetDiversityDetailForCustomerAsync(CustomerGuid)).Returns(Task.FromResult<List<Models.Diversity>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);
            var resultResponse = result as JsonResult;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(resultResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeOk_WhenDiversityDetailExists()
        {
            // Arrange
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId);
            var resultResponse = result as JsonResult;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(resultResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(
                _request,
                customerId).ConfigureAwait(false);
        }
    }
}
