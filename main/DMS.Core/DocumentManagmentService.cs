using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.Core.Domain;
using DMS.Core.Domain.Search;
using DMS.Core.Exceptions;
using DMS.Core.Persistence;

namespace DMS.Core
{
    public class DocumentManagmentService<TDocument, TDocumentMetadata> 
        where TDocument : Document<TDocumentMetadata>, new()
        where TDocumentMetadata : DocumentMetadata
    {
        public IBinaryDocumentRepository BinaryDocumentRepository { get; private set; }
        public DocumentMetadataRepository<TDocumentMetadata> DocumentMetadataRepository { get; private set; }

        public void AddDocument(TDocument document)
        {
            if(DocumentMetadataRepository.DocumentExist(document.Key))
                throw new DMSException("Document with such an ID already exists.");

            var metadata = document.GetMetadata();
            var binaries = document.GetBinaries();

            using (var scope = new DMSTransactionScope())
            {
                Parallel.ForEach(binaries, binary => BinaryDocumentRepository.AddDocument(binary.Key, binary.Value));
                
                DocumentMetadataRepository.AddDocumentMetadata(metadata);
                scope.Complete();
            }
        }

        public TDocument GetDocument(DocumentKey documentKey)
        {
            var metadata = DocumentMetadataRepository.GetDocumentMetadata(documentKey);
            return GetDocument(metadata);
        }

        private TDocument GetDocument(TDocumentMetadata metadata)
        {
            var refernces = metadata.References.AsParallel().ToDictionary(e => e, e => BinaryDocumentRepository.GetDocument(e));

            var document = new TDocument();
            document.Initialize(metadata, refernces);
            return document;
        }

        public void RemoveDocument(DocumentKey documentKey)
        {
            var metadata = DocumentMetadataRepository.GetDocumentMetadata(documentKey);
            using (var scope = new DMSTransactionScope())
            {
                Parallel.ForEach(metadata.References, refKey => BinaryDocumentRepository.RemoveDocument(refKey));

                DocumentMetadataRepository.RemoveDocumentMetadata(documentKey);
                scope.Complete();
            }
        }

        public IEnumerable<TDocument> FindDocuments(ISearchCriteria criteria)
        {
            var metadata = FindDocumentsMetadata(criteria);
            return metadata.Select(GetDocument);
        }

        public IEnumerable<TDocumentMetadata> FindDocumentsMetadata(ISearchCriteria criteria)
        {
            return DocumentMetadataRepository.FinDocumentsMetadata(criteria);
        }
    }
}
