using System.Collections.Generic;
using System.IO;

namespace DMS.Core.Persistence
{
    public interface IDocumentMetadataParser
    {
        string Extension { get; }
        IDictionary<string, string> Parse(Stream stream);
    }
}
