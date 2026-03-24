using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;

namespace WpfApp2.viewmodel.analysis
{
    public class PurchaseAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();

        // Các cờ chặn vòng lặp phản hồi cho từng trường
        private bool _isSelectingM;
        private bool _isSelectingE;
        private bool _isSelectingV;

        private bool _isDropDownOpenV;
        public bool IsDropDownOpenV
        {
            get => _isDropDownOpenV;
            set { _isDropDownOpenV = value; OnPropertyChanged(); }
        }

        private bool _isDropDownOpenM;

        public bool IsDropDownOpenM
        {
            get => _isDropDownOpenM;
            set { _isDropDownOpenM = value; OnPropertyChanged(); }
        }
        private bool _isDropDownOpenE;

        public bool IsDropDownOpenE
        {
            get => _isDropDownOpenE;
            set { _isDropDownOpenE = value; OnPropertyChanged(); }
        }

        public ICommand SearchCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand VendorFocusCommand { get; set; }

        public ObservableCollection<PurchaseDto> PurchaseDtos { get; set; } = new ObservableCollection<PurchaseDto>();

        #region ===================== Suggestions =====================

        public ObservableCollection<SearchResultDto> ModelSuggestions { get; set; } = new ObservableCollection<SearchResultDto>();
        public ObservableCollection<SearchResultDto> VendorSuggestions { get; set; } = new ObservableCollection<SearchResultDto>();
        public ObservableCollection<SearchResultDto> EquipmentSuggestions { get; set; } = new ObservableCollection<SearchResultDto>();

        #endregion

        #region ===================== Search Text =====================

        private string _searchModelText;
        public string SearchModelText
        {
            get => _searchModelText;
            set
            {
                if (_searchModelText == value) return;
                _searchModelText = value;
                OnPropertyChanged();
                if (!_isSelectingM) UpdateModelSuggestions(value);
            }
        }

        private string _searchVendorText;
        public string SearchVendorText
        {
            get => _searchVendorText;
            set
            {
                if (_searchVendorText == value) return;
                _searchVendorText = value;
                OnPropertyChanged();
                if (!_isSelectingV) UpdateVendorSuggestions(value);
            }
        }

        private string _searchEquipmentText;
        public string SearchEquipmentText
        {
            get => _searchEquipmentText;
            set
            {
                if (_searchEquipmentText == value) return;
                _searchEquipmentText = value;
                OnPropertyChanged();
                if (!_isSelectingE) UpdateEquipmentSuggestions(value);
            }
        }

        #endregion

        #region ===================== Selected Item =====================

