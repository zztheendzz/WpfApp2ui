using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDTO;

using WpfApp2.Services;
using WpfApp2.Services.analysisService;

namespace WpfApp2.viewmodel.analysis
{
    public class PurchaseAnalysisVm : INotifyPropertyChanged
    {


        public ICommand SearchCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand VendorFocusCommand { get; set; }

        private readonly SearchService _searchService = new SearchService();

        #region ===================== Suggestions =====================

        public ObservableCollection<SearchResultDto> ModelSuggestions { get; set; }
            = new ObservableCollection<SearchResultDto>();

        public ObservableCollection<SearchResultDto> VendorSuggestions { get; set; }
            = new ObservableCollection<SearchResultDto>();

        public ObservableCollection<SearchResultDto> EquipmentSuggestions { get; set; }
            = new ObservableCollection<SearchResultDto>();

        #endregion

        #region ===================== Search Text =====================


        private bool _isSelecting;
        private string _searchModelText;
        public string SearchModelText
        {
            get => _searchModelText;
            set
            {
                _searchModelText = value;
                OnPropertyChanged();
                if (_isSelecting) return; // ✅ FIX QUAN TRỌNG
                UpdateModelSuggestions(value);
            }
        }

        private string _searchVendorText;
        public string SearchVendorText
        {
            get => _searchVendorText;
            set
            {
                _searchVendorText = value;
                OnPropertyChanged();
                UpdateVendorSuggestions(value);
            }
        }

        private string _searchEquipmentText;
        public string SearchEquipmentText
        {
            get => _searchEquipmentText;
            set
            {
                _searchEquipmentText = value;
                OnPropertyChanged();
                UpdateEquipmentSuggestions(value);
            }
        }

        #endregion





        #region ===================== Selected Item =====================

        private SearchResultDto _selectedModel;
        public SearchResultDto SelectedModel
        {
            get => _selectedModel;
            set
            {
                _isSelecting = true;

                _selectedModel = value;
                OnPropertyChanged();

                if (value != null)
                {
                    SelectedModelId = value.Id;
                    SearchModelText = value.Text;
                }
                MessageBox.Show("_selectedModel  " + _selectedModel);
                _isSelecting = false;
            }
        }

        private SearchResultDto _selectedVendor;
        public SearchResultDto SelectedVendor
        {
            get => _selectedVendor;
            set
            {
                _selectedVendor = value;
                OnPropertyChanged();

                if (value != null)
                {
                    SelectedVendorId = value.Id;
                    SearchVendorText = value.Text;
                }

            }
        }

        private SearchResultDto _selectedEquipment;
        public SearchResultDto SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                _selectedEquipment = value;
                OnPropertyChanged();

