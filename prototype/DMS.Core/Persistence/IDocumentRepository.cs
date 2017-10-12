using System.Collections.Generic;

namespace DMS.Core.Persistence
{
    public interface IDocumentRepository
    {
        void DeleteDocument(string documentKey);
        Document GetDocument(string documentKey);
        IEnumerable<Metadata> GetDocumentsMetadata();
        void Initialize();
        void SetDocument(Document document);
    }
}