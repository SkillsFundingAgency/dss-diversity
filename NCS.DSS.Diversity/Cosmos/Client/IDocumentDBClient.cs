using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Diversity.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}