using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Diversity.Cosmos
{
    public class DatabaseClient : IDatabaseClient
    {
        private Uri collectionLink;
        private DocumentClient documentClient;

        public Uri CreateCollectionLink()
        {
            if (collectionLink != null)
                return collectionLink;

        collectionLink = UriFactory.CreateDocumentCollectionUri(
            ConfigurationManager.AppSettings["DatabaseId"], 
            ConfigurationManager.AppSettings["CollectionId"]);

            return collectionLink;
        }
        
        public DocumentClient CreateDocumentClient()
        {
            if (documentClient != null)
                return documentClient;

            documentClient = new DocumentClient(new Uri(
                ConfigurationManager.AppSettings["Endpoint"]),
                ConfigurationManager.AppSettings["Key"]);

            return documentClient;
        }
        

    }
}
