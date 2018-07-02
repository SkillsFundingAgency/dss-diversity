using System;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Diversity.Cosmos
{
    public interface IDatabaseClient
    {
        Uri CreateCollectionLink();
        DocumentClient CreateDocumentClient();
    }
}