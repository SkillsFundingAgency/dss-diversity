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
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.Diversity.Tests.ServiceTests
{
    [TestFixture]
    public class PatchDiversityHttpTriggerServiceTests
    {
        private IPatchDiversityHttpTriggerService _DiversityHttpTriggerService;
        private IDiversityPatchService _diversityPatchService;
        private IDocumentDBProvider _documentDbProvider;
        private string _json;
        private Models.Diversity _Diversity;
        private DiversityPatch _DiversityPatch;
        private readonly Guid _DiversityId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _diversityPatchService = Substitute.For<IDiversityPatchService>();
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _DiversityHttpTriggerService = Substitute.For<PatchDiversityHttpTriggerService>( _documentDbProvider, _diversityPatchService);
            _DiversityPatch = Substitute.For<DiversityPatch>();
            _Diversity = Substitute.For<Models.Diversity>();

            _json = JsonConvert.SerializeObject(_DiversityPatch);
            _diversityPatchService.Patch(_json, _DiversityPatch).Returns(_Diversity.ToString());
        }

        [Test]
        public void PatchDiversityHttpTriggerServiceTests_PatchResource_ReturnsNullWhenDiversityJsonIsNullOrEmpty()
        {
            // Act
            var result = _DiversityHttpTriggerService.PatchResource(null, Arg.Any<DiversityPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void PatchDiversityHttpTriggerServiceTests_PatchResource_ReturnsNullWhenDiversityPatchIsNullOrEmpty()
        {
            // Act
            var result = _DiversityHttpTriggerService.PatchResource(Arg.Any<string>(), null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenDiversityIsNullOrEmpty()
        {
            // Act
            var result = await _DiversityHttpTriggerService.UpdateCosmosAsync(null, _DiversityId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenDiversityPatchServicePatchJsonIsNullOrEmpty()
        {
            _diversityPatchService.Patch(Arg.Any<string>(), Arg.Any<DiversityPatch>()).ReturnsNull();

            // Act
            var result = await _DiversityHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), _DiversityId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            _documentDbProvider.UpdateDiversityDetailAsync(Arg.Any<string>(), Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _DiversityHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), _DiversityId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.CreateDiversityDetailAsync(Arg.Any<Models.Diversity>()).Returns(Task.FromResult(new ResourceResponse<Document>(null)).Result);

            // Act
            var result = await _DiversityHttpTriggerService.UpdateCosmosAsync(_Diversity.ToString(), _DiversityId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
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

            _documentDbProvider.UpdateDiversityDetailAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _DiversityHttpTriggerService.UpdateCosmosAsync(_Diversity.ToString(), _DiversityId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Diversity>(result);

        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_GetDiversityForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _documentDbProvider.GetDiversityDetailForCustomerToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _DiversityHttpTriggerService.GetDiversityForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchDiversityHttpTriggerServiceTests_GetDiversityForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _documentDbProvider.GetDiversityDetailForCustomerToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_json).Result);

            // Act
            var result = await _DiversityHttpTriggerService.GetDiversityForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}