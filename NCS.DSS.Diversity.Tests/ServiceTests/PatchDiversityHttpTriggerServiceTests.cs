using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ServiceBus;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.Tests.ServiceTests
{
    [TestFixture]
    public class PatchDiversityHttpTriggerServiceTests
    {
        private IPatchDiversityHttpTriggerService _DiversityHttpTriggerService;
        private Mock<IDiversityPatchService> _diversityPatchService;
        private Mock<ICosmosDbProvider> _cosmosDbProvider;
        private Mock<IDiversityServiceBusClient> _serviceBusClient;

        private string _json;
        private Models.Diversity _diversity;
        private DiversityPatch _diversityPatch;
        private readonly Guid _customerId = Guid.Parse("044d15fa-e776-4797-8f57-bd2484d5b4b4");
        private readonly Guid _diversityId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _diversityPatchService = new Mock<IDiversityPatchService>();
            _cosmosDbProvider = new Mock<ICosmosDbProvider>();
            _serviceBusClient = new Mock<IDiversityServiceBusClient>();
            _DiversityHttpTriggerService = new PatchDiversityHttpTriggerService(_cosmosDbProvider.Object, _diversityPatchService.Object, _serviceBusClient.Object);
            _diversityPatch = new DiversityPatch();
            _diversity = new Models.Diversity();

            _json = JsonConvert.SerializeObject(_diversityPatch);
        }

        [Test]
        public void PatchDiversityHttpTriggerServiceTests_PatchResource_ReturnsNullWhenDiversityJsonNullOrEmpty()
        {
            // Act
            var result = _DiversityHttpTriggerService.PatchResource(null, _diversityPatch);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void PatchDiversityHttpTriggerServiceTests_PatchResource_ReturnsNullWhenDiversityPatchNullOrEmpty()
        {
            // Act
            var result = _DiversityHttpTriggerService.PatchResource(_json, null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_UpdateAsync_ReturnsResourceWhenUpdated()
        {
            // Arrange
            var mockItemResponse = new Mock<ItemResponse<Models.Diversity>>();

            var mockDiversity = new Models.Diversity
            {
                DiversityId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid()
            };

            mockItemResponse
            .Setup(response => response.Resource)
            .Returns(mockDiversity);

            mockItemResponse
            .Setup(response => response.StatusCode)
            .Returns(HttpStatusCode.OK);

            _cosmosDbProvider.Setup(x => x.UpdateDiversityDetailAsync(_json, _diversityId)).Returns(Task.FromResult(mockItemResponse.Object));

            // Act
            var result = await _DiversityHttpTriggerService.UpdateCosmosAsync(_json, _diversityId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Diversity>());
        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_GetDiversityForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.GetDiversityDetailForCustomerToUpdateAsync(_customerId, _diversityId)).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _DiversityHttpTriggerService.GetDiversityForCustomerAsync(_customerId, _diversityId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_GetDiversityForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.GetDiversityDetailForCustomerToUpdateAsync(_customerId, _diversityId)).Returns(Task.FromResult(_json));

            // Act
            var result = await _DiversityHttpTriggerService.GetDiversityForCustomerAsync(_customerId, _diversityId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<string>());
        }
    }
}