using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDto;
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
            set { if (_analysis == value) return; _analysis = value; OnPropertyChanged(); }
        }

        private SeriesCollection _pieSeriesCollection = new SeriesCollection();
        public SeriesCollection PieSeriesCollection
        {
            get => _pieSeriesCollection;
            set { if (_pieSeriesCollection == value) return; _pieSeriesCollection = value; OnPropertyChanged(); }
        }

        private ChartValues<decimal> _topItemsValues = new ChartValues<decimal>();
        public ChartValues<decimal> TopItemsValues
        {
            get => _topItemsValues;
            set { if (_topItemsValues == value) return; _topItemsValues = value; OnPropertyChanged(); }
        }

        private string[] _topItemsLabels;
        public string[] TopItemsLabels
        {
            get => _topItemsLabels;
            set { if (_topItemsLabels == value) return; _topItemsLabels = value; OnPropertyChanged(); }
        }

        private DateTime? _fromDate = new DateTime(DateTime.Now.Year, 1, 1);
        public DateTime? FromDate
        {
            get => _fromDate;
            set { if (_fromDate == value) return; _fromDate = value; OnPropertyChanged(); }
        }

        private DateTime? _toDate = DateTime.Now;
        public DateTime? ToDate
        {
            get => _toDate;
            set { if (_toDate == value) return; _toDate = value; OnPropertyChanged(); }
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

        // Thay đổi quan trọng: Dùng int? để phân biệt null (chưa chọn) và 0 (Id hợp lệ)
        private int? _selectedEquipmentId = null;
        public int? SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set { if (_selectedEquipmentId == value) return; _selectedEquipmentId = value; OnPropertyChanged(); }
        }

        public ICommand AnalyzeCommand { get; }
        public ICommand NavModelAnalysis { get; }
        public ICommand NavVendorAnalysis { get; }
        public ICommand NavBrandAnalysis { get; }
        public ICommand NavEquipmentAnalysis { get; }

        #endregion

        public EquipmentAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());
            NavModelAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển hướng Model..."));
            NavVendorAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển hướng Vendor..."));
            NavBrandAnalysis = new RelayCommand(_ => MessageBox.Show("Chuyển hướng Thương Hiệu..."));
            NavEquipmentAnalysis = new RelayCommand(_ => { /* Đang ở đây */ });
        }

        #region ===================== Methods =====================

        private void LoadData()
        {
            // Tự động gán nếu người dùng nhấn Analyze khi chưa chọn từ danh sách dropdown
            if (SelectedEquipmentId == null && SelectedSearchResult != null)
            {
                InternalApplySelection(SelectedSearchResult);
            }

            if (SelectedEquipmentId == null)
            {
                MessageBox.Show("Vui lòng nhập và chọn một Thiết bị.", "Thông báo");
                return;
            }

            try
            {
                var data = _service.GetEquipmentAnalysis(SelectedEquipmentId.Value, FromDate, ToDate);
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

            // 1. Cập nhật Biểu đồ Tròn
            var pieCollection = new SeriesCollection();
            if (data.BrandShares != null)
            {
                var sortedBrands = data.BrandShares.OrderByDescending(x => x.TotalAmount).ToList();
                var top10Brands = sortedBrands.Take(10).ToList();
                var otherBrands = sortedBrands.Skip(10).ToList();

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

                if (otherBrands.Any())
                {
                    pieCollection.Add(new PieSeries
                    {
                        Title = "Các loại khác",
                        Values = new ChartValues<decimal> { otherBrands.Sum(x => x.TotalAmount) },
                        DataLabels = true,
                        Fill = Brushes.Gray,
                        LabelPoint = p => $"Khác: {p.Participation:P1}"
                    });
                }
            }
            PieSeriesCollection = pieCollection;

            // 2. Cập nhật Biểu đồ Cột ngang
            if (data.TopItems != null)
            {
                TopItemsValues = new ChartValues<decimal>(data.TopItems.Select(x => x.TotalAmount).Reverse());
                TopItemsLabels = data.TopItems.Select(x => x.CategoryName).Reverse().ToArray();
            }
        }

        private void UpdateSuggestions()
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 1)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                SelectedEquipmentId = null;
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

        private void InternalApplySelection(SearchResultDto target)
        {
            if (target == null) return;
            _isInternalChange = true;

            SelectedEquipmentId = target.Data switch
            {
                Equipment e => e.Id,
                EquipmentDto d => d.Id,
                _ => target.Id
            };

            GlobalSearchText = target.Text;
            IsSearchDropDownOpen = false;
            _isInternalChange = false;
        }

        public void ConfirmSelection(SearchResultDto explicitItem = null)
        {
            var target = explicitItem ?? SelectedSearchResult;
            if (target == null) return;

            InternalApplySelection(target);
            LoadData(); // Gọi load data sau khi đã thoát khỏi logic check id của LoadData
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}