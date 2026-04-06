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
using WpfApp2.modelDTO.analysisDto.ShareDto;
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

        private BrandAnalysisDto _analysis;
        public BrandAnalysisDto Analysis
        {
            get => _analysis;
            set { if (_analysis == value) return; _analysis = value; OnPropertyChanged(); }
        }

        private ChartValues<decimal> _monthlyValues = new ChartValues<decimal>();
        public ChartValues<decimal> MonthlyValues
        {
            get => _monthlyValues;
            set { if (_monthlyValues == value) return; _monthlyValues = value; OnPropertyChanged(); }
        }

        private string[] _monthlyLabels;
        public string[] MonthlyLabels
        {
            get => _monthlyLabels;
            set { if (_monthlyLabels == value) return; _monthlyLabels = value; OnPropertyChanged(); }
        }

        private SeriesCollection _pieSeriesCollection = new SeriesCollection();
        public SeriesCollection PieSeriesCollection
        {
            get => _pieSeriesCollection;
            set { if (_pieSeriesCollection == value) return; _pieSeriesCollection = value; OnPropertyChanged(); }
        }

        private string _topModelDisplay = "N/A";
        public string TopModelDisplay
        {
            get => _topModelDisplay;
            set { if (_topModelDisplay == value) return; _topModelDisplay = value; OnPropertyChanged(); }
        }

        public DateTime? FromDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        public DateTime? ToDate { get; set; } = DateTime.Now;

        // Dùng int? để phân biệt null (chưa chọn) và 0 (Id hợp lệ từ DB)
        private int? _selectedBrandId = null;
        public int? SelectedBrandId
        {
            get => _selectedBrandId;
            set { if (_selectedBrandId == value) return; _selectedBrandId = value; OnPropertyChanged(); }
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
            set { if (_selectedSearchResult == value) return; _selectedSearchResult = value; OnPropertyChanged(); }
        }

        private bool _isSearchDropDownOpen;
        public bool IsSearchDropDownOpen
        {
            get => _isSearchDropDownOpen;
            set { if (_isSearchDropDownOpen == value) return; _isSearchDropDownOpen = value; OnPropertyChanged(); }
        }

        public ICommand AnalyzeCommand { get; }
        public ICommand NavModelAnalysis { get; }
        public ICommand NavVendorAnalysis { get; }
        public ICommand NavEquipmentAnalysis { get; }

        #endregion

        public BrandAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());

            NavModelAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Model đang được cập nhật."));
            NavVendorAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Vendor đang được cập nhật."));
            NavEquipmentAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Thiết bị đang được cập nhật."));
        }

        #region ===================== Methods =====================

        private void LoadData()
        {
            // Tự động xác nhận gợi ý nếu nhấn Analyze mà chưa chọn từ danh sách
            if (SelectedBrandId == null && SelectedSearchResult != null)
            {
                InternalApplySelection(SelectedSearchResult);
            }

            if (SelectedBrandId == null)
            {
                MessageBox.Show("Vui lòng nhập và chọn một Thương hiệu để phân tích.", "Thông báo");
                return;
            }

            try
            {
                var data = _service.GetBrandAnalysis(SelectedBrandId.Value, FromDate, ToDate);
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

        private void UpdateUIComponents(BrandAnalysisDto data)
        {
            if (data == null) return;

            // 1. Biểu đồ cột
            MonthlyValues.Clear();
            if (data.MonthlySpends != null)
            {
                MonthlyValues.AddRange(data.MonthlySpends.Select(x => x.Amount));
                MonthlyLabels = data.MonthlySpends.Select(x => x.MonthYear).ToArray();
            }

            // 2. Biểu đồ tròn
            var pieCollection = new SeriesCollection();
            if (data.ModelShares != null)
            {
                foreach (var share in data.ModelShares)
                {
                    pieCollection.Add(new PieSeries
                    {
                        Title = share.CategoryName,
                        Values = new ChartValues<decimal> { share.TotalAmount },
                        DataLabels = true,
                        LabelPoint = p => $"{p.Participation:P1}"
                    });
                }
            }
            PieSeriesCollection = pieCollection;

            // 3. KPI Top Model
            var top = data.ModelShares?.OrderByDescending(x => x.TotalAmount).FirstOrDefault();
            TopModelDisplay = top != null ? $"{top.CategoryName} ({top.Percentage:F1}%)" : "N/A";

            OnPropertyChanged(nameof(MonthlyLabels));
        }

        private void UpdateSuggestions()
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 1)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                SelectedBrandId = null;
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
        /// Gán dữ liệu nội bộ để tránh gây ra vòng lặp đệ quy vô tận với LoadData
        /// </summary>
        private void InternalApplySelection(SearchResultDto target)
        {
            if (target == null) return;
            _isInternalChange = true;
            try
            {
                SelectedBrandId = target.Id;
                GlobalSearchText = target.Text;
                IsSearchDropDownOpen = false;
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        public void ConfirmSelection(SearchResultDto item = null)
        {
            var target = item ?? SelectedSearchResult;
            if (target == null) return;

            InternalApplySelection(target);
            LoadData(); // Gọi LoadData từ hành động người dùng là an toàn
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}