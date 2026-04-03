using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;
using WpfApp2.Services.exception;

namespace WpfApp2.viewmodel.analysis
{
    public class VendorAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly VendorAnalysisSv _service = new VendorAnalysisSv();
        private bool _isInternalChange;

        #region Properties
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
                var results = _searchService.SearchVendor(GlobalSearchText);
                SearchSuggestions.Clear();
                foreach (var item in results) SearchSuggestions.Add(item);

                IsSearchDropDownOpen = SearchSuggestions.Any();

                // Mặc định highlight dòng đầu để tiện nhấn Enter
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException)
            {
                IsSearchDropDownOpen = false;
            }
        }

        // Chỉnh sửa: Thêm tham số optional để nhận diện Item cụ thể khi Click
        public void ConfirmSelection(SearchResultDto explicitItem = null)
        {
            // Nếu có explicitItem (từ click chuột) thì dùng nó, không thì dùng SelectedSearchResult (từ phím Enter)
            var target = explicitItem ?? SelectedSearchResult;

            if (target == null) return;

            _isInternalChange = true;
            try
            {
                if (target.Data is Vendor v) { SelectedVendorId = v.Id; GlobalSearchText = v.VendorName; }
                else if (target.Data is VendorDto vd) { SelectedVendorId = vd.Id; GlobalSearchText = vd.VendorName; }
                else { SelectedVendorId = target.Id; GlobalSearchText = target.Text; }

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
            if (SelectedVendorId == 0) return;
            try { Analysis = _service.GetVendorAnalysis(SelectedVendorId); }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}