using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DMS.Core
{
    public class Document
    {
        public string Key { get; private set; }
        public Metadata Metadata { get; private set; }
        public byte[] Binary { get; set; }

        public Document(string key, IDictionary<string, string> metadata, byte[] binary)
        {
            Key = key;
            Metadata = new Metadata(key, metadata);
            Binary = binary;
        }
    }

    public class Metadata
    {
        public string DocumentKey { get; private set; }
        public IDictionary<string, string> Data { get; private set; }

        public Metadata(string documentKey, IDictionary<string, string> data)
        {
            DocumentKey = documentKey;
            Data = data;
        }
    }
}
