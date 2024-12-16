using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ServiceBus;
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
    public class PostDiversityHttpTriggerServiceTests
    {
        private IPostDiversityHttpTriggerService _diversityHttpTriggerService;
        private Mock<ICosmosDbProvider> _cosmosDbProvider;
        private Models.Diversity _diversity;

        [SetUp]
        public void Setup()
        {
            _cosmosDbProvider = new Mock<ICosmosDbProvider>();
            var serviceBusClient = new Mock<IDiversityServiceBusClient>();
            _diversityHttpTriggerService = new PostDiversityHttpTriggerService(_cosmosDbProvider.Object, serviceBusClient.Object);
            _diversity = new Models.Diversity();
        }

        [Test]
        public async Task PostDiversityHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenDiversityJsonNull()
        {
            // Act
            var result = await _diversityHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PostDiversityHttpTriggerServiceTests_CreateAsync_ReturnsResource()
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
            .Returns(HttpStatusCode.Created);

            _cosmosDbProvider.Setup(x => x.CreateDiversityDetailAsync(_diversity)).Returns(Task.FromResult(mockItemResponse.Object));

            // Act
            var result = await _diversityHttpTriggerService.CreateAsync(_diversity);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Diversity>());

        }
    }
}