using System;
using System.Collections.Generic;
using System.IO;
using DMS.Core;
using DMS.Core.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS.WebAPI
{
    [Route("documents")]
    public class DocumentsController : Controller
    {
        private readonly MongoDBDocumentRepository repository;

        public DocumentsController()
        {
            repository = new MongoDBDocumentRepository();
            repository.Initialize();
        }

        // GET documents
        [HttpGet, Route("")]
        public IEnumerable<Metadata> GetMetadata()
        {
            return repository.GetDocumentsMetadata();
        }

        // POST documents
        [HttpPost("")]
        public IActionResult SetDocument([FromBody]Metadata document)
        {
            repository.SetDocument(new Document(document.DocumentKey, document.Data, new byte[] { }));
            return Ok(document.DocumentKey);
        }

        // POST documents/upload
        [HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            
            var document = repository.GetDocument(Path.GetFileNameWithoutExtension(file.FileName));
            if (document == null)
                throw new ArgumentException($"Can not find metadata for document '{file.FileName}'.");

            using (var stream = new MemoryStream())
            {
                file.OpenReadStream().CopyTo(stream);
                document.Binary = stream.ToArray();
                repository.SetDocument(document);

                return Ok(document.Key);
            }
        }

        // Get documents/download?key=test-case1
        [HttpGet("download")]
        public IActionResult Download([FromQuery(Name = "key")] string documentKey)
        {
            var document = repository.GetDocument(documentKey);
            if(document == null)
                throw new ArgumentException($"Can not find document '{documentKey}'");
            using (var stream = new MemoryStream(document.Binary))
            {
                var response = File(stream.ToArray(), "application/json"); // FileStreamResult //"application/octet-stream"
                return response;
            }
        }

        // DELETE documents?key=test-case1
        [HttpDelete("")]
        public void Delete([FromQuery(Name = "key")] string documentKey)
        {
            repository.DeleteDocument(documentKey);
        }
    }
}