        private SearchResultDto _selectedModel;
        public SearchResultDto SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel == value) return;
                _isSelectingM = true;
                _selectedModel = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _selectedModelId = value.Id;
                    OnPropertyChanged(nameof(SelectedModelId));
                    _searchModelText = value.Text;
                    OnPropertyChanged(nameof(SearchModelText));
                }
                _isSelectingM = false;
            }
        }

        private SearchResultDto _selectedVendor;
        public SearchResultDto SelectedVendor
        {
            get => _selectedVendor;
            set
            {
                if (_selectedVendor == value) return;
                _isSelectingV = true;
                _selectedVendor = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _selectedVendorId = value.Id;
                    OnPropertyChanged(nameof(SelectedVendorId));
                    _searchVendorText = value.Text;
                    OnPropertyChanged(nameof(SearchVendorText));
                }
                _isSelectingV = false;
            }
        }

        private SearchResultDto _selectedEquipment;
        public SearchResultDto SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                if (_selectedEquipment == value) return;
                _isSelectingE = true;
                _selectedEquipment = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _selectedEquipmentId = value.Id;
                    OnPropertyChanged(nameof(SelectedEquipmentId));
                    _searchEquipmentText = value.Text;
                    OnPropertyChanged(nameof(SearchEquipmentText));
                }
                _isSelectingE = false;
            }
        }

        #endregion

        #region ===================== Filter Properties =====================

        private int _selectedModelId;
        public int SelectedModelId { get => _selectedModelId; set { _selectedModelId = value; OnPropertyChanged(); } }

        private int _selectedVendorId;
        public int SelectedVendorId { get => _selectedVendorId; set { _selectedVendorId = value; OnPropertyChanged(); } }

        private int _selectedEquipmentId;
        public int SelectedEquipmentId { get => _selectedEquipmentId; set { _selectedEquipmentId = value; OnPropertyChanged(); LoadData(); } }

        private DateTime? _dateFrom;
        public DateTime? SelectedDateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); ValidateDate(); } }

        private DateTime? _dateTo;
        public DateTime? SelectedDateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); ValidateDate(); } }

        private decimal? _priceMin;
        public decimal? PriceMin { get => _priceMin; set { if (value < 0) value = 0; _priceMin = value; OnPropertyChanged(); ValidateRange(); } }

        private decimal? _priceMax;
        public decimal? PriceMax { get => _priceMax; set { if (value < 0) value = 0; _priceMax = value; OnPropertyChanged(); ValidateRange(); } }

        private string _error;
        public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

        private int? _count;
        public int? SelectedCount { get => _count; set { _count = value; OnPropertyChanged(); } }

        #endregion

        public PurchaseAnalysisVm()
        {
            SearchCommand = new RelayCommand(Search);
            ClearCommand = new RelayCommand(Clear);
        }

        #region ===================== Logic Methods =====================

        public void Clear(object obj)
        {
            // Bật tất cả cờ chặn để tránh gọi API search khi đang xóa
            _isSelectingM = _isSelectingV = _isSelectingE = true;

            try
            {
                // 1. Clear Suggestions lists
                ModelSuggestions.Clear();
                VendorSuggestions.Clear();
                EquipmentSuggestions.Clear();

                // 2. Clear Selected Objects
                _selectedModel = null;
                _selectedVendor = null;
                _selectedEquipment = null;
                OnPropertyChanged(nameof(SelectedModel));
                OnPropertyChanged(nameof(SelectedVendor));
                OnPropertyChanged(nameof(SelectedEquipment));

                // 3. Clear Search Texts (Ép UI về rỗng)
                _searchModelText = string.Empty;
                _searchVendorText = string.Empty;
                _searchEquipmentText = string.Empty;
                OnPropertyChanged(nameof(SearchModelText));
                OnPropertyChanged(nameof(SearchVendorText));
                OnPropertyChanged(nameof(SearchEquipmentText));

                // 4. Reset IDs và các Filter khác
                _selectedModelId = _selectedVendorId = _selectedEquipmentId = 0;
                OnPropertyChanged(nameof(SelectedModelId));
                OnPropertyChanged(nameof(SelectedVendorId));
                OnPropertyChanged(nameof(SelectedEquipmentId));

                _priceMin = _priceMax = null;
                _dateFrom = _dateTo = null;
                _error = null;
                OnPropertyChanged(nameof(PriceMin));
                OnPropertyChanged(nameof(PriceMax));
                OnPropertyChanged(nameof(SelectedDateFrom));
                OnPropertyChanged(nameof(SelectedDateTo));
                OnPropertyChanged(nameof(Error));

                // 5. Load lại dữ liệu mặc định
                PurchaseDtos.Clear();
            }
            finally
            {
                _isSelectingM = _isSelectingV = _isSelectingE = false;
            }
        }

        private void LoadData()
        {
            PurchaseDtos.Clear();
            PurchaseAnalysisSv purchaseService = new PurchaseAnalysisSv();
            var list = purchaseService.Search3(
                SelectedEquipmentId == 0 ? null : (int?)SelectedEquipmentId,
                SelectedModelId == 0 ? null : (int?)SelectedModelId,
                SelectedVendorId == 0 ? null : (int?)SelectedVendorId,
                PriceMin, PriceMax, SelectedDateFrom, SelectedDateTo
            ).ToList();

            foreach (var item in list) PurchaseDtos.Add(item);
            SelectedCount = list.Count;
        }

        public void Search(object obj) => LoadData();

        private void UpdateModelSuggestions(string t) 
        { 
            if (_isSelectingM || string.IsNullOrWhiteSpace(t) || t.Length < 2) { ModelSuggestions.Clear(); return; }

            var res = _searchService.SearchModel(t); ModelSuggestions.Clear(); foreach (var i in res) ModelSuggestions.Add(i);
        }
        private void UpdateVendorSuggestions(string t) { 
            if (_isSelectingV || string.IsNullOrWhiteSpace(t) || t.Length < 2) { VendorSuggestions.Clear(); return; }

            var res = _searchService.SearchVendor(t); VendorSuggestions.Clear(); foreach (var i in res) VendorSuggestions.Add(i); }
        private void UpdateEquipmentSuggestions(string t) { 
            if (_isSelectingE || string.IsNullOrWhiteSpace(t) || t.Length < 2) { EquipmentSuggestions.Clear(); return; }

            var res = _searchService.SearchEquipment(t); EquipmentSuggestions.Clear(); foreach (var i in res) EquipmentSuggestions.Add(i); }

        private void ValidateDate() { if (SelectedDateFrom > SelectedDateTo) Error = "From phải ≤ To"; else Error = null; }
        private void ValidateRange() { if (PriceMin > PriceMax) Error = "Min phải ≤ Max"; else Error = null; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}