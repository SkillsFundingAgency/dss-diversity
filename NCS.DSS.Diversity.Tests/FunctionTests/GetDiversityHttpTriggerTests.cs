using System;
using System.Collections.Generic;
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
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using Xunit;

namespace NCS.DSS.Diversity.Tests.FunctionTests
{
    public class GetDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidCustomerId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private static readonly Guid CustomerGuid = Guid.NewGuid();

        private readonly Models.Diversity _diversity;
        private Mock<ILogger> _log;
        private DefaultHttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private readonly HttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly Mock<IGuidHelper> _guidHelper;
        private readonly Mock<IGetDiversityHttpTriggerService> _getDiversityHttpTriggerService;
        private readonly GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger function;
        private IJsonHelper _jsonHelper;
        private Mock<ILoggerHelper> _loggerHelper;

        public GetDiversityHttpTriggerTests()
        {
            _request = null;

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();

            _diversity = new Models.Diversity();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _getDiversityHttpTriggerService = new Mock<IGetDiversityHttpTriggerService>();

            _loggerHelper = new Mock<ILoggerHelper>();
            _jsonHelper = new JsonHelper();
            _guidHelper = new Mock<IGuidHelper>();

            function = new GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger(_resourceHelper.Object,
                _getDiversityHttpTriggerService.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper.Object);

            _httpRequestHelper.Setup(x => x.GetDssCorrelationId(_request)).Returns(ValidDssCorrelationId);
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _guidHelper.Setup(x => x.ValidateGuid(ValidCustomerId)).Returns(CustomerGuid);
            var diversityList = new List<Models.Diversity>();
            _getDiversityHttpTriggerService.Setup(x => x.GetDiversityDetailForCustomerAsync(It.IsAny<Guid>())).Returns(Task.FromResult(diversityList));
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _guidHelper.Setup(x => x.ValidateGuid(ValidCustomerId)).Returns(Guid.Empty);

            // Act
            var result = await RunFunction(InValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesntExist()
        {
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenDiversityDetailDoesntExist()
        {
            _getDiversityHttpTriggerService.Setup(x => x.GetDiversityDetailForCustomerAsync(CustomerGuid)).Returns(Task.FromResult<List<Models.Diversity>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeOk_WhenDiversityDetailExists()
        {
            _resourceHelper.Setup(x => x.DoesCustomerExist(CustomerGuid)).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await function.Run(
                _request,
                _log.Object,
                customerId).ConfigureAwait(false);
        }
    }
}