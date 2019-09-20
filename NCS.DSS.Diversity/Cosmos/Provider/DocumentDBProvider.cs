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
        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;
            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                    return true;
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }

        public bool DoesDiversityDetailsExistForCustomer(Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            var diversityDetailsForCustomerQuery = client.CreateDocumentQuery<Models.Diversity>(collectionUri, new FeedOptions { MaxItemCount = 1 });
            return diversityDetailsForCustomerQuery.Where(x => x.CustomerId == customerId).AsEnumerable().Any();
        }

        public async Task<Guid?> GetDiversityDetailIdForCustomerAsync(Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

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

        public async Task<Models.Diversity> GetDiversityDetailForCustomerAsync(Guid customerId, Guid diversityId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var diversityDetailQuery = client
                ?.CreateDocumentQuery<Models.Diversity>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId &&
                            x.DiversityId == diversityId)
                .AsDocumentQuery();

            if (diversityDetailQuery == null)
                return null;

            var diversityDetails = await diversityDetailQuery.ExecuteNextAsync<Models.Diversity>();

            return diversityDetails?.FirstOrDefault();
        }

        public async Task<ResourceResponse<Document>> CreateDiversityDetailAsync(Models.Diversity diversity)
        {

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, diversity);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateDiversityDetailAsync(Models.Diversity diversity)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(diversity.DiversityId.GetValueOrDefault());

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, diversity);

            return response;
        }
    }

}