using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace DMS.Core.Persistence
{
    public class WebAPIDocumentRepository : IDocumentRepository
    {
        private readonly HttpClient client = new HttpClient();
        private IDictionary<string, Metadata> metadata = new Dictionary<string, Metadata>();

        public void Initialize()
        {
            client.BaseAddress = new Uri("http://localhost:5001/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void DeleteDocument(string documentKey)
        {
            var response = client.DeleteAsync($"documents?key={documentKey}").Result;
            response.EnsureSuccessStatusCode();
        }

        public Document GetDocument(string documentKey)
        {
            if (!metadata.ContainsKey(documentKey))
                throw new ArgumentException($"Unknown document'{documentKey}'");

            Document result = null;
            var response = client.GetAsync($"documents/download?key={documentKey}").Result;
            if (response.IsSuccessStatusCode)
            {
                result = new Document(documentKey, metadata[documentKey].Data, response.Content.ReadAsByteArrayAsync().Result);
            }
            return result;
        }

        public IEnumerable<Metadata> GetDocumentsMetadata()
        {
            IEnumerable<Metadata> result = null;
            var response = client.GetAsync("documents").Result;
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<IEnumerable<Metadata>>(
                    response.Content.ReadAsStringAsync().Result);
                metadata = result.ToDictionary(e => e.DocumentKey, e => e);
            }
            return result;
        }

        public void SetDocument(Document document)
        {
            var response = client.PostAsync("documents",
                new StringContent(JsonConvert.SerializeObject(document.Metadata), Encoding.UTF8, "application/json")).Result;
            response.EnsureSuccessStatusCode();

            var bytes = new ByteArrayContent(document.Binary);

            var multiContent = new MultipartFormDataContent();
            multiContent.Add(bytes, "file", document.Key);

            response = client.PostAsync("documents/upload", multiContent).Result;
            response.EnsureSuccessStatusCode();
        }
    }
}