                if (value != null)
                {
                    SelectedEquipmentId = value.Id;
                    SearchEquipmentText = value.Text;
                }
            }
        }

        #endregion

        #region ===================== Selected Id =====================

        private int _selectedModelId;
        public int SelectedModelId
        {
            get => _selectedModelId;
            set
            {
                _selectedModelId = value;
               // LoadData();
                OnPropertyChanged();
            }
        }

        private int _selectedVendorId;
        public int SelectedVendorId
        {
            get => _selectedVendorId;
            set
            {
                _selectedVendorId = value;
               // LoadData();
                OnPropertyChanged();
            }
        }

        private int _selectedEquipmentId;
        public int SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set
            {
                _selectedEquipmentId = value;
                LoadData();
                OnPropertyChanged();
            }
        }

        #endregion

        private DateTime? _dateFrom;
        public DateTime? SelectedDateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                OnPropertyChanged();
                MessageBox.Show("from" + _dateFrom);
               // LoadData();
            }
        }
        private DateTime? _dateTo;
        public DateTime? SelectedDateTo
        {
            get => _dateTo;
            set
            {
                _dateTo = value;
                OnPropertyChanged();
               // LoadData();
            }
        }


        private void ValidateDate()
        {
            if (SelectedDateFrom.HasValue && SelectedDateTo.HasValue)
            {
                if (SelectedDateFrom > SelectedDateTo)
                {
                    Error = "From phải ≤ To";
                    return;
                }
            }

            Error = null;
        }

        private int? _count;
        public int? SelectedCount
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }

        #region ===================== Update Suggestions =====================

        private void UpdateModelSuggestions(string searchText)
        {
            if (_isSelecting) return;
            ModelSuggestions.Clear();

            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
                return;

            var results = _searchService.SearchModel(searchText);

            foreach (var item in results)
                ModelSuggestions.Add(item);
        }

        private void UpdateVendorSuggestions(string searchText)
        {
            VendorSuggestions.Clear();

            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
                return;

            var results = _searchService.SearchVendor(searchText);

            foreach (var item in results)
                VendorSuggestions.Add(item);
        }

        private void UpdateEquipmentSuggestions(string searchText)
        {
            EquipmentSuggestions.Clear();

            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
                return;

            var results = _searchService.SearchEquipment(searchText);

            foreach (var item in results)
                EquipmentSuggestions.Add(item);
        }

        #endregion

        public PurchaseAnalysisVm()
        {
            SearchCommand = new RelayCommand( Search);
            ClearCommand = new RelayCommand(Clear);
            PurchaseDtos = new ObservableCollection<PurchaseDto>();

        }

        public void Search(Object obj)
        {
            LoadData();
        }

        public void Clear(Object obj)
        {
            SearchModelText = string.Empty;
            SearchVendorText = string.Empty;
            SearchEquipmentText = string.Empty;

            // ===== Reset selected item =====
            SelectedModel = null;
            SelectedVendor = null;
            SelectedEquipment = null;

            // ===== Reset Id =====
            SelectedModelId = 0;
            SelectedVendorId = 0;
            SelectedEquipmentId = 0;

            // ===== Reset price =====
            PriceMin = null;
            PriceMax = null;

            // ===== Clear error =====
            Error = null;

            // ===== Clear suggestions =====
            ModelSuggestions.Clear();
            VendorSuggestions.Clear();
            EquipmentSuggestions.Clear();

            // ===== Reload data (load all) =====
            LoadData();
        }


        public ObservableCollection<PurchaseDto> PurchaseDtos { get; set; }
        private void LoadData()
        {
            PurchaseDtos.Clear();
            PurchaseAnalysisSv purchaseService = new PurchaseAnalysisSv();
            var list = purchaseService.Search3(
                SelectedEquipmentId == 0 ? null : SelectedEquipmentId,
                SelectedModelId == 0 ? null : SelectedModelId,
                SelectedVendorId == 0 ? null : SelectedVendorId,
                _priceMin==0 ? null : PriceMin,
                _priceMax==0 ? null : PriceMax,
                SelectedDateFrom.HasValue ?  SelectedDateFrom: null ,
                SelectedDateTo.HasValue? SelectedDateTo:null

            ).ToList();
            foreach (var item in list)
            { PurchaseDtos.Add(item);}
            SelectedCount = list.Count;
        }



        private decimal? _priceMin;
        public decimal? PriceMin
        {
            get => _priceMin;
            set
            {
                if (_priceMin == value) return;

                // ❗ chỉ cho số dương
                if (value < 0) value = 0;

                _priceMin = value;
                OnPropertyChanged();

                ValidateRange();
               // Filter(); // gọi search
            }
        }


        private decimal? _priceMax;
        public decimal? PriceMax
        {
            get => _priceMax;
            set
            {

                // ❗ chỉ cho số dương
                if (value < 0) value = 0;

                _priceMax = value;
                OnPropertyChanged();

                ValidateRange();
               // LoadData();
               // Filter(); // gọi search
            }
        }

        private string _error;
        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                OnPropertyChanged();
                ValidateRange();
                //LoadData() ;
            }
        }

        // ✅ check Min <= Max
        private void ValidateRange()
        {
            if (PriceMin.HasValue && PriceMax.HasValue)
            {
                if (PriceMin > PriceMax)
                {
                    Error = "Min phải ≤ Max";
                    return;
                }
            }

            Error = null;
        }

        // 🔍 gọi search/filter
        private void Filter()
        {
            if (!string.IsNullOrEmpty(Error)) return;

            LoadData();
        }

        #region ===================== INotify =====================

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}