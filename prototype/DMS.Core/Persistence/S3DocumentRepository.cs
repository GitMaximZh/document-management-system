using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace DMS.Core.Persistence
{
    public class S3DocumentRepository : IDocumentRepository
    {
        private const string BUCKET = "rs-dms";
        private AmazonS3Client s3Client = new AmazonS3Client(
            new BasicAWSCredentials("AKIAI4B7GDTVFVM23L4A", "OPWQJ2RhxNllgs9N/L6GkJezym7AfuXQLnKrpSY5"),
            RegionEndpoint.EUCentral1);

        //private AmazonS3Client s3Client = new AmazonS3Client(
        //    new BasicAWSCredentials("AKIAJ24BFEFHHGJ6HG4Q", "BVF60+pkTfw2d/jACavFLI5Cxkl0E4iCw8KcKLy5"),
        //    RegionEndpoint.EUCentral1);

        public void Initialize()
        {
            if (AmazonS3Util.DoesS3BucketExistAsync(s3Client, BUCKET).Result)
                return;

            try
            {
                s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = BUCKET,
                    UseClientRegion = true
                }).RunSynchronously();
            }
            catch (AmazonS3Exception e)
            {
                if (e.ErrorCode != null &&
                    (e.ErrorCode.Equals("InvalidAccessKeyId") ||
                     e.ErrorCode.Equals("InvalidSecurity")))
                    throw new DMSException("Check the provided AWS Credentials.");

                throw new DMSException($"Error occurred. Message:'{e.Message}' when writing an object");
            }
        }

        public IEnumerable<Metadata> GetDocumentsMetadata()
        {
            try
            {
                var documentKeys = s3Client.ListObjectsV2Async(
                    new ListObjectsV2Request { BucketName = BUCKET }).Result.S3Objects.Select(e => e.Key);

                var result = new BlockingCollection<Metadata>();
                documentKeys.AsParallel().ForAll(
                    key =>
                    {
                        var request = new GetObjectMetadataRequest()
                        {
                            BucketName = BUCKET,
                            Key = key
                        };
                        var response = s3Client.GetObjectMetadataAsync(request).Result;
                        result.Add(new Metadata(key,
                            response.Metadata.Keys.ToDictionary(e => 
                                FirstCharToUpper(e.Replace("x-amz-meta-", "")), e => response.Metadata[e])));
                    });
                return result;
            }
            catch (AmazonS3Exception e)
            {
                throw new DMSException($"Can't get documents metadata. Error message: '{e.Message}'");
            }
        }

        private static string FirstCharToUpper(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public Document GetDocument(string documentKey)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = BUCKET,
                    Key = documentKey
                };

                using (var response = s3Client.GetObjectAsync(request).Result)
                using (var stream = new MemoryStream())
                {
                    response.ResponseStream.CopyTo(stream);
                    return new Document(documentKey, response.Metadata.Keys
                        .ToDictionary(e => e, e => response.Metadata[e]), stream.ToArray());
                }
            }
            catch (AmazonS3Exception e)
            {
                throw new DMSException($"Can't download document '{documentKey}'. Error message: '{e.Message}'");
            }
        }

        public void SetDocument(Document document)
        {
            try
            {
                using (var stream = new MemoryStream(document.Binary))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = BUCKET,
                        Key = document.Key,
                        InputStream = stream
                    };

                    foreach (var metadata in document.Metadata.Data)
                        request.Metadata.Add(metadata.Key, metadata.Value);

                    s3Client.PutObjectAsync(request).RunSynchronously();
                }
               
            }
            catch (AmazonS3Exception e)
            {
                throw new DMSException($"Can't upload document '{document.Key}'. Error message: '{e.Message}'");
            }
        }

        public void DeleteDocument(string documentKey)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = BUCKET,
                Key = documentKey
            };
            try
            {
                s3Client.DeleteObjectAsync(request).RunSynchronously();
            }
            catch (AmazonS3Exception e)
            {
                throw new DMSException($"Can't delete document '{documentKey}'. Error message: '{e.Message}'");
            }
        }
    }
}
