using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.Tests.FunctionTests
{
    [TestFixture]
    public class GetDiversityByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDiversityId = "aa57e39e-4469-4c79-a9e9-9cb4ef410382";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private static readonly Guid CustomerGuid = Guid.NewGuid();
        private static readonly Guid DiversityGuid = Guid.NewGuid();

        private Mock<IGetDiversityByIdHttpTriggerService> _getDiversityByIdHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILogger<GetDiversityByIdHttpTrigger.Function.GetDiversityByIdHttpTrigger>> _log;

        private Models.Diversity _diversity;
        private HttpRequest _request;
        private GetDiversityByIdHttpTrigger.Function.GetDiversityByIdHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _getDiversityByIdHttpTriggerService = new Mock<IGetDiversityByIdHttpTriggerService>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _resourceHelper = new Mock<IResourceHelper>();
            _log = new Mock<ILogger<GetDiversityByIdHttpTrigger.Function.GetDiversityByIdHttpTrigger>>();

            _diversity = new Models.Diversity();
            _request = new DefaultHttpContext().Request;

            _function = new GetDiversityByIdHttpTrigger.Function.GetDiversityByIdHttpTrigger(
                _getDiversityByIdHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                _log.Object
                );

            _httpRequestHelper.Setup(x => x.GetDssCorrelationId(_request)).Returns(ValidDssCorrelationId);
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidDiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenDiversityIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesntExist()
        {
            // Arrange
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenDiversityDetailDoesntExist()
        {
            // Arrange
            _getDiversityByIdHttpTriggerService.Setup(x => x.GetDiversityDetailByIdAsync(CustomerGuid, DiversityGuid)).Returns(Task.FromResult<Models.Diversity>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenDiversityDetailExists()
        {
            // Arrange
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(true));
            _getDiversityByIdHttpTriggerService.Setup(x => x.GetDiversityDetailByIdAsync(CustomerGuid, DiversityGuid)).Returns(Task.FromResult(_diversity));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        private async Task<IActionResult> RunFunction(string customerId, string diversityId)
        {
            return await _function.Run(
                _request,
                customerId,
                diversityId).ConfigureAwait(false);
        }
    }
}
