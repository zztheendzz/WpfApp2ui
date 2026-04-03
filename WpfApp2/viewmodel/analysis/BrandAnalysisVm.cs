using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;
using WpfApp2.Services.exception;

namespace WpfApp2.viewmodel.analysis
{
    public class BrandAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly BrandAnalysisSv _service = new BrandAnalysisSv();
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
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 2)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                return;
            }

            try
            {
                var results = _searchService.SearchBrand(GlobalSearchText);
                SearchSuggestions.Clear();
                foreach (var item in results) SearchSuggestions.Add(item);

                IsSearchDropDownOpen = SearchSuggestions.Any();

                // Tự động highlight dòng đầu tiên cho Enter
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException)
            {
                IsSearchDropDownOpen = false;
            }
        }

        // CẬP NHẬT: Thêm tham số explicitItem để nhận diện item khi click chuột
        public void ConfirmSelection(SearchResultDto explicitItem = null)
        {
            var target = explicitItem ?? SelectedSearchResult;
            if (target == null) return;

            _isInternalChange = true;
            try
            {
                SelectedBrandId = target.Id;
                GlobalSearchText = target.Text;

                IsSearchDropDownOpen = false;
                LoadData();
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        private void LoadData()
        {
            if (SelectedBrandId == 0) return;
            try
            {
                Analysis = _service.GetBrandAnalysis(SelectedBrandId);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Hệ thống đang bận tính toán. Vui lòng thử lại sau.", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}