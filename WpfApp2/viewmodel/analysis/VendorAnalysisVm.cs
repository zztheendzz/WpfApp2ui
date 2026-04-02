using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;

namespace WpfApp2.viewmodel.analysis
{
    class VendorAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly VendorAnalysisSv _service = new VendorAnalysisSv();

        // 1. Cờ chặn vòng lặp phản hồi
        private bool _isInternalChange;

        #region ===================== Search Properties =====================

        private string _globalSearchText;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                if (_globalSearchText == value) return;
                _globalSearchText = value;
                OnPropertyChanged();

                // 2. Chỉ update suggestion nếu không phải thay đổi nội bộ (do chọn item)
                if (!_isInternalChange)
                {
                    if (string.IsNullOrWhiteSpace(value)) SelectedVendorId = 0;
                    UpdateSuggestions();
                }
            }
        }

        private bool _isSearchDropDownOpen;
        public bool IsSearchDropDownOpen
        {
            get => _isSearchDropDownOpen;
            set { _isSearchDropDownOpen = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SearchResultDto> SearchSuggestions { get; } = new ObservableCollection<SearchResultDto>();

        private SearchResultDto _selectedSearchResult;
        public SearchResultDto SelectedSearchResult
        {
            get => _selectedSearchResult;
            set { _selectedSearchResult = value; OnPropertyChanged(); }
        }

        public int SelectedVendorId { get; set; }

        #endregion

        #region ===================== Data Properties =====================

        private VendorAnalysisDto _analysis;
        public VendorAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        public ICommand AnalyzeCommand { get; set; }

        #endregion

        public VendorAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());
        }

        #region ===================== Logic Methods =====================

        private void UpdateSuggestions()
        {
            // Kiểm tra điều kiện như bên Purchase (độ dài >= 2)
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 2)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                return;
            }

            var results = _searchService.SearchVendor(GlobalSearchText);

            SearchSuggestions.Clear();
            foreach (var item in results) SearchSuggestions.Add(item);

            IsSearchDropDownOpen = SearchSuggestions.Any();

            // Tự động focus vào dòng đầu tiên để tiện nhấn Enter
            SelectedSearchResult = SearchSuggestions.FirstOrDefault();
        }

        // 3. Hàm xác nhận lựa chọn tương tự Purchase
        public void ConfirmSelection()
        {
            if (SelectedSearchResult == null) return;

            _isInternalChange = true;
            try
            {
                if (SelectedSearchResult.Data is Vendor vendor)
                {
                    SelectedVendorId = vendor.Id;
                    GlobalSearchText = vendor.VendorName;
                }
                else if (SelectedSearchResult.Data is VendorDto vendorDto)
                {
                    SelectedVendorId = vendorDto.Id;
                    GlobalSearchText = vendorDto.VendorName;
                }

                IsSearchDropDownOpen = false;
                LoadData(); // Tự động chạy phân tích sau khi chọn
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        private void LoadData()
        {
            if (SelectedVendorId == 0) return;
            Analysis = _service.GetVendorAnalysis(SelectedVendorId);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}