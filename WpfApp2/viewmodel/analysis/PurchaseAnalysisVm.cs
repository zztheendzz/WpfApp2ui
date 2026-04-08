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

        public ObservableCollection<SearchResultDto> BrandSuggestions { get; } = new ObservableCollection<SearchResultDto>();
        // Selected Items for UI Highlight
        private SearchResultDto _selectedModel;
        public SearchResultDto SelectedModel { get => _selectedModel; set { _selectedModel = value; OnPropertyChanged(); } }

        private SearchResultDto _selectedVendor;
        public SearchResultDto SelectedVendor { get => _selectedVendor; set { _selectedVendor = value; OnPropertyChanged(); } }

        private SearchResultDto _selectedEquipment;
        public SearchResultDto SelectedEquipment { get => _selectedEquipment; set { _selectedEquipment = value; OnPropertyChanged(); } }

        private SearchResultDto _selectedBrand;
        public SearchResultDto SelectedBrand { get => _selectedBrand; set { _selectedBrand = value; OnPropertyChanged(); } }

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

        private string _searchBrandText;
        public string SearchBrandText
        {
            get => _searchBrandText;
            set
            {
                if (_searchBrandText == value) return;
                _searchBrandText = value;
                OnPropertyChanged();
                if (!_isInternalChange) UpdateSuggestions("B", value);
            }
        }

        // Popup States
        private bool _isDropDownOpenM;
        public bool IsDropDownOpenM { get => _isDropDownOpenM; set { _isDropDownOpenM = value; OnPropertyChanged(); } }

        private bool _isDropDownOpenV;
        public bool IsDropDownOpenV { get => _isDropDownOpenV; set { _isDropDownOpenV = value; OnPropertyChanged(); } }

        private bool _isDropDownOpenE;
        public bool IsDropDownOpenE { get => _isDropDownOpenE; set { _isDropDownOpenE = value; OnPropertyChanged(); } }

        private bool _isDropDownOpenB;
        public bool IsDropDownOpenB { get => _isDropDownOpenB; set { _isDropDownOpenB = value; OnPropertyChanged(); } }

        // Filters
        public int SelectedModelId { get; set; }
        public int SelectedVendorId { get; set; }
        public int SelectedEquipmentId { get; set; }
        public int SelectedBrandId { get; set; }
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
                    SelectedBrandId == 0 ? null : (int?)SelectedBrandId,
                    SelectedDateFrom,
                    SelectedDateTo,
                    PriceMin,
                    PriceMax
                );

                Analysis = result;
                UpdateCharts(result);
                MessageBox.Show($"Lỗi tải dữ liệu: {Analysis.TotalRecord}");
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
                else if (type == "B" && SelectedBrand != null)
                {
                    SearchBrandText = SelectedBrand.Text;
                    SelectedBrandId = SelectedBrand.Id;
                    IsDropDownOpenB = false;
                }
                LoadData(); // Tự động load dữ liệu sau khi chọn
            }
            finally { _isInternalChange = false; }
        }
        public void LoadAllSuggestions(string type)
        {
            switch (type)
            {
                case "M":
                    UpdateSuggestions("M", "__ALL__");
                    break;
                case "V":
                    UpdateSuggestions("V", "__ALL__");
                    break;
                case "E":
                    UpdateSuggestions("E", "__ALL__");
                    break;
                case "B":
                    UpdateSuggestions("B", "__ALL__");
                    break;
            }
        }
        private void UpdateSuggestions(string type, string query)
        {
            bool isAll = query == "__ALL__";

            switch (type)
            {
                case "M":
                    var resM = isAll
                        ? _searchService.SearchModel("")
                        : _searchService.SearchModel(query);

                    ModelSuggestions.Clear();
                    foreach (var i in resM) ModelSuggestions.Add(i);

                    IsDropDownOpenM = true; // 🔥 luôn mở
                    SelectedModelId = 0;
                    break;

                case "V":
                    var resV = isAll
                        ? _searchService.SearchVendor("")
                        : _searchService.SearchVendor(query);

                    VendorSuggestions.Clear();
                    foreach (var i in resV) VendorSuggestions.Add(i);

                    IsDropDownOpenV = true;
                    SelectedVendorId = 0;
                    break;

                case "E":
                    var resE = isAll
                        ? _searchService.SearchEquipment("")
                        : _searchService.SearchEquipment(query);

                    EquipmentSuggestions.Clear();
                    foreach (var i in resE) EquipmentSuggestions.Add(i);

                    IsDropDownOpenE = true;
                    SelectedEquipmentId = 0;
                    break;

                case "B":
                    var resB = isAll
                        ? _searchService.SearchBrand("")
                        : _searchService.SearchBrand(query);

                    BrandSuggestions.Clear();
                    foreach (var i in resB) BrandSuggestions.Add(i);

                    IsDropDownOpenB = true;
                    SelectedBrandId = 0;
                    break;
            }
        }

        private void ClearAll()
        {
            _isInternalChange = true;
            Analysis = new ModelAnalysisDto();
            SearchModelText = SearchVendorText = SearchEquipmentText = SearchBrandText = string.Empty;
            SelectedModelId = SelectedVendorId = SelectedEquipmentId = SelectedBrandId = 0;
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