using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;

namespace WpfApp2.viewmodel.analysis
{
    class VendorAnalysisVm : INotifyPropertyChanged
    {

        private SearchService _searchService = new SearchService();


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
            var results = _searchService.SearchVendor(GlobalSearchText);

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
                    OnPropertyChanged(); // thông báo UI
                    if (value != null)
                    {

                        // Nếu Data là BrandDto, lấy Id và tên hiển thị
                        if (value.Data is Vendor vendor)
                        {
                            SelectedVendorId = vendor.Id;
                            GlobalSearchText = vendor.VendorName;

                        }
                        else if (value.Data is VendorDto vendorDto)
                        {
                            SelectedVendorId = vendorDto.Id;
                            GlobalSearchText = vendorDto.VendorName;

                        }

                        // Đóng dropdown khi chọn xong
                        IsSearchDropDownOpen = false;

                        // Load dữ liệu phân tích cho brand đã chọn
                        LoadData();
                        OnPropertyChanged();
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

        // Service dùng để phân tích dữ liệu theo 
        private readonly VendorAnalysisSv _service;

        // Command cho nút phân tích
        public ICommand AnalyzeCommand { get; set; }

        // Id brand được chọn
        private int _selectedVendorId;
        public int SelectedVendorId
        {
            get => _selectedVendorId;
            set
            {
                _selectedVendorId = value;
                OnPropertyChanged(); // thông báo UI
            }
        }

        // List brand dùng cho ComboBox hoặc UI khác
        public ObservableCollection<VendorDto> Vendors { get; set; }
        public ObservableCollection<VendorDto> VendorSearch { get; set; }

        // Constructor
        public VendorAnalysisVm()
        {
            _service = new VendorAnalysisSv(); // init service phân tích

            var vendorService = new VendorService(); // init service Brand
            var list = vendorService.GetVendorDTO(); // lấy danh sách brand từ service

            Vendors = new ObservableCollection<VendorDto>(list); // danh sách hiển thị
            VendorSearch = new ObservableCollection<VendorDto>(list); // danh sách dùng search
            AnalyzeCommand = new RelayCommand(_ => LoadData()); // gán command nút phân tích
        }

        // Property chứa dữ liệu phân tích brand
        private VendorAnalysisDto _analysis;
        public VendorAnalysisDto Analysis
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
            if (SelectedVendorId == 0) return; // nếu chưa chọn brand => thoát


            Analysis = _service.GetVendorAnalysis(SelectedVendorId);

            // gọi service lấy dữ liệu
            String search = _globalSearchText;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}