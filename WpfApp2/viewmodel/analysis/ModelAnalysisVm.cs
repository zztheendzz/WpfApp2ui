using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;

namespace WpfApp2.viewmodel.analysis
{
    public class ModelAnalysisVm : INotifyPropertyChanged
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
            var results = _searchService.SearchModel(GlobalSearchText);

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
                        if (value.Data is Model model)
                        {
                            SelectedModelId = model.Id;
                            GlobalSearchText = model.ModelName;

                        }
                        else if (value.Data is ModelDto modelDto)
                        {
                            SelectedModelId = modelDto.Id;
                            GlobalSearchText = modelDto.ModelName;

                        }

                        // Đóng dropdown khi chọn xong
                        IsSearchDropDownOpen = false;

                        // Load dữ liệu phân tích cho brand đã chọn
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
        private readonly ModelAnalysisSv _service;

        // Command cho nút phân tích
        public ICommand AnalyzeCommand { get; set; }

        // Id brand được chọn
        private int _selectedModelId;
        public int SelectedModelId
        {
            get => _selectedModelId;
            set
            {
                _selectedModelId = value;
                OnPropertyChanged(); // thông báo UI
            }
        }

        // List brand dùng cho ComboBox hoặc UI khác
        public ObservableCollection<ModelDto> Models { get; set; }
        public ObservableCollection<ModelDto> ModelsSearch { get; set; }

        // Constructor
        public ModelAnalysisVm()
        {
            _service = new ModelAnalysisSv(); // init service phân tích

            var modelService = new ModelService(); // init service Brand
            var list = modelService.GetModelDTO(); // lấy danh sách brand từ service

            Models = new ObservableCollection<ModelDto>(list); // danh sách hiển thị
            ModelsSearch = new ObservableCollection<ModelDto>(list); // danh sách dùng search
            AnalyzeCommand = new RelayCommand(_ => LoadData()); // gán command nút phân tích
        }

        // Property chứa dữ liệu phân tích brand
        private ModelAnalysisDto _analysis;
        public ModelAnalysisDto Analysis
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
            if (SelectedModelId == 0) return; // nếu chưa chọn brand => thoát


            Analysis = _service.GetModelAnalysis(SelectedModelId); // gọi service lấy dữ liệu
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}