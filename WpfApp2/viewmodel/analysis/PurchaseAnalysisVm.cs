using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly PurchaseAnalysisSv _purchaseService = new PurchaseAnalysisSv();

        // Cờ chặn vòng lặp phản hồi cho từng trường
        private bool _isInternalChange;

        #region ===================== Dropdown States =====================

        private bool _isDropDownOpenM;
        public bool IsDropDownOpenM { get => _isDropDownOpenM; set { _isDropDownOpenM = value; OnPropertyChanged(); } }

        private bool _isDropDownOpenV;
        public bool IsDropDownOpenV { get => _isDropDownOpenV; set { _isDropDownOpenV = value; OnPropertyChanged(); } }

        private bool _isDropDownOpenE;
        public bool IsDropDownOpenE { get => _isDropDownOpenE; set { _isDropDownOpenE = value; OnPropertyChanged(); } }

        #endregion

        public ICommand SearchCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public ObservableCollection<PurchaseDto> PurchaseDtos { get; } = new ObservableCollection<PurchaseDto>();

        #region ===================== Suggestions & Selected Items =====================

        public ObservableCollection<SearchResultDto> ModelSuggestions { get; } = new ObservableCollection<SearchResultDto>();
        public ObservableCollection<SearchResultDto> VendorSuggestions { get; } = new ObservableCollection<SearchResultDto>();
        public ObservableCollection<SearchResultDto> EquipmentSuggestions { get; } = new ObservableCollection<SearchResultDto>();

        // Selected Objects (Dùng để Binding với ListBox SelectedItem)
        private SearchResultDto _selectedModel;
        public SearchResultDto SelectedModel { get => _selectedModel; set { _selectedModel = value; OnPropertyChanged(); } }

        private SearchResultDto _selectedVendor;
        public SearchResultDto SelectedVendor { get => _selectedVendor; set { _selectedVendor = value; OnPropertyChanged(); } }

        private SearchResultDto _selectedEquipment;
        public SearchResultDto SelectedEquipment { get => _selectedEquipment; set { _selectedEquipment = value; OnPropertyChanged(); } }

        #endregion

        #region ===================== Search Text Properties =====================

        private string _searchModelText;
        public string SearchModelText
        {
            get => _searchModelText;
            set
            {
                if (_searchModelText == value) return;
                _searchModelText = value;
                OnPropertyChanged();
                if (!_isInternalChange)
                {
                    if (string.IsNullOrWhiteSpace(value)) SelectedModelId = 0;
                    UpdateSuggestions("M", value);
                }
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
                if (!_isInternalChange)
                {
                    if (string.IsNullOrWhiteSpace(value)) SelectedVendorId = 0;
                    UpdateSuggestions("V", value);
                }
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
                if (!_isInternalChange)
                {
                    if (string.IsNullOrWhiteSpace(value)) SelectedEquipmentId = 0;
                    UpdateSuggestions("E", value);
                }
            }
        }

        #endregion

        #region ===================== Filter Data (IDs, Dates, Prices) =====================

        public int SelectedModelId { get; set; }
        public int SelectedVendorId { get; set; }
        public int SelectedEquipmentId { get; set; }

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
            SearchCommand = new RelayCommand(_ => LoadData());
            ClearCommand = new RelayCommand(_ => ClearAll());
        }

        #region ===================== Logic Methods =====================

        private void UpdateSuggestions(string type, string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                if (type == "M") { ModelSuggestions.Clear(); IsDropDownOpenM = false; }
                if (type == "V") { VendorSuggestions.Clear(); IsDropDownOpenV = false; }
                if (type == "E") { EquipmentSuggestions.Clear(); IsDropDownOpenE = false; }
                return;
            }

            switch (type)
            {
                case "M":
                    var resM = _searchService.SearchModel(query);
                    ModelSuggestions.Clear(); foreach (var i in resM) ModelSuggestions.Add(i);
                    IsDropDownOpenM = ModelSuggestions.Any();
                    SelectedModel = ModelSuggestions.FirstOrDefault();
                    break;
                case "V":
                    var resV = _searchService.SearchVendor(query);
                    VendorSuggestions.Clear(); foreach (var i in resV) VendorSuggestions.Add(i);
                    IsDropDownOpenV = VendorSuggestions.Any();
                    SelectedVendor = VendorSuggestions.FirstOrDefault();
                    break;
                case "E":
                    var resE = _searchService.SearchEquipment(query);
                    EquipmentSuggestions.Clear(); foreach (var i in resE) EquipmentSuggestions.Add(i);
                    IsDropDownOpenE = EquipmentSuggestions.Any();
                    SelectedEquipment = EquipmentSuggestions.FirstOrDefault();
                    break;
            }
        }

        // Hàm này gọi khi người dùng nhấn Enter hoặc Click chọn item từ Popup
        public void ConfirmSelection(string type)
        {
            _isInternalChange = true;
            try
            {
                if (type == "M" && SelectedModel != null)
                {
                    SearchModelText = SelectedModel.Text;
                    SelectedModelId = SelectedModel.Id;
                    IsDropDownOpenM = false;
                }
                else if (type == "V" && SelectedVendor != null)
                {
                    SearchVendorText = SelectedVendor.Text;
                    SelectedVendorId = SelectedVendor.Id;
                    IsDropDownOpenV = false;
                }
                else if (type == "E" && SelectedEquipment != null)
                {
                    SearchEquipmentText = SelectedEquipment.Text;
                    SelectedEquipmentId = SelectedEquipment.Id;
                    IsDropDownOpenE = false;
                    LoadData(); // Tự động load nếu là thiết bị
                }
            }
            finally { _isInternalChange = false; }
        }

        private void LoadData()
        {
            PurchaseDtos.Clear();
            var list = _purchaseService.Search3(
                SelectedEquipmentId == 0 ? null : (int?)SelectedEquipmentId,
                SelectedModelId == 0 ? null : (int?)SelectedModelId,
                SelectedVendorId == 0 ? null : (int?)SelectedVendorId,
                PriceMin, PriceMax, SelectedDateFrom, SelectedDateTo
            ).ToList();

            foreach (var item in list) PurchaseDtos.Add(item);
            SelectedCount = list.Count;
        }

        private void ClearAll()
        {
            _isInternalChange = true;
            try
            {
                ModelSuggestions.Clear(); VendorSuggestions.Clear(); EquipmentSuggestions.Clear();
                SearchModelText = SearchVendorText = SearchEquipmentText = string.Empty;
                SelectedModelId = SelectedVendorId = SelectedEquipmentId = 0;
                SelectedModel = null; SelectedVendor = null; SelectedEquipment = null;
                PriceMin = PriceMax = null; SelectedDateFrom = SelectedDateTo = null;
                Error = null; PurchaseDtos.Clear(); SelectedCount = 0;
                IsDropDownOpenM = IsDropDownOpenV = IsDropDownOpenE = false;
            }
            finally { _isInternalChange = false; }
        }

        private void ValidateDate() { Error = (SelectedDateFrom > SelectedDateTo) ? "Từ ngày ≤ Đến ngày" : null; }
        private void ValidateRange() { Error = (PriceMin > PriceMax) ? "Min ≤ Max" : null; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}