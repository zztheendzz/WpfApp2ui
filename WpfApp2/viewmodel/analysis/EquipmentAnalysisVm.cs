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
using WpfApp2.model; // Chứa class Equipment
using WpfApp2.modelDto; // Chứa EquipmentDto
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.modelDTO.analysisDto.ShareDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;
using WpfApp2.Services.exception;

namespace WpfApp2.viewmodel.analysis
{
    public class EquipmentAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly EquipmentAnalysisSv _service = new EquipmentAnalysisSv();
        private bool _isInternalChange;

        #region ===================== Properties =====================

        private EquipmentAnalysisDto _analysis;
        public EquipmentAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        // --- CHART BINDING ---

        // Biểu đồ tròn: Tỷ lệ chi tiêu theo Thương hiệu trong thiết bị
        private SeriesCollection _pieSeriesCollection = new SeriesCollection();
        public SeriesCollection PieSeriesCollection
        {
            get => _pieSeriesCollection;
            set { _pieSeriesCollection = value; OnPropertyChanged(); }
        }

        // Biểu đồ cột ngang: Top 5 linh kiện giá trị cao nhất
        private ChartValues<decimal> _topItemsValues = new ChartValues<decimal>();
        public ChartValues<decimal> TopItemsValues
        {
            get => _topItemsValues;
            set { _topItemsValues = value; OnPropertyChanged(); }
        }

        private string[] _topItemsLabels;
        public string[] TopItemsLabels
        {
            get => _topItemsLabels;
            set { _topItemsLabels = value; OnPropertyChanged(); }
        }

        // --- FILTER & SEARCH ---
        private DateTime? _fromDate = new DateTime(DateTime.Now.Year, 1, 1);
        public DateTime? FromDate
        {
            get => _fromDate;
            set { _fromDate = value; OnPropertyChanged(); }
        }

        private DateTime? _toDate = DateTime.Now;
        public DateTime? ToDate
        {
            get => _toDate;
            set { _toDate = value; OnPropertyChanged(); }
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

                if (!_isInternalChange) UpdateSuggestions();
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

        private int _selectedEquipmentId;
        public int SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set { _selectedEquipmentId = value; OnPropertyChanged(); }
        }

        // --- COMMANDS ---
        public ICommand AnalyzeCommand { get; }
        public ICommand NavModelAnalysis { get; }
        public ICommand NavVendorAnalysis { get; }
        public ICommand NavBrandAnalysis { get; }
        public ICommand NavEquipmentAnalysis { get; }

        #endregion

        public EquipmentAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());

            // Điều hướng (Giả sử bạn dùng NavigationService hoặc gán MessageBox như Brand)
            NavModelAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển hướng Model..."));
            NavVendorAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển hướng Vendor..."));
            NavBrandAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển hướng Thương Hiệu..."));
            NavEquipmentAnalysis = new RelayCommand(_ => { /* Đang ở đây rồi */ });
        }

        #region ===================== Methods =====================

        private void LoadData()
        {
            if (SelectedEquipmentId <= 0)
            {
                if (SelectedSearchResult != null) ConfirmSelection(SelectedSearchResult);
                else return;
            }

            try
            {
                var data = _service.GetEquipmentAnalysis(SelectedEquipmentId, FromDate, ToDate);
                if (data != null)
                {
                    Analysis = data;
                    UpdateUIComponents(data);
                }
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Database đang bận. Thử lại sau.", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void UpdateUIComponents(EquipmentAnalysisDto data)
        {
            if (data == null) return;

            // 1. Cập nhật Biểu đồ Tròn (Brand Shares) - Lấy Top 10 + Nhóm "Khác"
            var pieCollection = new SeriesCollection();

            // Sắp xếp giảm dần theo số tiền
            var sortedBrands = data.BrandShares.OrderByDescending(x => x.TotalAmount).ToList();

            // Lấy 10 thằng đầu tiên
            var top10Brands = sortedBrands.Take(2).ToList();

            // Những thằng còn lại (từ vị trí thứ 11 trở đi)
            var otherBrands = sortedBrands.Skip(10).ToList();

            // Thêm Top 10 vào biểu đồ
            foreach (var share in top10Brands)
            {
                pieCollection.Add(new PieSeries
                {
                    Title = share.CategoryName,
                    Values = new ChartValues<decimal> { share.TotalAmount },
                    DataLabels = true,
                    LabelPoint = p => $"{p.SeriesView.Title}: {p.Participation:P1}"
                });
            }

            // Nếu có các thương hiệu ngoài Top 10, gộp chúng lại thành "Các loại khác"
            if (otherBrands.Any())
            {
                decimal otherTotal = otherBrands.Sum(x => x.TotalAmount);
                pieCollection.Add(new PieSeries
                {
                    Title = "Các loại khác",
                    Values = new ChartValues<decimal> { otherTotal },
                    DataLabels = true,
                    Fill = System.Windows.Media.Brushes.Gray, // Đặt màu xám để phân biệt
                    LabelPoint = p => $"Khác: {p.Participation:P1}"
                });
            }

            // Gán 1 lần duy nhất để cập nhật UI mượt mà
            PieSeriesCollection = pieCollection;

            // 2. Cập nhật Biểu đồ Cột ngang (Top 5 Items) - Giữ nguyên logic cũ
            if (data.TopItems != null)
            {
                TopItemsValues = new ChartValues<decimal>(data.TopItems.Select(x => x.TotalAmount).Reverse());
                TopItemsLabels = data.TopItems.Select(x => x.CategoryName).Reverse().ToArray();
            }
        }

        private void UpdateSuggestions()
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 2)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                SelectedEquipmentId = 0;
                return;
            }

            try
            {
                var results = _searchService.SearchEquipment(GlobalSearchText);
                SearchSuggestions.Clear();
                foreach (var item in results) SearchSuggestions.Add(item);

                IsSearchDropDownOpen = SearchSuggestions.Any();
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch { IsSearchDropDownOpen = false; }
        }

        public void ConfirmSelection(SearchResultDto explicitItem = null)
        {
            var target = explicitItem ?? SelectedSearchResult;
            if (target == null) return;

            _isInternalChange = true;
            try
            {
                SelectedEquipmentId = target.Data switch
                {
                    Equipment e => e.Id,
                    EquipmentDto d => d.Id,
                    _ => target.Id
                };

                GlobalSearchText = target.Text;
                IsSearchDropDownOpen = false;
                LoadData();
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}