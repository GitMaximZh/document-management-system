using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MaterialDesignThemes.Wpf;
using Client.DMS.Desktop.Annotations;
using Client.DMS.Desktop.Model;
using DMS.Core;
using DMS.Core.Persistence;

namespace Client.DMS.Desktop.ViewModel
{
    public class TestCasesViewModel : INotifyPropertyChanged
    {
        private readonly IDocumentRepository localRepository;
        private readonly IDocumentRepository s3Repository;
        private readonly IDocumentRepository mongodbRepository;

        private bool skipTestCasePropertyChanged = false;

        private bool useLocalStorage = true;
        private bool useS3Storage = true;
        private bool useMongoDbStorage = true;

        public bool UseLocalStorage
        {
            get { return useLocalStorage; }
            set
            {
                if (value == useLocalStorage) return;
                useLocalStorage = value;
                OnPropertyChanged();
                Refresh();
            }
        }

        public bool UseS3Storage
        {
            get { return useS3Storage; }
            set
            {
                if (value == useS3Storage) return;
                useS3Storage = value;
                OnPropertyChanged();
                Refresh();
            }
        }

        public bool UseMongoDBStorage
        {
            get { return useMongoDbStorage; }
            set
            {
                if (value == useMongoDbStorage) return;
                useMongoDbStorage = value;
                OnPropertyChanged();
                Refresh();
            }
        }

        public ObservableCollection<TestCaseViewModel> TestCases { get; }

        public DelegateCommand<object> RefreshCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public TestCasesViewModel()
        {
            localRepository = new LocalDocumentRepository("Storage", new TestCaseMetadataParser());
            s3Repository = new S3DocumentRepository();
            mongodbRepository = new WebAPIDocumentRepository();

            TestCases = new ObservableCollection<TestCaseViewModel>();
            RefreshCommand = new DelegateCommand<object>(e =>
            {
                Refresh();
            });
        }

        private void Refresh()
        {
            this.TestCases.Clear();
            this.Initialize();
        }

        public void Initialize()
        {
            localRepository.Initialize();
            s3Repository.Initialize();
            mongodbRepository.Initialize();

            var localMetadata = UseLocalStorage ? localRepository.GetDocumentsMetadata().ToDictionary(e => e.DocumentKey, e => e)
                : new Dictionary<string, Metadata>();
            var s3Metadata = UseS3Storage ? s3Repository.GetDocumentsMetadata().ToDictionary(e => e.DocumentKey, e => e)
                : new Dictionary<string, Metadata>();
            var mongoDB = UseMongoDBStorage ? mongodbRepository.GetDocumentsMetadata().ToDictionary(e => e.DocumentKey, e => e)
                : new Dictionary<string, Metadata>();

            var createTestCaseViewModel = new Func<Metadata, bool, bool, bool, TestCaseViewModel>((m, e1, e2, e3) =>
            {
                var vm = new TestCaseViewModel(m, e1, e2, e3);
                vm.PropertyChanged += OnTestCasePropertyChanged;
                return vm;
            });

            foreach (var key in localMetadata.Keys)
                TestCases.Add(createTestCaseViewModel(localMetadata[key], true, s3Metadata.ContainsKey(key), mongoDB.ContainsKey(key)));
            foreach (var key in s3Metadata.Keys.Except(localMetadata.Keys))
                TestCases.Add(createTestCaseViewModel(s3Metadata[key], false, true, mongoDB.ContainsKey(key)));
            foreach (var key in mongoDB.Keys.Except(localMetadata.Keys).Except(s3Metadata.Keys))
                TestCases.Add(createTestCaseViewModel(mongoDB[key], false, false, true));
        }

        private void OnTestCasePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == TestCaseViewModel.IsInLocalStorageProperty)
                OnIsInLocalStorageChanged((TestCaseViewModel)sender);
            else if (propertyChangedEventArgs.PropertyName == TestCaseViewModel.IsInS3StorageProperty)
                OnIsInS3StorageProperty((TestCaseViewModel)sender);
            else if (propertyChangedEventArgs.PropertyName == TestCaseViewModel.IsInMongoDBStorageProperty)
                OnIsInMongoDBStorageProperty((TestCaseViewModel)sender);
        }

        private async void OnIsInMongoDBStorageProperty(TestCaseViewModel sender)
        {
            if (skipTestCasePropertyChanged)
            {
                skipTestCasePropertyChanged = false;
                return;
            }

            if (!sender.IsInS3Storage &&
                !sender.IsInLocalStorage &&
                !sender.IsInMongoDBStorage &&
                !(bool)await DialogHost.Show("The file will be removed. Whould you like to continue?"))
            {
                skipTestCasePropertyChanged = true;
                sender.IsInMongoDBStorage = true;
                return;
            }

            if (sender.IsInMongoDBStorage)
                mongodbRepository.SetDocument(localRepository.GetDocument(sender.Key));
            else
            {
                mongodbRepository.DeleteDocument(sender.Key);
                if (!sender.IsInLocalStorage && !sender.IsInS3Storage)
                    TestCases.Remove(sender);
            }
        }

        private async void OnIsInS3StorageProperty(TestCaseViewModel sender)
        {
            if (skipTestCasePropertyChanged)
            {
                skipTestCasePropertyChanged = false;
                return;
            }

            if (!sender.IsInS3Storage && 
                !sender.IsInLocalStorage && 
                !sender.IsInMongoDBStorage &&
                !(bool)await DialogHost.Show("The file will be removed. Whould you like to continue?"))
            {
                skipTestCasePropertyChanged = true;
                sender.IsInS3Storage = true;
                return;
            }

            if (sender.IsInS3Storage)
                s3Repository.SetDocument(localRepository.GetDocument(sender.Key));
            else
            {
                s3Repository.DeleteDocument(sender.Key);
                if (!sender.IsInLocalStorage && !sender.IsInMongoDBStorage)
                    TestCases.Remove(sender);
            }
        }

        private async void OnIsInLocalStorageChanged(TestCaseViewModel sender)
        {
            if (skipTestCasePropertyChanged)
            {
                skipTestCasePropertyChanged = false;
                return;
            }

            if (!sender.IsInS3Storage &&
                !sender.IsInLocalStorage &&
                !sender.IsInMongoDBStorage &&
                !(bool)await DialogHost.Show("The file will be removed. Whould you like to continue?"))
            {
                skipTestCasePropertyChanged = true;
                sender.IsInLocalStorage = true;
                return;
            }

            if (sender.IsInLocalStorage)
                localRepository.SetDocument(s3Repository.GetDocument(sender.Key));
            else
            {
                localRepository.DeleteDocument(sender.Key);
                if (!sender.IsInS3Storage && !sender.IsInMongoDBStorage)
                    TestCases.Remove(sender);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
