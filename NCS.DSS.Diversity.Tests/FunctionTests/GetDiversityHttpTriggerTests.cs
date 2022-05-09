using System;
using System.Collections.Generic;
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
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Diversity.Tests.FunctionTests
{
    public class GetDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidCustomerId = "1111111-2222-3333-4444-555555555555";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private static readonly Guid CustomerGuid = Guid.NewGuid();

        private readonly ILogger _log;
        private readonly HttpRequest _request;
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IGuidHelper _guidHelper;
        private readonly IGetDiversityHttpTriggerService _getDiversityHttpTriggerService;
        private readonly GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger _getDiversityHttpTrigger;

        public GetDiversityHttpTriggerTests()
        {
            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _getDiversityHttpTriggerService = Substitute.For<IGetDiversityHttpTriggerService>();

            var loggerHelper = Substitute.For<ILoggerHelper>();
            var jsonHelper = Substitute.For<IJsonHelper>();
            _guidHelper = Substitute.For<IGuidHelper>();

            _getDiversityHttpTrigger = Substitute.For<GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger>(_resourceHelper,
                _getDiversityHttpTriggerService,
                loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                jsonHelper,
                _guidHelper);


            var diversityList = new List<Models.Diversity>();
            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns("9999999999");
            _guidHelper.ValidateGuid(ValidCustomerId).Returns(CustomerGuid);
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _getDiversityHttpTriggerService.GetDiversityDetailForCustomerAsync(Arg.Any<Guid>()).Returns(Task.FromResult(diversityList).Result);

            SetUpHttpResponseMessageHelper();
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenSubcontractorIdIsNotProvided()
        {
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _guidHelper.ValidateGuid(ValidCustomerId).Returns(Guid.Empty);

            // Act
            var result = await RunFunction(InValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesntExist()
        {
            _resourceHelper.DoesCustomerExist(CustomerGuid).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenDiversityDetailDoesntExist()
        {
            _getDiversityHttpTriggerService.GetDiversityDetailForCustomerAsync(CustomerGuid).Returns(Task.FromResult<List<Models.Diversity>>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeOk_WhenDiversityDetailExists()
        {
            _resourceHelper.DoesCustomerExist(CustomerGuid).Returns(true);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await _getDiversityHttpTrigger.Run(
                _request,
                _log,
                customerId).ConfigureAwait(false);
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