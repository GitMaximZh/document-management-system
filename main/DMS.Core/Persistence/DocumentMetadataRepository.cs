using System.Collections.Generic;
using DMS.Core.Domain;
using DMS.Core.Domain.Search;

namespace DMS.Core.Persistence
{
    public interface DocumentMetadataRepository<TDocumentMetadata> : IRepository
        where TDocumentMetadata: DocumentMetadata
    {
        void AddDocumentMetadata(TDocumentMetadata metadata);
        TDocumentMetadata GetDocumentMetadata(DocumentKey key);
        bool DocumentExist(DocumentKey key);
        void UpdateDocumentMetadata(TDocumentMetadata metadata);
        void RemoveDocumentMetadata(DocumentKey key);

        void RegisterSearchVisitor<T>(ISearchVisitor<T> visitor) where T : ISearchCriteria;
        IEnumerable<TDocumentMetadata> FinDocumentsMetadata(ISearchCriteria criteria);
    }
}
