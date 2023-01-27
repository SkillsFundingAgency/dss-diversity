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
using Moq;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;
using NUnit.Framework;

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
        private Mock<IDiversityPatchService> _diversityPatchService;
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
        private Models.DiversityPatch _diversityPatch;
        private PatchDiversityHttpTrigger.Function.PatchDiversityHttpTrigger function;
        private string _json;

        [SetUp]
        public void Setup()
        {
            _diversity = new Models.Diversity();
            _diversityPatch = new Models.DiversityPatch();

            _request = null;

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();

            _diversity = new Models.Diversity();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _validate = new Mock<IValidate>();
            _patchDiversityHttpTriggerService = new Mock<IPatchDiversityHttpTriggerService>();
            _diversityPatchService = new Mock<IDiversityPatchService>();

            _loggerHelper = new Mock<ILoggerHelper>();
            _jsonHelper = new JsonHelper();
            _guidHelper = new Mock<IGuidHelper>();

            _json = JsonConvert.SerializeObject(_diversity);


            function = new PatchDiversityHttpTrigger.Function.PatchDiversityHttpTrigger(
                _resourceHelper.Object,
                _patchDiversityHttpTriggerService.Object,
                _validate.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper.Object);


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
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {

            var result = await RunFunction(InValidId, DiversityId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenDiversityIdIsInvalid()
        {

            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityHasFailedValidation()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.Setup(x => x.ValidateResource(_diversityPatch)).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenDiversityRequestIsInvalid()
        {
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.DiversityPatch>(_request)).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(false));
            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }


        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateDiversityRecord()
        {

            _patchDiversityHttpTriggerService.Setup(x => x.GetDiversityForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_diversity.ToString()));
            _patchDiversityHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), DiversityGuid)).Returns(Task.FromResult<Models.Diversity>(null));

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }


        [Test]
        public async Task PatchDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsInvalid()
        {
            _patchDiversityHttpTriggerService.Setup(x => x.GetDiversityForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_diversity.ToString()));
            _patchDiversityHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.DiversityPatch>())).Returns(_json);
            _patchDiversityHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), DiversityGuid)).Returns(Task.FromResult(_diversity));

            var result = await RunFunction(ValidCustomerId, DiversityId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        public async Task<HttpResponseMessage> RunFunction(string customerId, string diversityId)
        {
            return await function.Run(
                _request,
                _log.Object,
                customerId,
                diversityId).ConfigureAwait(false);
        }
    }
}
