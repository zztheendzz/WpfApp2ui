using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.view.dialog;

namespace WpfApp2.viewmodel
{

    class PurchaseViewModel: INotifyPropertyChanged
    {
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ObservableCollection<PurchaseDto> purchases { get; set; }

        public ICollectionView PurchasesView { get; set; }
        public PurchaseViewModel()
        {
            PurchaseService purchaseService = new PurchaseService();

            //purchases = new ObservableCollection<PurchaseDto>(purchaseService.GetPurchaseDTO());

           // PurchasesView = CollectionViewSource.GetDefaultView(purchases);
           // PurchasesView.Filter = FilterPurchase;

            EditCommand = new RelayCommand(x => Edit((PurchaseDto)x));
            DeleteCommand = new RelayCommand(x => Delete((PurchaseDto)x));
            AddCommand = new RelayCommand(x => Add());
        }

        public void LoadData()
        {
            PurchaseService purchaseService = new PurchaseService();
            purchases = new ObservableCollection<PurchaseDto>(
                purchaseService.Search(
                    SelectedModel?.Id,
                    SelectedVendor?.Id,
                    SelectedEquipment?.Id,
                    SelectedCategory?.Id,
                    DateFrom,
                    DateTo,
                    MinPrice,
                    MaxPrice
                )
            );

            OnPropertyChanged(nameof(purchases));
        }

        private bool FilterPurchase(object obj)
        {
            if (obj is not PurchaseDto item)
                return false;

            // SEARCH
            if (!string.IsNullOrWhiteSpace(SearchText))
                if (!item.ModelName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    return false;

            // MODEL
            if (SelectedModel != null)
                if (item.ModelName != SelectedModel.ModelCode)
                    return false;

            // VENDOR
            if (SelectedVendor != null)
                if (item.VendorName != SelectedVendor.VendorName)
                    return false;

            // EQUIPMENT
            if (SelectedEquipment != null)
                if (item.EquipmentName != SelectedEquipment.EquipmentName)
                    return false;

            // CATEGORY
            //if (SelectedCategory != null)
            //    if (item.Category != SelectedCategory.CategoryName)
            //        return false;

            // DATE
            //if (DateFrom != null)
            //    if (item.PurchaseDate < DateFrom)
            //        return false;

            //if (DateTo != null)
            //    if (item.PurchaseDate > DateTo)
            //        return false;

            // PRICE
            if (MinPrice != null)
                if (item.UnitPrice < MinPrice)
                    return false;

            if (MaxPrice != null)
                if (item.UnitPrice > MaxPrice)
                    return false;

            return true;
        }

        // SEARCH
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }

        // MODEL
        private ModelDto _selectedModel;
        public ModelDto SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }

        // VENDOR
        private VendorDto _selectedVendor;
        public VendorDto SelectedVendor
        {
            get => _selectedVendor;
            set
            {
                _selectedVendor = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }

        // CATEGORY
        private CategoryDto _selectedCategory;
        public CategoryDto SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }

        // DATE
        private DateTime? _dateFrom;
        public DateTime? DateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }

        private DateTime? _dateTo;
        public DateTime? DateTo
        {
            get => _dateTo;
            set
            {
                _dateTo = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }

        // PRICE
        private decimal? _minPrice;
        public decimal? MinPrice
        {
            get => _minPrice;
            set
            {
                _minPrice = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }

        private decimal? _maxPrice;
        public decimal? MaxPrice
        {
            get => _maxPrice;
            set
            {
                _maxPrice = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }


        // EQUIPMENT
        private EquipmentDto _selectedEquipment;
        public EquipmentDto SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                _selectedEquipment = value;
                OnPropertyChanged();
                PurchasesView.Refresh();
            }
        }


        public void Delete(PurchaseDto purchase)
        {
            PurchaseService purchaseService = new PurchaseService();

            purchaseService.Delete(purchase.Id);

            purchases.Remove(purchase);
        }

        public void Edit(PurchaseDto purchase)
        {
            var dialog = new edit(purchase);

            if (dialog.ShowDialog() == true)
            {
                PurchaseService purchaseService = new PurchaseService();
                purchaseService.Edit(purchase);

                OnPropertyChanged(nameof(purchases));
            }

        }
        public void Add()
        {
            var purchase = new PurchaseDto();   // object mới

            var dialog = new edit(purchase);

            if (dialog.ShowDialog() == true)
            {
                PurchaseService purchaseService = new PurchaseService();

                int newId = purchaseService.Add(purchase);

                purchase.Id = newId;

                purchases.Add(purchase);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}