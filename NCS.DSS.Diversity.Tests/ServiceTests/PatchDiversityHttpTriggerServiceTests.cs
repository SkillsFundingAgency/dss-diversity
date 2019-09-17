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
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.ServiceBus;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace NCS.DSS.Diversity.Tests.ServiceTests
{
   
    public class PatchDiversityHttpTriggerServiceTests
    {
        private IPatchDiversityHttpTriggerService _DiversityHttpTriggerService;
        private IDiversityPatchService _diversityPatchService;
        private IDocumentDBProvider _documentDbProvider;
        private IServiceBusClient _serviceBusClient;

        private string _json;
        private readonly Models.Diversity _diversity;
        private readonly DiversityPatch _diversityPatch;
        private readonly Guid _customerId = Guid.Parse("044d15fa-e776-4797-8f57-bd2484d5b4b4");
        private readonly Guid _diversityId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        public PatchDiversityHttpTriggerServiceTests()
        {
            _diversityPatchService = Substitute.For<IDiversityPatchService>();
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _serviceBusClient = Substitute.For<IServiceBusClient>();
            _DiversityHttpTriggerService = Substitute.For<PatchDiversityHttpTriggerService>(_documentDbProvider, _diversityPatchService, _serviceBusClient);
            _diversityPatch = new DiversityPatch();
            _diversity = new Models.Diversity();

            _json = JsonConvert.SerializeObject(_diversityPatch);
        }

        [Fact]
        public void PatchDiversityHttpTriggerServiceTests_PatchResource_ReturnsNullWhenDiversityJsonNullOrEmpty()
        {
            // Act
            var result = _DiversityHttpTriggerService.PatchResource(null, _diversityPatch);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void PatchDiversityHttpTriggerServiceTests_PatchResource_ReturnsNullWhenDiversityPatchNullOrEmpty()
        {
            // Act
            var result = _DiversityHttpTriggerService.PatchResource(_json, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PatchDiversityHttpTriggerServiceTests_UpdateAsync_ReturnsResourceWhenUpdated()
        {
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.UpdateDiversityDetailAsync(_json, _diversityId).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _DiversityHttpTriggerService.UpdateCosmosAsync(_json, _diversityId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Models.Diversity>(result);

        }

        [Fact]
        public async Task PatchDiversityHttpTriggerServiceTests_GetDiversityForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _documentDbProvider.GetDiversityDetailForCustomerToUpdateAsync(_customerId, _diversityId).Returns(Task.FromResult<string>(null).Result);

            // Act
            var result = await _DiversityHttpTriggerService.GetDiversityForCustomerAsync(_customerId, _diversityId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PatchDiversityHttpTriggerServiceTests_GetDiversityForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _documentDbProvider.GetDiversityDetailForCustomerToUpdateAsync(_customerId, _diversityId).Returns(Task.FromResult(_json).Result);

            // Act
            var result = await _DiversityHttpTriggerService.GetDiversityForCustomerAsync(_customerId, _diversityId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }
    }
}