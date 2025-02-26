using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.Tests.FunctionTests
{
    [TestFixture]
    public class PatchDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string DiversityId = "111a3e1c-2516-4a6b-9fc1-c85e578ac099";
        private static readonly Guid CustomerGuid = Guid.NewGuid();
        private static readonly Guid DiversityGuid = Guid.NewGuid();

        private Mock<IPatchDiversityHttpTriggerService> _patchDiversityHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<ILogger<PatchDiversityHttpTrigger.Function.PatchDiversityHttpTrigger>> _log;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IValidate> _validate;
        private Mock<IDynamicHelper> _dynamicHelper;

        private Models.Diversity _diversity;
        private Models.DiversityPatch _diversityPatch;
        private HttpRequest _request;
        private string _json;
        private PatchDiversityHttpTrigger.Function.PatchDiversityHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _diversity = new Models.Diversity();
            _diversityPatch = new Models.DiversityPatch();
            _request = new DefaultHttpContext().Request;
            _diversity = new Models.Diversity();

            _patchDiversityHttpTriggerService = new Mock<IPatchDiversityHttpTriggerService>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _log = new Mock<ILogger<PatchDiversityHttpTrigger.Function.PatchDiversityHttpTrigger>>();
            _resourceHelper = new Mock<IResourceHelper>();
            _validate = new Mock<IValidate>();
            _dynamicHelper = new Mock<IDynamicHelper>();

            _json = JsonConvert.SerializeObject(_diversity);

            _function = new PatchDiversityHttpTrigger.Function.PatchDiversityHttpTrigger(
                _patchDiversityHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                _log.Object,
                _validate.Object,
                _dynamicHelper.Object);

            _httpRequestHelper.Setup(x => x.GetDssCorrelationId(_request)).Returns(ValidDssCorrelationId);
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Diversity>(_request)).Returns(Task.FromResult(_diversity));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.DiversityPatch>(_request)).Returns(Task.FromResult(_diversityPatch));
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, DiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenDiversityIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityHasFailedValidation()
        {
            // Arrange
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.Setup(x => x.ValidateResource(_diversityPatch)).Returns(validationResults);

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.DiversityPatch>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToPatchDiversityResource()
        {
            // Arrange
            string patchedDiversity = null;
            _patchDiversityHttpTriggerService.Setup(x => x.GetDiversityForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_diversity.ToString()));
            _patchDiversityHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<DiversityPatch>())).Returns(patchedDiversity);

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateDiversityRecord()
        {
            // Arrange
            string patchedDiversity = $"{{ DiversityId = {Guid.NewGuid()} }}";
            _patchDiversityHttpTriggerService.Setup(x => x.GetDiversityForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_diversity.ToString()));
            _patchDiversityHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<DiversityPatch>())).Returns(patchedDiversity);
            _patchDiversityHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), DiversityGuid)).Returns(Task.FromResult<Models.Diversity>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsInvalid()
        {
            // Arrange
            _patchDiversityHttpTriggerService.Setup(x => x.GetDiversityForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_diversity.ToString()));
            _patchDiversityHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.DiversityPatch>())).Returns(_json);
            _patchDiversityHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), DiversityGuid)).Returns(Task.FromResult(_diversity));

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        public async Task<IActionResult> RunFunction(string customerId, string diversityId)
        {
            return await _function.Run(
                _request,
                customerId,
                diversityId).ConfigureAwait(false);
        }
    }
}
