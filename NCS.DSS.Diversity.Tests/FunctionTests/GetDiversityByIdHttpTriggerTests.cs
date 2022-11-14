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
using NSubstitute;
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

        private readonly ILogger _log;
        private readonly Models.Diversity _diversity;
        private readonly HttpRequest _request;
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IGuidHelper _guidHelper;
        private readonly IGetDiversityByIdHttpTriggerService _getDiversityByIdHttpTriggerService;
        private readonly GetDiversityByIdHttpTrigger.Function.GetDiversityByIdHttpTrigger _getDiversityByIdHttpTrigger;
        
        public GetDiversityByIdHttpTriggerTests()
        {
            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _diversity = Substitute.For<Models.Diversity>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _getDiversityByIdHttpTriggerService = Substitute.For<IGetDiversityByIdHttpTriggerService>();

            var loggerHelper = Substitute.For<ILoggerHelper>();
            var jsonHelper = Substitute.For<IJsonHelper>();
            _guidHelper = Substitute.For<IGuidHelper>();

            _getDiversityByIdHttpTrigger = Substitute.For<GetDiversityByIdHttpTrigger.Function.GetDiversityByIdHttpTrigger>(_resourceHelper,
                _getDiversityByIdHttpTriggerService,
                loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                jsonHelper,
                _guidHelper);

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            //_guidHelper.ValidateGuid(ValidCustomerId).Returns(CustomerGuid);
            if (!Guid.TryParse(ValidCustomerId, out var CustomerGuid))
            { }
            //_guidHelper.ValidateGuid(ValidDiversityId).Returns(DiversityGuid);
            if (!Guid.TryParse(ValidDiversityId, out var DiversityGuid))
            { }
            SetUpHttpResponseMessageHelper();
            
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            //_guidHelper.ValidateGuid(ValidCustomerId).Returns(Guid.Empty);
            if (!Guid.TryParse(ValidCustomerId, out var CustomerGuid))
            { }
            // Act
            var result = await RunFunction(InValidId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenDiversityIdIsInvalid()
        {
            //_guidHelper.ValidateGuid(ValidDiversityId).Returns(Guid.Empty);
            if (!Guid.TryParse(ValidDiversityId, out var DiversityGuid))
            { }
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesntExist()
        {
            _resourceHelper.DoesCustomerExist(CustomerGuid).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenDiversityDetailDoesntExist()
        {

            _getDiversityByIdHttpTriggerService.GetDiversityDetailByIdAsync(CustomerGuid, DiversityGuid).Returns(Task.FromResult<Models.Diversity>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenDiversityDetailExists()
        {
            _resourceHelper.DoesCustomerExist(CustomerGuid).Returns(true);
            _getDiversityByIdHttpTriggerService.GetDiversityDetailByIdAsync(CustomerGuid, DiversityGuid).Returns(Task.FromResult(_diversity).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidDiversityId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string diversityId)
        {
            return await _getDiversityByIdHttpTrigger.Run(
                _request,
                _log,
                customerId,
                diversityId).ConfigureAwait(false);
        }

        private void SetUpHttpResponseMessageHelper()
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
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

        }
    }
}