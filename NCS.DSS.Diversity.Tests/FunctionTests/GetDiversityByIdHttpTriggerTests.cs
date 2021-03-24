using System;
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
using NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service;
using Moq;
using Xunit;

namespace NCS.DSS.Diversity.Tests.FunctionTests
{

    public class GetDiversityByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDiversityId = "aa57e39e-4469-4c79-a9e9-9cb4ef410382";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private static readonly Guid CustomerGuid = Guid.NewGuid();
        private static readonly Guid DiversityGuid = Guid.NewGuid();

        private readonly Models.Diversity _diversity;
        private Mock<ILogger> _log;
        private DefaultHttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private readonly HttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly Mock<IGuidHelper> _guidHelper;
        private readonly Mock<IGetDiversityByIdHttpTriggerService> _getDiversityByIdHttpTriggerService;
        private readonly GetDiversityByIdHttpTrigger.Function.GetDiversityByIdHttpTrigger function;
        private IJsonHelper _jsonHelper;
        private Mock<ILoggerHelper> _loggerHelper;

        public GetDiversityByIdHttpTriggerTests()
        {
            _request = null;

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();

            _diversity = new Models.Diversity();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _getDiversityByIdHttpTriggerService = new Mock<IGetDiversityByIdHttpTriggerService>();

            _loggerHelper = new Mock<ILoggerHelper>();
            _jsonHelper = new JsonHelper();
            _guidHelper = new Mock<IGuidHelper>();

            function = new GetDiversityByIdHttpTrigger.Function.GetDiversityByIdHttpTrigger(_resourceHelper.Object,
                _getDiversityByIdHttpTriggerService.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper.Object);

            _httpRequestHelper.Setup(x => x.GetDssCorrelationId(_request)).Returns(ValidDssCorrelationId);
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _guidHelper.Setup(x => x.ValidateGuid(ValidCustomerId)).Returns(CustomerGuid);
            _guidHelper.Setup(x => x.ValidateGuid(ValidDiversityId)).Returns(DiversityGuid);            
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _guidHelper.Setup(x => x.ValidateGuid(ValidCustomerId)).Returns(Guid.Empty);

            // Act
            var result = await RunFunction(InValidId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenDiversityIdIsInvalid()
        {
            _guidHelper.Setup(x => x.ValidateGuid(ValidCustomerId)).Returns(Guid.Empty);

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesntExist()
        {
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenDiversityDetailDoesntExist()
        {

            _getDiversityByIdHttpTriggerService.Setup(x => x.GetDiversityDetailByIdAsync(CustomerGuid, DiversityGuid)).Returns(Task.FromResult<Models.Diversity>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeOk_WhenDiversityDetailExists()
        {
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(true));
            _getDiversityByIdHttpTriggerService.Setup(x => x.GetDiversityDetailByIdAsync(CustomerGuid, DiversityGuid)).Returns(Task.FromResult(_diversity));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string diversityId)
        {
            return await function.Run(
                _request,
                _log.Object,
                customerId,
                diversityId).ConfigureAwait(false);
        }
    }
}