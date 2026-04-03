using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows; // Thêm để dùng MessageBox
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

        // Cờ chặn vòng lặp phản hồi
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

                // Chỉ update suggestion nếu không phải thay đổi nội bộ (do chọn item)
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

                // Tự động focus vào dòng đầu tiên để tiện nhấn Enter
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException)
            {
                // Silent fail cho phần gợi ý khi đang gõ
                IsSearchDropDownOpen = false;
            }
        }

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
                else
                {
                    // Fallback
                    SelectedVendorId = SelectedSearchResult.Id;
                    GlobalSearchText = SelectedSearchResult.Text;
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

            try
            {
                Analysis = _service.GetVendorAnalysis(SelectedVendorId);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Cơ sở dữ liệu đang bận tính toán dữ liệu Nhà cung cấp. Vui lòng thử lại sau.", "SQLite Locked");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}