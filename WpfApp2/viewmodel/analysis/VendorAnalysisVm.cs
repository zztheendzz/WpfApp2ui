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
    public class VendorAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly VendorAnalysisSv _service = new VendorAnalysisSv();
        private bool _isInternalChange;

        #region ===================== Properties =====================

        private VendorAnalysisDto _analysis;
        public VendorAnalysisDto Analysis
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

        // Thay đổi quan trọng: Dùng int? để chấp nhận ID = 0 là một giá trị hợp lệ
        private int? _selectedVendorId = null;
        public int? SelectedVendorId
        {
            get => _selectedVendorId;
            set { if (_selectedVendorId == value) return; _selectedVendorId = value; OnPropertyChanged(); }
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
        public ICommand NavBrandAnalysis { get; }
        public ICommand NavEquipmentAnalysis { get; }

        #endregion

        public VendorAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());
            NavModelAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Model đang được cập nhật."));
            NavBrandAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển sang Dashboard Thương hiệu."));
            NavEquipmentAnalysis = new RelayCommand(_ => MessageBox.Show("Tính năng Phân tích Thiết bị đang được cập nhật."));
        }

        #region ===================== Methods =====================

        private void LoadData()
        {
            // Nếu chưa có ID nhưng đang có item được chọn trong list gợi ý, tự động gán luôn
            if (SelectedVendorId == null && SelectedSearchResult != null)
            {
                InternalApplySelection(SelectedSearchResult);
            }

            // Kiểm tra thực tế xem có ID để load chưa (chấp nhận cả ID = 0)
            if (SelectedVendorId == null)
            {
                MessageBox.Show("Vui lòng nhập và chọn một Nhà cung cấp từ danh sách gợi ý.", "Thông báo");
                return;
            }

            try
            {
                // Truyền SelectedVendorId.Value vì Service cần int
                var data = _service.GetVendorAnalysis(SelectedVendorId.Value, FromDate, ToDate);
                Analysis = data;
                MessageBox.Show("data = "+ Analysis.Items.Count + "id= "+ SelectedVendorId);
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

        private void UpdateUIComponents(VendorAnalysisDto data)
        {
            if (data == null) return;

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

            var top = data.ModelShares?.OrderByDescending(x => x.TotalAmount).FirstOrDefault();
            TopModelDisplay = top != null ? $"{top.CategoryName} ({top.Percentage:F1}%)" : "N/A";

            OnPropertyChanged(nameof(MonthlyLabels));
        }

        private void UpdateSuggestions()
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchText))
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                SelectedVendorId = null; // Reset về null thay vì 0
                return;
            }

            try
            {
                var results = _searchService.SearchVendor(GlobalSearchText);
                SearchSuggestions.Clear();
                foreach (var item in results) SearchSuggestions.Add(item);

                IsSearchDropDownOpen = SearchSuggestions.Any();
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException) { IsSearchDropDownOpen = false; }
        }

        /// <summary>
        /// Hàm nội bộ để gán dữ liệu mà không gây ra vòng lặp vô tận
        /// </summary>
        private void InternalApplySelection(SearchResultDto target)
        {
            if (target == null) return;
            _isInternalChange = true;
            SelectedVendorId = target.Id;
            GlobalSearchText = target.Text;
            IsSearchDropDownOpen = false;
            _isInternalChange = false;
        }


        public void ConfirmSelection(SearchResultDto item = null)
        {
            var target = item ?? SelectedSearchResult;
            if (target == null) return;

            InternalApplySelection(target);
            LoadData(); // Gọi LoadData từ UI Action là an toàn
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}