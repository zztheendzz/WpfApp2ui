using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;

namespace WpfApp2.viewmodel.analysis
{
    public class BrandAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly BrandAnalysisSv _service = new BrandAnalysisSv();

        // Cờ chặn vòng lặp phản hồi khi gán text từ kết quả chọn
        private bool _isInternalChange;

        #region ===================== Properties =====================

        private string _globalSearchText;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                if (_globalSearchText == value) return;
                _globalSearchText = value;
                OnPropertyChanged();

                // Chỉ cập nhật gợi ý nếu KHÔNG phải thay đổi nội bộ do chọn item
                if (!_isInternalChange)
                {
                    if (string.IsNullOrWhiteSpace(value)) SelectedBrandId = 0;
                    UpdateSuggestions();
                }
            }
        }

        private SearchResultDto _selectedSearchResult;
        public SearchResultDto SelectedSearchResult
        {
            get => _selectedSearchResult;
            set
            {
                if (_selectedSearchResult == value) return;
                _selectedSearchResult = value;
                OnPropertyChanged();
            }
        }

        private bool _isSearchDropDownOpen;
        public bool IsSearchDropDownOpen
        {
            get => _isSearchDropDownOpen;
            set
            {
                if (_isSearchDropDownOpen == value) return;
                _isSearchDropDownOpen = value;
                OnPropertyChanged();
            }
        }

        private int _selectedBrandId;
        public int SelectedBrandId
        {
            get => _selectedBrandId;
            set { _selectedBrandId = value; OnPropertyChanged(); }
        }

        private BrandAnalysisDto _analysis;
        public BrandAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        public ICommand AnalyzeCommand { get; set; }
        public ObservableCollection<SearchResultDto> SearchSuggestions { get; } = new ObservableCollection<SearchResultDto>();

        #endregion

        public BrandAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());
        }

        #region ===================== Methods =====================

        private void UpdateSuggestions()
        {
            // Kiểm tra độ dài keyword tương tự Purchase/Vendor
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 2)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                return;
            }

            var results = _searchService.SearchBrand(GlobalSearchText);

            SearchSuggestions.Clear();
            foreach (var item in results)
            {
                SearchSuggestions.Add(item);
            }

            IsSearchDropDownOpen = SearchSuggestions.Any();

            // Mặc định chọn item đầu tiên để người dùng nhấn Enter là lấy luôn
            SelectedSearchResult = SearchSuggestions.FirstOrDefault();
        }

        /// <summary>
        /// Hàm này được gọi từ Code-behind khi nhấn Enter hoặc Click vào ListBoxItem
        /// </summary>
        public void ConfirmSelection()
        {
            if (SelectedSearchResult == null) return;

            _isInternalChange = true;
            try
            {
                SelectedBrandId = SelectedSearchResult.Id;
                GlobalSearchText = SelectedSearchResult.Text;

                IsSearchDropDownOpen = false;
                LoadData(); // Tự động load dữ liệu ngay khi chọn xong
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        private void LoadData()
        {
            if (SelectedBrandId == 0) return;
            Analysis = _service.GetBrandAnalysis(SelectedBrandId);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}