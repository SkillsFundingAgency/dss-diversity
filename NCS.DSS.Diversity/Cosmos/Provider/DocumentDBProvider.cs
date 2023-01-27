using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.Common.Standard;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Diversity.Cosmos.Client;
using NCS.DSS.Diversity.Cosmos.Helper;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Diversity.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {

        private string _customerJson;

        public string GetCustomerJson()
        {
            return _customerJson;
        }

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
                {
                    _customerJson = response.Resource.ToString();
                    return true;
                }
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
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

        public async Task<Models.Diversity> GetDiversityDetailIdForCustomerAsync(Guid customerId, Guid diversityId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var diversityDetailQuery = client
                ?.CreateDocumentQuery<Models.Diversity>(collectionUri, new FeedOptions {MaxItemCount = 1})
                .Where(x => x.CustomerId == customerId &&
                            x.DiversityId == diversityId)
                .AsDocumentQuery();

            if (diversityDetailQuery == null)
                return null;

            var diversityDetails = await diversityDetailQuery.ExecuteNextAsync<Models.Diversity>();

            var diversityDetail = diversityDetails?.FirstOrDefault();

            return diversityDetail;
        }

        public async Task<List<Models.Diversity>> GetDiversityDetailsForCustomerAsync(Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var diversityDetailQuery = client
                ?.CreateDocumentQuery<Models.Diversity>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId)
                .AsDocumentQuery();

            if (diversityDetailQuery == null)
                return null;

            var diversityDetails = new List<Models.Diversity>();

            while (diversityDetailQuery.HasMoreResults)
            {
                var response = await diversityDetailQuery.ExecuteNextAsync<Models.Diversity>();
                diversityDetails.AddRange(response);
            }

            return diversityDetails.Any() ? diversityDetails : null;
        }
        
        public async Task<string> GetDiversityDetailForCustomerToUpdateAsync(Guid customerId, Guid diversityId)
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

            var diversityDetails = await diversityDetailQuery.ExecuteNextAsync();

            return diversityDetails?.FirstOrDefault()?.ToString();
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

        public async Task<ResourceResponse<Document>> UpdateDiversityDetailAsync(string diversityJson, Guid diversityId)
        {
            if (string.IsNullOrEmpty(diversityJson))
                return null;

            var documentUri = DocumentDBHelper.CreateDocumentUri(diversityId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var diversityDocumentJObject = JObject.Parse(diversityJson);

            var response = await client.ReplaceDocumentAsync(documentUri, diversityDocumentJObject);

            return response;
        }
    }

}