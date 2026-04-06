using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows; // Thêm để sử dụng MessageBox
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysisDto;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;
using WpfApp2.Services.exception;

namespace WpfApp2.viewmodel.analysis
{
    public class ModelAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly ModelAnalysisSv _service = new ModelAnalysisSv();
        public ModelVendorMatrixDto MatrixData { get; set; } = new ModelVendorMatrixDto
        {
            Vendors = new List<string>(),
            Rows = new List<ModelVendorMatrixRowDto>()
        };
        // Cờ chặn vòng lặp phản hồi khi gán text từ kết quả chọn
        private bool _isInternalChange;

        #region ===================== Properties =====================

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
                    if (string.IsNullOrWhiteSpace(value)) SelectedModelId = 0;
                    UpdateSuggestions();
                }
            }
        }

        private SearchResultDto _selectedSearchResult;
        public SearchResultDto SelectedSearchResult
        {
            get => _selectedSearchResult;
            set
            {
                if (_selectedSearchResult == value) return;
                _selectedSearchResult = value;
                OnPropertyChanged();
            }
        }

        private bool _isSearchDropDownOpen;
        public bool IsSearchDropDownOpen
        {
            get => _isSearchDropDownOpen;
            set
            {
                if (_isSearchDropDownOpen == value) return;
                _isSearchDropDownOpen = value;
                OnPropertyChanged();
            }
        }

        private int _selectedModelId;
        public int SelectedModelId
        {
            get => _selectedModelId;
            set { _selectedModelId = value; OnPropertyChanged(); }
        }

        private ModelAnalysisDto _analysis;
        public ModelAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        public ICommand AnalyzeCommand { get; set; }
        public ObservableCollection<SearchResultDto> SearchSuggestions { get; } = new ObservableCollection<SearchResultDto>();

        #endregion

        public ModelAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());
            var service = new ModelAnalysisSv();


        }

        #region ===================== Methods =====================

        public void AddModelToMatrix(int modelId)
        {
            var sv = new ModelAnalysisSv();
            var data = sv.GetMatrixByModel(modelId);

            if (data == null || data.Count == 0) return;

            var modelName = data.First().ModelName;
            var modelCode = data.First().ModelCode;
            // ❌ tránh trùng
            if (MatrixData.Rows.Any(x => x.ModelName == modelName))
                return;

            // ===== update vendor =====
            var newVendors = data
                .Select(x => x.VendorName)
                .Where(x => !string.IsNullOrEmpty(x)) // 🔥 tránh lỗi null
                .Distinct();

            foreach (var v in newVendors)
            {
                if (!MatrixData.Vendors.Contains(v))
                    MatrixData.Vendors.Add(v);
            }

            // ===== build row =====
            var row = new ModelVendorMatrixRowDto
            {
                ModelName = modelName,
                ModelCode = modelCode
            };

            foreach (var vendor in MatrixData.Vendors)
            {
                var latest = data
                    .Where(x => x.VendorName == vendor)
                    .OrderByDescending(x => x.PurchaseDate)
                    .FirstOrDefault();

                row.VendorPrices[vendor] = latest?.UnitPrice;
            }

            MatrixData.Rows.Add(row);

            UpdateTotalRow();
        }
        private void UpdateTotalRow()
        {
            // 1. Xóa tất cả các dòng TOTAL đang có để tính lại từ đầu
            MatrixData.Rows.RemoveAll(x => x.IsTotalRow || x.ModelName == "TOTAL");

            // 2. Nếu không có dữ liệu model nào thì không thêm dòng Total
            if (MatrixData.Rows.Count == 0) return;

            var totalRow = new ModelVendorMatrixRowDto
            {
                ModelName = "TOTAL",
                IsTotalRow = true
            };

            // 3. Tính tổng cho từng Vendor
            foreach (var vendor in MatrixData.Vendors)
            {
                // Chỉ tính tổng trên các dòng KHÔNG PHẢI total
                var sum = MatrixData.Rows
                    .Sum(x => (x.VendorPrices.ContainsKey(vendor) ? x.VendorPrices[vendor] : 0) ?? 0);

                totalRow.VendorPrices[vendor] = sum;
            }

            // 4. Thêm vào cuối danh sách
            MatrixData.Rows.Add(totalRow);

            // 5. QUAN TRỌNG: Phát tín hiệu để DataGrid vẽ lại cột và dòng
            OnPropertyChanged(nameof(MatrixData));
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
                var results = _searchService.SearchModel(GlobalSearchText);

                SearchSuggestions.Clear();
                foreach (var item in results)
                {
                    SearchSuggestions.Add(item);
                }

                IsSearchDropDownOpen = SearchSuggestions.Any();
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException)
            {
                // Khi đang gõ phím, nếu DB bị khóa thì tạm thời ẩn gợi ý để tránh làm phiền người dùng
                IsSearchDropDownOpen = false;
            }
        }

        public void ConfirmSelection()
        {
            if (SelectedSearchResult == null) return;

            _isInternalChange = true;
            try
            {
                if (SelectedSearchResult.Data is Model model)
                {
                    SelectedModelId = model.Id;
                    GlobalSearchText = model.ModelName;
                }
                else if (SelectedSearchResult.Data is ModelDto modelDto)
                {
                    SelectedModelId = modelDto.Id;
                    GlobalSearchText = modelDto.ModelName;
                }
                else
                {
                    SelectedModelId = SelectedSearchResult.Id;
                    GlobalSearchText = SelectedSearchResult.Text;
                }

                IsSearchDropDownOpen = false;
                LoadData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error confirming selection: {ex.Message}");
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        private void LoadData()
        {
            if (SelectedModelId == 0) return;

            try
            {
                Analysis = _service.GetModelAnalysis(SelectedModelId);
                AddModelToMatrix(SelectedSearchResult.Id);

            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Dữ liệu phân tích Model đang bị khóa bởi tiến trình khác. Vui lòng thử lại sau giây lát.", "SQLite Locked");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
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