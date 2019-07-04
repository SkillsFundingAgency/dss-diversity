using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NCS.DSS.Diversity.Tests
{
    [TestFixture]
    public class GetDiversityHttpTriggerTests
    {
        private const string ValidCustomerId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidCustomerId = "1111111-2222-3333-4444-555555555555";
        private readonly Guid _diversityDetailId = Guid.Parse("aa57e39e-4469-4c79-a9e9-9cb4ef410382");
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";

        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IGetDiversityHttpTriggerService _getDiversityHttpTriggerService;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _getDiversityHttpTriggerService = Substitute.For<IGetDiversityHttpTriggerService>();

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            SetUpHttpResponseMessageHelper();
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesntExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeNoContent_WhenDiversityDetailDoesntExist()
        {

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            
            _getDiversityHttpTriggerService.GetDiversityDetailIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Guid?>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetDiversityHttpTrigger_ReturnsStatusCodeOk_WhenDiversityDetailExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            _getDiversityHttpTriggerService.GetDiversityDetailIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(_diversityDetailId).Result);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await GetDiversityHttpTrigger.Function.GetDiversityHttpTrigger.Run(
                _request, 
                _log, 
                customerId,
                _resourceHelper,
                _getDiversityHttpTriggerService,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }

        private void SetUpHttpResponseMessageHelper()
        {
            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            _httpResponseMessageHelper
                .Ok(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

        }
    }
}