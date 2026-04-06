using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using WpfApp2.command;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;

namespace WpfApp2.viewmodel.analysis
{
    public class PurchaseAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly PurchaseAnalysisSv _purchaseService = new PurchaseAnalysisSv();
        private bool _isInternalChange;

        #region ===================== Properties Dữ Liệu & Charts =====================

        private ModelAnalysisDto _analysis;
        public ModelAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        // --- LiveCharts Properties ---
        private SeriesCollection _vendorPriceSeries;
        public SeriesCollection VendorPriceSeries { get => _vendorPriceSeries; set { _vendorPriceSeries = value; OnPropertyChanged(); } }

        private string[] _vendorLabels;
        public string[] VendorLabels { get => _vendorLabels; set { _vendorLabels = value; OnPropertyChanged(); } }

        private SeriesCollection _priceTrendSeries;
        public SeriesCollection PriceTrendSeries { get => _priceTrendSeries; set { _priceTrendSeries = value; OnPropertyChanged(); } }

        private string[] _trendLabels;
        public string[] TrendLabels { get => _trendLabels; set { _trendLabels = value; OnPropertyChanged(); } }

        #endregion

        #region ===================== Filter & Search Properties =====================

        // Suggestions
        public ObservableCollection<SearchResultDto> ModelSuggestions { get; } = new ObservableCollection<SearchResultDto>();
        public ObservableCollection<SearchResultDto> VendorSuggestions { get; } = new ObservableCollection<SearchResultDto>();
        public ObservableCollection<SearchResultDto> EquipmentSuggestions { get; } = new ObservableCollection<SearchResultDto>();

        // Selected Items for UI Highlight
        private SearchResultDto _selectedModel;
        public SearchResultDto SelectedModel { get => _selectedModel; set { _selectedModel = value; OnPropertyChanged(); } }

        private SearchResultDto _selectedVendor;
        public SearchResultDto SelectedVendor { get => _selectedVendor; set { _selectedVendor = value; OnPropertyChanged(); } }

        private SearchResultDto _selectedEquipment;
        public SearchResultDto SelectedEquipment { get => _selectedEquipment; set { _selectedEquipment = value; OnPropertyChanged(); } }

        // Search Texts
        private string _searchModelText;
        public string SearchModelText
        {
            get => _searchModelText;
            set
            {
                if (_searchModelText == value) return;
                _searchModelText = value;
                OnPropertyChanged();
                if (!_isInternalChange) UpdateSuggestions("M", value);
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
                if (!_isInternalChange) UpdateSuggestions("V", value);
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
                if (!_isInternalChange) UpdateSuggestions("E", value);
            }
        }

        // Popup States
        private bool _isDropDownOpenM;
        public bool IsDropDownOpenM { get => _isDropDownOpenM; set { _isDropDownOpenM = value; OnPropertyChanged(); } }

        private bool _isDropDownOpenV;
        public bool IsDropDownOpenV { get => _isDropDownOpenV; set { _isDropDownOpenV = value; OnPropertyChanged(); } }

        private bool _isDropDownOpenE;
        public bool IsDropDownOpenE { get => _isDropDownOpenE; set { _isDropDownOpenE = value; OnPropertyChanged(); } }

        // Filters
        public int SelectedModelId { get; set; }
        public int SelectedVendorId { get; set; }
        public int SelectedEquipmentId { get; set; }
        public DateTime? SelectedDateFrom { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        public DateTime? SelectedDateTo { get; set; } = DateTime.Now;
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }

        public ICommand SearchCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        #endregion

        public PurchaseAnalysisVm()
        {
            SearchCommand = new RelayCommand(_ => LoadData());
            ClearCommand = new RelayCommand(_ => ClearAll());
        }

        #region ===================== Logic Methods =====================

        private void LoadData()
        {
            try
            {
                var result = _purchaseService.GetComprehensiveAnalysis(
                    SearchModelText,
                    SelectedModelId == 0 ? null : (int?)SelectedModelId,
                    SelectedVendorId == 0 ? null : (int?)SelectedVendorId,
                    SelectedEquipmentId == 0 ? null : (int?)SelectedEquipmentId,
                    SelectedDateFrom,
                    SelectedDateTo,
                    PriceMin,
                    PriceMax
                );

                Analysis = result;
                UpdateCharts(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        private void UpdateCharts(ModelAnalysisDto data)
        {
            if (data == null || !data.Items.Any())
            {
                VendorPriceSeries = new SeriesCollection();
                PriceTrendSeries = new SeriesCollection();
                return;
            }

            // 1. Biểu đồ cột: So sánh giá Vendor
            VendorLabels = data.VendorComparison.Select(x => x.CategoryName).ToArray();
            VendorPriceSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Giá trung bình",
                    Values = new ChartValues<decimal>(data.VendorComparison.Select(x => x.TotalAmount)),
                    DataLabels = true,
                    LabelPoint = p => p.Y.ToString("N0")
                }
            };

            // 2. Biểu đồ đường: Xu hướng giá
            TrendLabels = data.PriceTrend.Select(x => x.MonthYear).ToArray();
            PriceTrendSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Đơn giá",
                    Values = new ChartValues<decimal>(data.PriceTrend.Select(x => x.Amount)),
                    PointGeometrySize = 10,
                    StrokeThickness = 3
                }
            };

            OnPropertyChanged(nameof(VendorLabels));
            OnPropertyChanged(nameof(TrendLabels));
        }

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
                }
                LoadData(); // Tự động load dữ liệu sau khi chọn
            }
            finally { _isInternalChange = false; }
        }

        private void UpdateSuggestions(string type, string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
            {
                if (type == "M") { ModelSuggestions.Clear(); IsDropDownOpenM = false; SelectedModelId = 0; }
                if (type == "V") { VendorSuggestions.Clear(); IsDropDownOpenV = false; SelectedVendorId = 0; }
                if (type == "E") { EquipmentSuggestions.Clear(); IsDropDownOpenE = false; SelectedEquipmentId = 0; }
                return;
            }
            switch (type)
            {
                case "M":
                    var resM = _searchService.SearchModel(query);
                    ModelSuggestions.Clear();
                    foreach (var i in resM) ModelSuggestions.Add(i);
                    IsDropDownOpenM = ModelSuggestions.Any();
                    // BỎ DÒNG: SelectedModel = ModelSuggestions.FirstOrDefault();
                    break;
                case "V":
                    var resV = _searchService.SearchVendor(query);
                    VendorSuggestions.Clear();
                    foreach (var i in resV) VendorSuggestions.Add(i);
                    IsDropDownOpenV = VendorSuggestions.Any();
                    // BỎ DÒNG: SelectedVendor = VendorSuggestions.FirstOrDefault();
                    break;
                case "E":
                    var resE = _searchService.SearchEquipment(query);
                    EquipmentSuggestions.Clear();
                    foreach (var i in resE) EquipmentSuggestions.Add(i);
                    IsDropDownOpenE = EquipmentSuggestions.Any();
                    // BỎ DÒNG: SelectedEquipment = EquipmentSuggestions.FirstOrDefault();
                    break;
            }
        }

        private void ClearAll()
        {
            _isInternalChange = true;
            Analysis = new ModelAnalysisDto();
            SearchModelText = SearchVendorText = SearchEquipmentText = string.Empty;
            SelectedModelId = SelectedVendorId = SelectedEquipmentId = 0;
            PriceMin = PriceMax = null;
            SelectedDateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            SelectedDateTo = DateTime.Now;
            VendorPriceSeries?.Clear();
            PriceTrendSeries?.Clear();
            _isInternalChange = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}