using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.Services.exception;
using WpfApp2.Services.sessionService;
using WpfApp2.view.dialog;

namespace WpfApp2.viewmodel.tableVm
{
    public class PurchaseViewModel : INotifyPropertyChanged
    {
        // Khởi tạo service dùng chung
        private readonly PurchaseService _purchaseService = new PurchaseService();

        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }

        private ObservableCollection<PurchaseDto> _purchases;
        public ObservableCollection<PurchaseDto> purchases
        {
            get => _purchases;
            set
            {
                _purchases = value;
                OnPropertyChanged();
                // Cập nhật lại View khi collection thay đổi
                PurchasesView = CollectionViewSource.GetDefaultView(_purchases);
                PurchasesView.Filter = FilterPurchase;
                OnPropertyChanged(nameof(PurchasesView));
            }
        }

        public ICollectionView PurchasesView { get; set; }

        public PurchaseViewModel()
        {
            // Load dữ liệu ban đầu
            LoadInitialData();

            EditCommand = new RelayCommand(x => Edit((PurchaseDto)x));
            DeleteCommand = new RelayCommand(x => Delete((PurchaseDto)x));
            AddCommand = new RelayCommand(x => Add());
        }

        private void LoadInitialData()
        {
            try
            {
                var data = _purchaseService.GetPurchaseDTO();
                purchases = new ObservableCollection<PurchaseDto>(data);
                
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Hệ thống bận, không thể tải danh sách đơn hàng.", "Thông báo");
                purchases = new ObservableCollection<PurchaseDto>();
            }
        }

        public void LoadData() // Đây là hàm Search/Filter từ Database
        {
            try
            {
                var searchResult = _purchaseService.Search(
                    SelectedModel?.Id,
                    SelectedVendor?.Id,
                    SelectedEquipment?.Id,
                    DateFrom,
                    DateTo,
                    MinPrice,
                    MaxPrice
                );
                purchases = new ObservableCollection<PurchaseDto>(searchResult);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Thao tác tìm kiếm thất bại do Database đang bị khóa.", "Lỗi");
            }
        }

        // --- Logic Filter trên View (Client-side) ---
        private bool FilterPurchase(object obj)
        {
            if (obj is not PurchaseDto item) return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
                if (item.ModelCode == null || !item.ModelCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    return false;

            return true;
        }

        // --- Properties cho Search/Filter (Rút gọn) ---
        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); PurchasesView?.Refresh(); } }

        private ModelDto _selectedModel;
        public ModelDto SelectedModel { get => _selectedModel; set { _selectedModel = value; OnPropertyChanged(); } }

        private VendorDto _selectedVendor;
        public VendorDto SelectedVendor { get => _selectedVendor; set { _selectedVendor = value; OnPropertyChanged(); } }

        private DateTime? _dateFrom;
        public DateTime? DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }

        private DateTime? _dateTo;
        public DateTime? DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }

        private decimal? _minPrice;
        public decimal? MinPrice { get => _minPrice; set { _minPrice = value; OnPropertyChanged(); } }

        private decimal? _maxPrice;
        public decimal? MaxPrice { get => _maxPrice; set { _maxPrice = value; OnPropertyChanged(); } }

        private EquipmentDto _selectedEquipment;
        public EquipmentDto SelectedEquipment { get => _selectedEquipment; set { _selectedEquipment = value; OnPropertyChanged(); } }


        // --- Thao tác Database ---
        public void Delete(PurchaseDto purchase)
        {
            if (purchase == null) return;
            if (MessageBox.Show("Xóa đơn hàng này?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                _purchaseService.Delete(purchase.Id);
                purchases.Remove(purchase);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Database đang bận, không thể xóa lúc này.", "Lỗi");
            }
        }

        public void Edit(PurchaseDto purchase)
        {
            if (purchase == null) return;
            var dialog = new edit(purchase);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _purchaseService.Edit(purchase);
                    LoadInitialData(); // Load lại để đảm bảo các thông tin Join (ModelName, VendorName) chính xác
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Không thể lưu vì Database bị khóa.", "Lỗi");
                }
            }
        }

        public void Add()
        {
            var purchase = new PurchaseDto
            {
                UserName = SessionService.CurrentUser.UserName,
                UserId = SessionService.CurrentUser.Id
            };

            var dialog = new edit(purchase);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    int newId = _purchaseService.Add(purchase);
                    var newItem = _purchaseService.GetPurchaseDTO().FirstOrDefault(x => x.Id == newId);
                    if (newItem != null) purchases.Add(newItem);
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Thêm đơn hàng thất bại do xung đột truy cập database.", "Lỗi");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}