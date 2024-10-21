using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ServiceBus;
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
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private Models.Diversity _diversity;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            var serviceBusClient = new Mock<IServiceBusClient>();
            _diversityHttpTriggerService = new PostDiversityHttpTriggerService(_documentDbProvider.Object, serviceBusClient.Object);
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
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.Created, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.Setup(x => x.CreateDiversityDetailAsync(_diversity)).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _diversityHttpTriggerService.CreateAsync(_diversity);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Diversity>());

        }
    }
}