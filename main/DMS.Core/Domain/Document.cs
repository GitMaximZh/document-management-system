using System;
using System.Collections.Generic;
using System.Text;

namespace DMS.Core.Domain
{
    public abstract class Document<TDocumentMetadata> where TDocumentMetadata : DocumentMetadata
    {
        public DocumentKey Key { get; private set; }

        public Document(DocumentKey key)
        {
            Key = key;
        }

        public abstract void Initialize(TDocumentMetadata metadata, IDictionary<DocumentKey, byte[]> binaries);

        public abstract TDocumentMetadata GetMetadata();

        public abstract IDictionary<DocumentKey, byte[]> GetBinaries();
    }
}
