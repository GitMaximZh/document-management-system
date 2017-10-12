using System.ComponentModel;
using System.Runtime.CompilerServices;
using Client.DMS.Desktop.Annotations;
using Client.DMS.Desktop.Model;
using DMS.Core;

namespace Client.DMS.Desktop.ViewModel
{
    public class TestCaseViewModel : INotifyPropertyChanged
    {
        internal const string IsInLocalStorageProperty = "IsInLocalStorage";
        internal const string IsInS3StorageProperty = "IsInS3Storage";
        internal const string IsInMongoDBStorageProperty = "IsInMongoDBStorage";

        private bool isInLocalStorage;
        private bool isInS3Storage;
        private bool isInMongoDBStorage;

        public string Key { get; }
        public string Name { get; }
        public string Date { get; }
        public string Result { get; }

        public bool IsInLocalStorage
        {
            get { return isInLocalStorage; }
            set
            {
                if (value == isInLocalStorage) return;
                isInLocalStorage = value;
                OnPropertyChanged();
            }
        }

        public bool IsInS3Storage
        {
            get { return isInS3Storage; }
            set
            {
                if (value == isInS3Storage) return;
                isInS3Storage = value;
                OnPropertyChanged();
            }
        }

        public bool IsInMongoDBStorage
        {
            get { return isInMongoDBStorage; }
            set
            {
                if (value == isInMongoDBStorage) return;
                isInMongoDBStorage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TestCaseViewModel(Metadata metadata, bool isLocal, bool isS3, bool isMongoDB)
        {
            Key = metadata.DocumentKey;
            Name = metadata.Data[TestCaseMetadataParser.NameKey];
            Date = metadata.Data[TestCaseMetadataParser.DateKey];
            Result = metadata.Data[TestCaseMetadataParser.ResultKey];

            IsInLocalStorage = isLocal;
            IsInS3Storage = isS3;
            IsInMongoDBStorage = isMongoDB;
        }
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
