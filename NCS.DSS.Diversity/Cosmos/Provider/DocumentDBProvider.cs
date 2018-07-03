using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Diversity.Cosmos.Client;
using NCS.DSS.Diversity.Cosmos.Helper;

namespace NCS.DSS.Diversity.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var query = client.CreateDocumentQuery<Models.Diversity>(collectionUri, new FeedOptions { MaxItemCount = 1 });
            var customerExists = query.Where(x => x.CustomerId == customerId).AsEnumerable().Any();

            return customerExists;
        }

        public async Task<Guid?> GetDiversityDetailIdForCustomerAsync(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var diversityDetailQuery = client
                ?.CreateDocumentQuery<Models.Diversity>(collectionUri, new FeedOptions {MaxItemCount = 1})
                .Where(x => x.CustomerId == customerId)
                .AsDocumentQuery();

            if (diversityDetailQuery == null)
                return null;

            var diversityDetails = await diversityDetailQuery.ExecuteNextAsync<Models.Diversity>();

            var diversityDetail = diversityDetails?.FirstOrDefault();

            return diversityDetail?.DiversityId;
        }

        public async Task<ResourceResponse<Document>> CreateDiversityDetailAsync(Models.Diversity diversity)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, diversity);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateDiversityDetailAsync(Models.Diversity diversity)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(diversity.DiversityId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, diversity);

            return response;
        }
    }

}