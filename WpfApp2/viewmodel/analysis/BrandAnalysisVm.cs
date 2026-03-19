using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command; // chứa RelayCommand
using WpfApp2.model; // model cơ bản như BrandDto, PurchaseDto
using WpfApp2.modelDto; // model DTO khác
using WpfApp2.modelDTO; // có thể trùng hoặc mở rộng DTO
using WpfApp2.modelDTO.analysysDto; // DTO dùng để phân tích brand
using WpfApp2.Services; // chứa BrandService, SearchService
using WpfApp2.Services.analysisService; // chứa BrandAnalysisSv

namespace WpfApp2.viewmodel.analysis
{

    public class BrandAnalysisVm : INotifyPropertyChanged
    {
        
        private SearchService _searchService = new SearchService();

        private bool _isSelecting;
        public ObservableCollection<SearchResultDto> SearchSuggestions { get; set; }
            = new ObservableCollection<SearchResultDto>();

   
        private string _globalSearchText;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                _globalSearchText = value;
                OnPropertyChanged();

                if (_isSelecting) return; // 🔥 chặn loop

                UpdateSuggestions();
            }
        }

        // Hàm cập nhật gợi ý tìm kiếm dựa trên GlobalSearchText
        private void UpdateSuggestions()
        {
            SearchSuggestions.Clear(); // xóa các gợi ý cũ

            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 2)
            {

                IsSearchDropDownOpen = false;
                return;
            }

            // Lấy danh sách Brand phù hợp với từ khóa
            var results = _searchService.SearchBrand(GlobalSearchText);

            // Thêm từng item vào ObservableCollection để UI tự động cập nhật
            foreach (var item in results)
                SearchSuggestions.Add(item);


            IsSearchDropDownOpen = SearchSuggestions.Count > 0;
        }

        // Selected item khi người dùng chọn từ gợi ý
        private SearchResultDto _selectedSearchResult;
        public SearchResultDto SelectedSearchResult
        {
            get => _selectedSearchResult;
            set
            {
                if (_selectedSearchResult != value)
                {
                    _selectedSearchResult = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        _isSelecting = true;

                        if (value.Data is Brand brand)
                        {
                            SelectedBrandId = brand.Id;
                            GlobalSearchText = brand.BrandName;
                        }
                        else if (value.Data is BrandDto brandDto)
                        {
                            SelectedBrandId = brandDto.Id;
                            GlobalSearchText = brandDto.BrandName;
                        }

                        _isSelecting = false;

                        IsSearchDropDownOpen = false;
                        LoadData();
                    }
                }
            }
        }
        

        // Dropdown có đang mở hay không
        private bool _isSearchDropDownOpen;
        public bool IsSearchDropDownOpen
        {
            get => _isSearchDropDownOpen;
            set
            {
                _isSearchDropDownOpen = value;
                OnPropertyChanged(); // thông báo UI
            }
        }

        // Service dùng để phân tích dữ liệu theo Brand
        private readonly BrandAnalysisSv _service;

        // Command cho nút phân tích
        public ICommand AnalyzeCommand { get; set; }

        // Id brand được chọn
        private int _selectedBrandId;
        public int SelectedBrandId
        {
            get => _selectedBrandId;
            set
            {
                _selectedBrandId = value;
                OnPropertyChanged(); // thông báo UI
            }
        }

        // List brand dùng cho ComboBox hoặc UI khác
        public ObservableCollection<BrandDto> Brands { get; set; }
        public ObservableCollection<BrandDto> BrandsSearch { get; set; }

        // Constructor
        public BrandAnalysisVm()
        {
            _service = new BrandAnalysisSv(); // init service phân tích

            var brandService = new BrandService(); // init service Brand
            var list = brandService.GetBrandDTO(); // lấy danh sách brand từ service

            Brands = new ObservableCollection<BrandDto>(list); // danh sách hiển thị
            BrandsSearch = new ObservableCollection<BrandDto>(list); // danh sách dùng search
            AnalyzeCommand = new RelayCommand(_ => LoadData()); // gán command nút phân tích
        }

        // Property chứa dữ liệu phân tích brand
        private BrandAnalysisDto _analysis;
        public BrandAnalysisDto Analysis
        {
            get => _analysis;
            set
            {
                _analysis = value;
                OnPropertyChanged(); // thông báo UI khi dữ liệu phân tích thay đổi
            }
        }

        // Load dữ liệu phân tích dựa trên SelectedBrandId
        private void LoadData()
        {
            if (SelectedBrandId == 0) return; // nếu chưa chọn brand => thoát


            Analysis = _service.GetBrandAnalysis(SelectedBrandId); // gọi service lấy dữ liệu
            String  _globalSearchText;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}