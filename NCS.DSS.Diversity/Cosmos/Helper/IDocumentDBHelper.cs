using System;

namespace NCS.DSS.Diversity.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid diversityDetailId);
    }
}