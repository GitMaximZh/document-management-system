using DMS.Core.Persistence;

namespace Client.DMS.Desktop.Model
{
    class TestCasesService
    {
        private readonly IDocumentRepository localRepository;
        private readonly IDocumentRepository s3Repository;

        public TestCasesService()
        {
            localRepository = new LocalDocumentRepository("Storage", new TestCaseMetadataParser());
            localRepository.Initialize();

            s3Repository = new S3DocumentRepository();
            s3Repository.Initialize();
        }
    }
}
