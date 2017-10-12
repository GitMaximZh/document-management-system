using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Client.DMS.Desktop.Model;
using Client.DMS.Desktop.ViewModel;
using DMS.Core.Persistence;

namespace Client.DMS.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var repository = new LocalDocumentRepository("Storage", new TestCaseMetadataParser());
            repository.Initialize();

            var vm = new TestCasesViewModel();
            vm.Initialize();

            DataContext = vm;
        }
    }
}
