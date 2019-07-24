using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ServiceBus;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Diversity.Tests.ServiceTests
{
   
    public class PostDiversityHttpTriggerServiceTests
    {
        private readonly IPostDiversityHttpTriggerService _diversityHttpTriggerService;
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly Models.Diversity _diversity;

        public PostDiversityHttpTriggerServiceTests()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            var serviceBusClient = Substitute.For<IServiceBusClient>();
            _diversityHttpTriggerService = Substitute.For<PostDiversityHttpTriggerService>(_documentDbProvider, serviceBusClient);
            _diversity = Substitute.For<Models.Diversity>();
        }

        [Fact]
        public async Task PostDiversityHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenDiversityJsonNull()
        {
            // Act
            var result = await _diversityHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PostDiversityHttpTriggerServiceTests_CreateAsync_ReturnsResource()
        {
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

            _documentDbProvider.CreateDiversityDetailAsync(Arg.Is(_diversity)).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _diversityHttpTriggerService.CreateAsync(_diversity);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Models.Diversity>(result);

        }
    }
}