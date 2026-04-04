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
using WpfApp2.modelDTO.analysisDto.ShareDto; // Namespace chứa AnalysisShareDto
using WpfApp2.Services;
using WpfApp2.Services.analysisService;
using WpfApp2.Services.exception;

namespace WpfApp2.viewmodel.analysis
{
    public class BrandAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly BrandAnalysisSv _service = new BrandAnalysisSv();
        private bool _isInternalChange; // Cờ chặn vòng lặp khi cập nhật UI từ code

        #region ===================== Properties =====================

        private BrandAnalysisDto _analysis;
        public BrandAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        // --- CHART BINDING ---
        private ChartValues<decimal> _monthlyValues = new ChartValues<decimal>();
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

        private SeriesCollection _pieSeriesCollection = new SeriesCollection();
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
        public DateTime? FromDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        public DateTime? ToDate { get; set; } = DateTime.Now;

        private int _selectedBrandId;
        public int SelectedBrandId
        {
            get => _selectedBrandId;
            set { _selectedBrandId = value; OnPropertyChanged(); }
        }

        private string _globalSearchText;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                if (_globalSearchText == value) return;
                _globalSearchText = value;
                OnPropertyChanged();

                // Chỉ tìm kiếm nếu sự thay đổi đến từ việc người dùng gõ phím
                if (!_isInternalChange)
                {
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
        public ICommand AnalyzeCommand { get; }
        public ICommand NavModelAnalysis { get; }
        public ICommand NavVendorAnalysis { get; }
        public ICommand NavEquipmentAnalysis { get; }

        #endregion

        public BrandAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());

            // Điều hướng giữa các trang phân tích
            NavModelAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Model đang được cập nhật."));
            NavVendorAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Vendor đang được cập nhật."));
            NavEquipmentAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Thiết bị đang được cập nhật."));
        }

        #region ===================== Methods =====================

        /// <summary>
        /// Thực thi lấy dữ liệu từ Service dựa trên ID Thương hiệu và Ngày tháng
        /// </summary>
        private void LoadData()
        {
            // Nếu chưa chốt ID nhưng có gợi ý đang highlight, tự động xác nhận
            if (SelectedBrandId == 0 && SelectedSearchResult != null)
            {
                ConfirmSelection(SelectedSearchResult);
                return;
            }

            if (SelectedBrandId == 0)
            {
                MessageBox.Show("Vui lòng nhập và chọn một Thương hiệu để phân tích.", "Thông báo");
                return;
            }

            try
            {
                var data = _service.GetBrandAnalysis(SelectedBrandId, FromDate, ToDate);
                Analysis = data;
                UpdateUIComponents(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Database đang bận (Locked). Vui lòng thử lại sau.", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật các thành phần Biểu đồ và Thẻ thông tin
        /// </summary>
        private void UpdateUIComponents(BrandAnalysisDto data)
        {
            if (data == null) return;

            // 1. Biểu đồ cột: Chi tiêu theo tháng
            MonthlyValues.Clear();
            MonthlyValues.AddRange(data.MonthlySpends.Select(x => x.Amount));
            MonthlyLabels = data.MonthlySpends.Select(x => x.MonthYear).ToArray();

            // 2. Biểu đồ tròn: Tỷ lệ theo Model (Dùng chung CategoryName từ AnalysisShareDto)
            var pieCollection = new SeriesCollection();
            foreach (var share in data.ModelShares)
            {
                pieCollection.Add(new PieSeries
                {
                    Title = share.CategoryName, // Chỗ này quan trọng: đồng bộ với Common Share DTO
                    Values = new ChartValues<decimal> { share.TotalAmount },
                    DataLabels = true,
                    LabelPoint = p => $"{p.Participation:P1}"
                });
            }
            PieSeriesCollection = pieCollection;

            // 3. KPI Top Model (Lấy Model chiếm tỉ trọng tiền cao nhất)
            var top = data.ModelShares.OrderByDescending(x => x.TotalAmount).FirstOrDefault();
            TopModelDisplay = top != null ? $"{top.CategoryName} ({top.Percentage:F1}%)" : "N/A";
        }

        /// <summary>
        /// Cập nhật danh sách gợi ý khi gõ TextBox
        /// </summary>
        private void UpdateSuggestions()
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 2)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                SelectedBrandId = 0;
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

        /// <summary>
        /// Xác nhận lựa chọn Thương hiệu từ gợi ý
        /// </summary>
        public void ConfirmSelection(SearchResultDto item = null)
        {
            var target = item ?? SelectedSearchResult;
            if (target == null) return;

            _isInternalChange = true;
            try
            {
                SelectedBrandId = target.Id;
                GlobalSearchText = target.Text;
                IsSearchDropDownOpen = false;

                // Tự động load dữ liệu ngay sau khi chọn xong
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