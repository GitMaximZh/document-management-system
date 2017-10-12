using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DMS.Core.Persistence
{
    public class LocalDocumentRepository : IDocumentRepository
    {
        private DirectoryInfo directory;

        public string DirectoryPath { get; }
        public IDocumentMetadataParser Parser { get; }

        public LocalDocumentRepository(string directoryPath, IDocumentMetadataParser parser)
        {
            DirectoryPath = directoryPath;
            Parser = parser;
        }

        public void Initialize()
        {
            directory = Directory.CreateDirectory(DirectoryPath);
        }

        public void DeleteDocument(string documentKey)
        {
            File.Delete(GetFileName(documentKey));
        }

        public Document GetDocument(string documentKey)
        {
            var fileName = GetFileName(documentKey);
            if (!File.Exists(fileName))
                return null;
            var bytes = File.ReadAllBytes(fileName);
            using (var stream = new MemoryStream(bytes))
            {
                return new Document(documentKey, Parser.Parse(stream), bytes);
            }
        }

        public IEnumerable<Metadata> GetDocumentsMetadata()
        {
            return directory.GetFiles($"*.{Parser.Extension}")
                .Select(e =>
                {
                    using (var stream = e.OpenRead())
                    {
                        return new Metadata(Path.GetFileNameWithoutExtension(e.Name), Parser.Parse(stream));
                    }
                }).ToList();
        }

        public void SetDocument(Document document)
        {
            var fileName = GetFileName(document.Key);
            using (var stream = File.OpenWrite(fileName))
            {
                stream.Write(document.Binary, 0, document.Binary.Length);
            }
        }

        private string GetFileName(string documentKey)
            => Path.Combine(directory.FullName, $"{documentKey}.{Parser.Extension}");
    }
}
