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
    public class EquipmentAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly EquipmentAnalysisSv _service = new EquipmentAnalysisSv();
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
                    // Nếu xóa sạch text thì reset ID
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        SelectedEquipmentId = 0;
                        SearchSuggestions.Clear();
                        IsSearchDropDownOpen = false;
                    }
                    else
                    {
                        UpdateSuggestions();
                    }
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

        private int _selectedEquipmentId;
        public int SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set { _selectedEquipmentId = value; OnPropertyChanged(); }
        }

        private EquipmentAnalysisDto _analysis;
        public EquipmentAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        public ICommand AnalyzeCommand { get; set; }
        public ObservableCollection<SearchResultDto> SearchSuggestions { get; } = new ObservableCollection<SearchResultDto>();

        #endregion

        public EquipmentAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());
        }

        #region ===================== Methods =====================

        private void UpdateSuggestions()
        {
            // Tăng trải nghiệm bằng cách chỉ search khi đủ 2 ký tự
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 2)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                return;
            }

            var results = _searchService.SearchEquipment(GlobalSearchText);

            SearchSuggestions.Clear();
            if (results != null)
            {
                foreach (var item in results)
                {
                    SearchSuggestions.Add(item);
                }
            }

            IsSearchDropDownOpen = SearchSuggestions.Any();

            // Tự động focus vào dòng đầu tiên để nhấn Enter là lấy luôn
            SelectedSearchResult = SearchSuggestions.FirstOrDefault();
        }

        public void ConfirmSelection()
        {
            if (SelectedSearchResult == null) return;

            _isInternalChange = true;
            try
            {
                // Lấy ID thông minh dựa trên kiểu Object chứa trong Data
                SelectedEquipmentId = SelectedSearchResult.Data switch
                {
                    Equipment e => e.Id,
                    EquipmentDto d => d.Id,
                    _ => SelectedSearchResult.Id
                };

                GlobalSearchText = SelectedSearchResult.Text;
                IsSearchDropDownOpen = false;

                // Load dữ liệu phân tích ngay lập tức
                LoadData();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần (ví dụ: Log lỗi)
                System.Diagnostics.Debug.WriteLine($"Error selecting equipment: {ex.Message}");
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        private void LoadData()
        {
            if (SelectedEquipmentId <= 0) return;

            // Thực hiện gọi service lấy dữ liệu phân tích
            var result = _service.GetEquipmentAnalysis(SelectedEquipmentId);
            if (result != null)
            {
                Analysis = result;
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