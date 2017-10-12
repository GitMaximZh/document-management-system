using System;
using System.Collections.Generic;
using System.Text;

namespace DMS.Core.Domain
{
    public class DocumentMetadata
    {
        public DocumentKey Key { get; private set; }
        public IEnumerable<DocumentKey> References { get; private set; }

        public DocumentMetadata(DocumentKey key)
        {
            Key = key;
        }
    }
}
