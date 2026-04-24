using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
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
using WpfApp2.Services.exportExcel;

namespace WpfApp2.viewmodel.analysis
{
    public class ModelAnalysisVm : INotifyPropertyChanged
    {
        // Khai báo Service ở cấp Class để dùng chung (tránh tạo mới liên tục tốn tài nguyên)
        private readonly SearchService _searchService = new SearchService();
        private readonly ModelAnalysisSv _service = new ModelAnalysisSv();
        private readonly ExportSv _exportService = new ExportSv();

        private bool _isInternalChange;

        #region ===================== Properties =====================

        private ModelVendorMatrixDto _matrixData = new ModelVendorMatrixDto
        {
            Vendors = new List<string>(),
            Rows = new List<ModelVendorMatrixRowDto>()
        };
        public ModelVendorMatrixDto MatrixData
        {
            get => _matrixData;
            set { _matrixData = value; OnPropertyChanged(); }
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
        private ObservableCollection<PurchaseDto> _analysisItems = new ObservableCollection<PurchaseDto>();
        public ObservableCollection<PurchaseDto> AnalysisItems
        {
            get => _analysisItems;
            set { _analysisItems = value; OnPropertyChanged(); }
        }
        public ObservableCollection<SearchResultDto> SearchSuggestions { get; } = new ObservableCollection<SearchResultDto>();
        public ObservableCollection<ModelAnalysisDto> AnalysisList { get; set; } = new ObservableCollection<ModelAnalysisDto>();

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

        private int _selectedModelId;
        public int SelectedModelId
        {
            get => _selectedModelId;
            set { _selectedModelId = value; OnPropertyChanged(); }
        }

        public ICommand AnalyzeCommand { get; }
        public ICommand ExportExcelCommand { get; }

        #endregion

        public ModelAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());
            ExportExcelCommand = new RelayCommand(_ => ExecuteExportExcel());
        }

        #region ===================== Methods =====================
        private int _lastSelectedModelId = -1;
        private void LoadData()
        {
            if (SelectedModelId == 0) return;
            if (SelectedModelId == _lastSelectedModelId)
                return;

            // ✅ Cập nhật lại cache
            _lastSelectedModelId = SelectedModelId;


            try
            {
                // 1. Thêm vào bảng so sánh Vendor (Matrix)
                AddModelToMatrix(SelectedModelId);

                // 2. Lấy dữ liệu Analysis (đối tượng chứa List bên trong)
                var analysisResult = _service.GetModelAnalysis(SelectedModelId);


                if (analysisResult != null && analysisResult.Items != null)
                {
                    AnalysisList.Add(analysisResult);
                    // TÁCH: Lấy từng item trong list con của Analysis rồi đẩy ra list riêng
                    foreach (var item in analysisResult.Items)
                    {
                        // Kiểm tra tránh trùng nếu cần, hoặc cứ add để hiện lịch sử
                        // Nếu muốn mỗi lần tìm là xóa bảng cũ thì dùng AnalysisItems.Clear() ở đầu hàm
                        AnalysisItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tách dữ liệu: {ex.Message}");
            }
        }
        public void AddModelToMatrix(int modelId)
        {
            var data = _service.GetMatrixByModel(modelId);
            if (data == null || data.Count == 0) return;
       
            var firstItem = data.First();
            if (MatrixData.Rows.Any(x => x.ModelCode == firstItem.ModelCode && !x.IsTotalRow)) return;

            // Cập nhật danh sách Vendor
            var newVendors = data.Select(x => x.VendorName).Where(v => !string.IsNullOrEmpty(v)).Distinct();
            foreach (var v in newVendors)
            {
                if (!MatrixData.Vendors.Contains(v)) MatrixData.Vendors.Add(v);
            }

            // Xây dựng dòng dữ liệu mới
            var row = new ModelVendorMatrixRowDto
            {
                ModelName = firstItem.ModelName,
                ModelCode = firstItem.ModelCode,
                Image = firstItem.Image
            };

            foreach (var vendor in MatrixData.Vendors)
            {
                row.VendorPrices[vendor] = data
                    .Where(x => x.VendorName == vendor)
                    .OrderByDescending(x => x.PurchaseDate)
                    .FirstOrDefault()?.UnitPrice;
            }

            MatrixData.Rows.Add(row);
            UpdateTotalRow();
        }

        private void UpdateTotalRow()
        {
            MatrixData.Rows.RemoveAll(x => x.IsTotalRow || x.ModelName == "TOTAL");
            if (MatrixData.Rows.Count == 0) return;

            var totalRow = new ModelVendorMatrixRowDto { ModelName = "TOTAL", IsTotalRow = true };
            foreach (var vendor in MatrixData.Vendors)
            {
                var sum = MatrixData.Rows.Sum(x => (x.VendorPrices.ContainsKey(vendor) ? x.VendorPrices[vendor] : 0) ?? 0);
                totalRow.VendorPrices[vendor] = sum;
            }
            MatrixData.Rows.Add(totalRow);
            OnPropertyChanged(nameof(MatrixData));
        }

        private void ExecuteExportExcel()
        {
            if (MatrixData.Rows.Count <= 1) // Chỉ có mỗi dòng TOTAL hoặc trống
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"BOM_Analysis_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    _exportService.ExportModelMatrix(MatrixData, sfd.FileName);
                    MessageBox.Show("Xuất file Excel thành công!", "Thành công");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi");
                }
            }
        }

        // Các hàm UpdateSuggestions, ConfirmSelection giữ nguyên logic search của bạn...
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
                foreach (var item in results) SearchSuggestions.Add(item);
                IsSearchDropDownOpen = SearchSuggestions.Any();
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException) { IsSearchDropDownOpen = false; }
        }

        public void ConfirmSelection()
        {
            if (SelectedSearchResult == null) return;
            _isInternalChange = true;
            try
            {
                SelectedModelId = SelectedSearchResult.Id;
                GlobalSearchText = SelectedSearchResult.Text;
                IsSearchDropDownOpen = false;
                LoadData();
            }
            finally { _isInternalChange = false; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}