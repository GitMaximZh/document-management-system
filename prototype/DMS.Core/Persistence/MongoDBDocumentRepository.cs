using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace DMS.Core.Persistence
{
    public class MongoDBDocumentRepository : IDocumentRepository
    {
        private const string IdKey = "Id";

        private IMongoClient client;
        private IMongoDatabase database;
        private IMongoCollection<BsonDocument> metadataCollection;
        private GridFSBucket fileBucket;
       
        public void Initialize()
        {
            client = new MongoClient("mongodb://mongo:27017");
            database = client.GetDatabase("rs-dms");
            metadataCollection = database.GetCollection<BsonDocument>("metadata");
            fileBucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = "documents",
                ChunkSizeBytes = 1048576, // 1MB  
            });
        }

        public void DeleteDocument(string documentKey)
        {
            metadataCollection.DeleteOne($"{{{IdKey}: '{documentKey}'}}");
            var file = fileBucket.Find(Builders<GridFSFileInfo>.Filter.Eq(e => e.Filename, documentKey)).FirstOrDefault();
            if(file != null)
                fileBucket.Delete(file.Id);
        }

        public Document GetDocument(string documentKey)
        {
            var bson = metadataCollection.Find($"{{{IdKey}: '{documentKey}'}}").FirstOrDefault();
            if (bson == null)
                return null;
            var metadata = ConvertBsonToMetadata(bson);
            var file = fileBucket.DownloadAsBytesByName(documentKey);
            return new Document(metadata.DocumentKey, metadata.Data, file);
        }

        public IEnumerable<Metadata> GetDocumentsMetadata()
        {
            return metadataCollection.Find(FilterDefinition<BsonDocument>.Empty).ToList()
                .Select(ConvertBsonToMetadata);
        }

        private Metadata ConvertBsonToMetadata(BsonDocument bson)
        {
            var dict = bson.Elements.Where(e => !e.Name.StartsWith("_")).ToDictionary(e => e.Name, e => e.Value.AsString);
            var key = dict[IdKey];
            dict.Remove(IdKey);
            return new Metadata(key, dict);
        }

        public void SetDocument(Document document)
        {
            var bson = new BsonDocument((Dictionary<string, string>)document.Metadata.Data);
            bson.Add(IdKey, document.Key);

            metadataCollection.ReplaceOne($"{{{IdKey}: '{document.Key}'}}", bson, new UpdateOptions {IsUpsert = true});
            fileBucket.UploadFromBytes(document.Key, document.Binary);
        }
    }
}
