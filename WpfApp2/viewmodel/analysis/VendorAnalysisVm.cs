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
    public class VendorAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly VendorAnalysisSv _service = new VendorAnalysisSv();
        private bool _isInternalChange; // Cờ chặn vòng lặp khi cập nhật UI từ code

        #region ===================== Properties =====================

        private VendorAnalysisDto _analysis;
        public VendorAnalysisDto Analysis
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

        private int _selectedVendorId;
        public int SelectedVendorId
        {
            get => _selectedVendorId;
            set { _selectedVendorId = value; OnPropertyChanged(); }
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

                // Chỉ thực hiện tìm kiếm gợi ý nếu thay đổi đến từ việc gõ phím (không phải do gán code)
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
        public ICommand NavBrandAnalysis { get; }
        public ICommand NavEquipmentAnalysis { get; }

        #endregion

        public VendorAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());

            // Điều hướng giữa các Dashboard (Giả định)
            NavModelAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Model đang được cập nhật."));
            NavBrandAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển sang Dashboard Thương hiệu."));
            NavEquipmentAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Thiết bị đang được cập nhật."));
        }

        #region ===================== Methods =====================

        /// <summary>
        /// Thực thi lấy dữ liệu từ Service dựa trên ID Nhà cung cấp và Khoảng ngày
        /// </summary>
        private void LoadData()
        {
            // Tự động xác nhận gợi ý đang chọn nếu người dùng nhấn Analyze mà chưa Enter chọn Item
            if (SelectedVendorId == 0 && SelectedSearchResult != null)
            {
                ConfirmSelection(SelectedSearchResult);
                return;
            }

            if (SelectedVendorId == 0)
            {
                MessageBox.Show("Vui lòng nhập và chọn một Nhà cung cấp từ danh sách gợi ý.", "Thông báo");
                return;
            }

            try
            {
                var data = _service.GetVendorAnalysis(SelectedVendorId, FromDate, ToDate);
                Analysis = data;
                UpdateUIComponents(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Cơ sở dữ liệu đang bận. Vui lòng thử lại sau ít giây.", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật dữ liệu cho các Chart và Thẻ thông tin (KPI)
        /// </summary>
        private void UpdateUIComponents(VendorAnalysisDto data)
        {
            if (data == null) return;

            // 1. Cập nhật biểu đồ cột: Chi tiêu theo tháng tại Vendor này
            MonthlyValues.Clear();
            if (data.MonthlySpends != null && data.MonthlySpends.Any())
            {
                MonthlyValues.AddRange(data.MonthlySpends.Select(x => x.Amount));
                MonthlyLabels = data.MonthlySpends.Select(x => x.MonthYear).ToArray();
            }
            else
            {
                MonthlyLabels = Array.Empty<string>();
            }

            // 2. Cập nhật biểu đồ tròn: Tỷ lệ các Model đã mua từ Vendor này
            var pieCollection = new SeriesCollection();
            if (data.ModelShares != null)
            {
                foreach (var share in data.ModelShares)
                {
                    pieCollection.Add(new PieSeries
                    {
                        Title = share.CategoryName, // Chứa ModelCode
                        Values = new ChartValues<decimal> { share.TotalAmount },
                        DataLabels = true,
                        LabelPoint = p => $"{p.Participation:P1}"
                    });
                }
            }
            PieSeriesCollection = pieCollection;

            // 3. Hiển thị thông tin Model chiếm tỷ trọng cao nhất
            var top = data.ModelShares?.OrderByDescending(x => x.TotalAmount).FirstOrDefault();
            TopModelDisplay = top != null ? $"{top.CategoryName} ({top.Percentage:F1}%)" : "N/A";

            // Cập nhật lại Labels cho trục X của Chart cột
            OnPropertyChanged(nameof(MonthlyLabels));
        }

        /// <summary>
        /// Gọi SearchService để lấy danh sách gợi ý Nhà cung cấp
        /// </summary>
        private void UpdateSuggestions()
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 1)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                SelectedVendorId = 0;
                return;
            }

            try
            {
                // Sử dụng SearchVendor thay vì SearchBrand
                var results = _searchService.SearchVendor(GlobalSearchText);
                SearchSuggestions.Clear();
                foreach (var item in results) SearchSuggestions.Add(item);

                IsSearchDropDownOpen = SearchSuggestions.Any();
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException) { IsSearchDropDownOpen = false; }
        }

        /// <summary>
        /// Chốt lựa chọn từ danh sách Popup và tiến hành tải dữ liệu
        /// </summary>
        public void ConfirmSelection(SearchResultDto item = null)
        {
            var target = item ?? SelectedSearchResult;
            if (target == null) return;

            _isInternalChange = true;
            try
            {
                SelectedVendorId = target.Id;
                GlobalSearchText = target.Text;
                IsSearchDropDownOpen = false;

                // Tự động phân tích ngay sau khi chọn xong Nhà cung cấp
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