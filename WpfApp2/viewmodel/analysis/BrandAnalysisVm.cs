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
using WpfApp2.Services.exception;

namespace WpfApp2.viewmodel.analysis
{
    public class BrandAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly BrandAnalysisSv _service = new BrandAnalysisSv();
        private bool _isInternalChange;

        #region ===================== Properties =====================

        // DTO Tổng chứa toàn bộ dữ liệu (TotalPrice, TotalTransactions, Items...)
        private BrandAnalysisDto _analysis;
        public BrandAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        // --- CHART BINDING ---
        private ChartValues<decimal> _monthlyValues;
        public ChartValues<decimal> MonthlyValues
        {
            get => _monthlyValues;
            set { _monthlyValues = value; OnPropertyChanged(); }
        }

        private string[] _monthlyLabels;
        public string[] MonthlyLabels
        {
            get => _monthlyLabels;
            set { _monthlyLabels = value; OnPropertyChanged(); }
        }

        private SeriesCollection _pieSeriesCollection;
        public SeriesCollection PieSeriesCollection
        {
            get => _pieSeriesCollection;
            set { _pieSeriesCollection = value; OnPropertyChanged(); }
        }

        private string _topModelDisplay = "N/A";
        public string TopModelDisplay
        {
            get => _topModelDisplay;
            set { _topModelDisplay = value; OnPropertyChanged(); }
        }

        // --- FILTER & SEARCH ---
        private DateTime? _fromDate = new DateTime(DateTime.Now.Year, 1, 1);
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }

        private DateTime? _toDate = DateTime.Now;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

        private int _selectedBrandId;
        public int SelectedBrandId { get => _selectedBrandId; set { _selectedBrandId = value; OnPropertyChanged(); } }

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

        public ObservableCollection<SearchResultDto> SearchSuggestions { get; } = new ObservableCollection<SearchResultDto>();

        private SearchResultDto _selectedSearchResult;
        public SearchResultDto SelectedSearchResult
        {
            get => _selectedSearchResult;
            set { _selectedSearchResult = value; OnPropertyChanged(); }
        }

        private bool _isSearchDropDownOpen;
        public bool IsSearchDropDownOpen
        {
            get => _isSearchDropDownOpen;
            set { _isSearchDropDownOpen = value; OnPropertyChanged(); }
        }

        // --- COMMANDS ---
        public ICommand AnalyzeCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }
        public ICommand ExportPdfCommand { get; set; }

        // Navigation Commands
        public ICommand NavModelAnalysis { get; set; }
        public ICommand NavVendorAnalysis { get; set; }
        public ICommand NavEquipmentAnalysis { get; set; }

        #endregion

        public BrandAnalysisVm()
        {
            // Khởi tạo các Command
            AnalyzeCommand = new RelayCommand(_ => LoadData());

            ExportExcelCommand = new RelayCommand(_ => MessageBox.Show("Tính năng Xuất Excel đang được phát triển."));
            ExportPdfCommand = new RelayCommand(_ => MessageBox.Show("Tính năng Xuất PDF đang được phát triển."));

            // Giả lập điều hướng (Thay bằng logic NavigationService của bạn)
            NavModelAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển sang Phân tích Model"));
            NavVendorAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển sang Phân tích Vendor"));
            NavEquipmentAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển sang Phân tích Thiết bị"));
        }

        #region ===================== Methods =====================

        private void LoadData()
        {
            // Nếu người dùng chọn item từ Suggestion mà chưa ConfirmSelection thì tự confirm luôn
            if (SelectedBrandId == 0 && SelectedSearchResult != null)
            {
                ConfirmSelection();
            }

            if (SelectedBrandId == 0)
            {
                MessageBox.Show("Vui lòng chọn một Thương hiệu để phân tích.", "Thông báo");
                return;
            }

            try
            {
                var data = _service.GetBrandAnalysis(SelectedBrandId, FromDate, ToDate);
                Analysis = data;
                UpdateChartData(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Hệ thống Database đang bận. Vui lòng thử lại sau.", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void UpdateChartData(BrandAnalysisDto data)
        {
            if (data == null) return;

            // 1. Biểu đồ cột (Chi tiêu hàng tháng)
            MonthlyValues = new ChartValues<decimal>(data.MonthlySpends.Select(x => x.Amount));
            MonthlyLabels = data.MonthlySpends.Select(x => x.MonthYear).ToArray();

            // 2. Biểu đồ tròn (Tỷ lệ Model)
            var pieCollection = new SeriesCollection();
            foreach (var share in data.ModelShares)
            {
                pieCollection.Add(new PieSeries
                {
                    Title = share.ModelCode,
                    Values = new ChartValues<decimal> { share.TotalAmount },
                    DataLabels = true,
                    LabelPoint = p => $"{p.Participation:P1}"
                });
            }
            PieSeriesCollection = pieCollection;

            // 3. Thẻ KPI Top Model
            var topModel = data.ModelShares.OrderByDescending(x => x.TotalAmount).FirstOrDefault();
            TopModelDisplay = topModel != null
                ? $"{topModel.ModelCode} ({topModel.Percentage:F1}%)"
                : "N/A";
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
                var results = _searchService.SearchBrand(GlobalSearchText);
                SearchSuggestions.Clear();
                foreach (var item in results) SearchSuggestions.Add(item);

                IsSearchDropDownOpen = SearchSuggestions.Any();
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException) { IsSearchDropDownOpen = false; }
        }

        public void ConfirmSelection(SearchResultDto explicitItem = null)
        {
            var target = explicitItem ?? SelectedSearchResult;
            if (target == null) return;

            _isInternalChange = true;
            try
            {
                SelectedBrandId = target.Id;
                GlobalSearchText = target.Text;
                IsSearchDropDownOpen = false; // Đóng popup

                // Gọi lệnh phân tích ngay lập tức sau khi chọn
                LoadData();
            }
            finally
            {
                _isInternalChange = false;
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