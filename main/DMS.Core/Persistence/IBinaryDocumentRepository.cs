using DMS.Core.Domain;

namespace DMS.Core.Persistence
{
    public interface IBinaryDocumentRepository : IRepository
    {
        void AddDocument(DocumentKey key, byte[] document);
        byte[] GetDocument(DocumentKey key);
        void UpdateDocument(DocumentKey key, byte[] document);
        void RemoveDocument(DocumentKey key);
    }
}
