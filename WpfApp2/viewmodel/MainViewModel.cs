using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.view.analysis;
using WpfApp2.view.pages;

namespace WpfApp2.viewmodel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public ICommand ShowModelPageCommand { get; set; }
        public ICommand ShowBrandlPageCommand { get; set; }
        public ICommand ShowUserPageCommand { get; set; }
        public ICommand ShowEquipmentPageCommand { get; set; }
        public ICommand ShowCategoryPageCommand { get; set; }
        public ICommand ShowPuchaseHistoryPageCommand { get; set; }
        public ICommand ShowVendorPageCommand { get; set; }

        public ICommand ShowModelAnalysisCommand { get; set; }
        public ICommand ShowVendorAnalysisCommand { get; set; }
        public ICommand ShowBrandAnalysisCommand { get; set; }

        public ICommand ShowEquipmentAnalysisCommand { get; set; }
        public ICommand ShowImportExcelCommand { get; set; }
        

        private object _currentPage;
        public object CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(CurrentPage)));
            }
        }

        public MainViewModel()
        {
            ShowModelPageCommand = new RelayCommand(OpenModelPage);
            ShowBrandlPageCommand = new RelayCommand(OpenBrandPage);
            ShowUserPageCommand = new RelayCommand(OpenUserPage);
            ShowEquipmentPageCommand = new RelayCommand(OpenEquipmentPage);
            ShowCategoryPageCommand = new RelayCommand(OpenPuchaseHistoryPage);
            ShowPuchaseHistoryPageCommand = new RelayCommand(OpenVendorPage);
            ShowVendorPageCommand = new RelayCommand(OpenCurrencyPage);
            ShowModelAnalysisCommand = new RelayCommand(OpenModelAnalysis);
            ShowVendorAnalysisCommand = new RelayCommand(OpenVendorAnalysis);
            ShowBrandAnalysisCommand = new RelayCommand(OpenBrandAnalysis);
            ShowEquipmentAnalysisCommand = new RelayCommand(OpenEquipmentAnalysis);
            ShowImportExcelCommand = new RelayCommand(OpenPageImportExcel);

            //CurrentPage = new newModel(); // page mặc định
        }

        void OpenPageImportExcel(object obj)
        {
            CurrentPage = new pageImportExcel();
        }
        void OpenEquipmentAnalysis(object obj)
        {
            CurrentPage = new EquipmentAnalysis();
        }

        void OpenBrandAnalysis(object obj)
        {
            CurrentPage = new BrandAnalysis();
        }

        void OpenVendorAnalysis(object obj)
        {
            CurrentPage = new VendorAnalysis();
        }

        void OpenModelAnalysis(object obj)
        {
            CurrentPage = new ModelAnalysis();
        }
        void OpenModelPage(object obj)
        {
            CurrentPage = new newModel();
        }

        void OpenBrandPage(object obj)
        {
            CurrentPage = new pageBrand();
        }
        void OpenCategoryPage(object obj)
        {
           // CurrentPage = new pageCurrency();
        }
        void OpenUserPage(object obj)
        {
           // CurrentPage = new pageUser();
        }
        void OpenEquipmentPage(object obj)
        {
            CurrentPage = new pageEquipment();
        }
        void OpenPuchaseHistoryPage(object obj)
        {
            CurrentPage = new pageCategory();
        }
        void OpenVendorPage(object obj)
        {
            CurrentPage = new pagePuchaseHistory();
        }
        void OpenCurrencyPage(object obj)
        {
            CurrentPage = new pageVendor();
        }

        private SearchService _searchService = new SearchService();

        public ObservableCollection<SearchResult> SearchSuggestions { get; set; }
            = new ObservableCollection<SearchResult>();

        private string _globalSearchText;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                _globalSearchText = value;
                OnPropertyChanged();
                UpdateSuggestions();
            }
        }

        private void UpdateSuggestions()
        {
            SearchSuggestions.Clear();

            if (string.IsNullOrWhiteSpace(GlobalSearchText))
                return;
            if (GlobalSearchText.Length < 2)
                return;

            var results = _searchService.GlobalSearch(GlobalSearchText);

            foreach (var item in results)
                SearchSuggestions.Add(item);
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}