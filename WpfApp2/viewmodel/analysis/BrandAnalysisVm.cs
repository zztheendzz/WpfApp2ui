using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly BrandAnalysisSv _service;

        // Cờ chặn vòng lặp khi gán text từ kết quả chọn
        private bool _isSelecting;

        public ICommand AnalyzeCommand { get; set; }

        public ObservableCollection<SearchResultDto> SearchSuggestions { get; set; }
            = new ObservableCollection<SearchResultDto>();

        public ObservableCollection<BrandDto> Brands { get; set; }

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

                // Chỉ cập nhật gợi ý nếu KHÔNG phải đang trong quá trình chọn item
                if (!_isSelecting)
                {
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

                _isSelecting = true; // Bắt đầu chặn
                _selectedSearchResult = value;
                OnPropertyChanged();

                if (value != null)
                {
                    SelectedBrandId = value.Id;
                    _globalSearchText = value.Text; // Cập nhật text hiển thị
                    OnPropertyChanged(nameof(GlobalSearchText));

                    IsSearchDropDownOpen = false;
                    LoadData();
                }
                _isSelecting = false; // Mở chặn
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
            set
            {
                _selectedBrandId = value;
                OnPropertyChanged();
            }
        }

        private BrandAnalysisDto _analysis;
        public BrandAnalysisDto Analysis
        {
            get => _analysis;
            set
            {
                _analysis = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public BrandAnalysisVm()
        {
            _service = new BrandAnalysisSv();
            var brandService = new BrandService();

            var list = brandService.GetBrandDTO();
            Brands = new ObservableCollection<BrandDto>(list);

            AnalyzeCommand = new RelayCommand(_ => LoadData());
        }

        #region ===================== Methods =====================

        private void UpdateSuggestions()
        {
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

            IsSearchDropDownOpen = SearchSuggestions.Count > 0;
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