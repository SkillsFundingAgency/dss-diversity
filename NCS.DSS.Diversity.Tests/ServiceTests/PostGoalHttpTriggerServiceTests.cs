using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Diversity.Tests.ServicesTests
{
    [TestFixture]
    public class PosDiversityHttpTriggerServiceTests
    {
        private IPostDiversityHttpTriggerService _DiversityHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private string _json;
        private Models.Diversity _Diversity;
        private readonly Guid _diversityId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _DiversityHttpTriggerService = Substitute.For<PostDiversityHttpTriggerService>(_documentDbProvider);
            _Diversity = Substitute.For<Models.Diversity>();
            _json = JsonConvert.SerializeObject(_Diversity);
        }

        [Test]
        public async Task PostActionPlanHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenActionPlanJsonIsNull()
        {
            // Act
            var result = await _DiversityHttpTriggerService.CreateAsync(Arg.Any<Models.Diversity>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PostActionPlanHttpTriggerServiceTests_CreateAsync_ReturnsResource()
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

            _documentDbProvider.CreateDiversityDetailAsync(_Diversity).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _DiversityHttpTriggerService.CreateAsync(_Diversity);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Diversity>(result);

        }
    }
}