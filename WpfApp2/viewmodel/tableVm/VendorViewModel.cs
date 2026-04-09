using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.Services.exception;
using WpfApp2.view.dialog;

namespace WpfApp2.viewmodel.tableVm
{
    public class VendorViewModel : INotifyPropertyChanged
    {
        private readonly VendorService _vendorService = new VendorService();

        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }

        private ObservableCollection<VendorDto> _vendors;
        public ObservableCollection<VendorDto> Vendors
        {
            get => _vendors;
            set { _vendors = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplySearch(); // lọc lại dữ liệu mỗi khi thay đổi
            }
        }

        public VendorViewModel()
        {
            Vendors = new ObservableCollection<VendorDto>();
            LoadData();

            EditCommand = new RelayCommand(x => Edit((VendorDto)x));
            DeleteCommand = new RelayCommand(x => Delete((VendorDto)x));
            AddCommand = new RelayCommand(x => Add());
        }

        private void LoadData()
        {
            try
            {
                var data = _vendorService.GetVendorDTO();
                Vendors = new ObservableCollection<VendorDto>(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Cơ sở dữ liệu đang bận, không thể tải danh sách nhà cung cấp.", "Thông báo");
                Vendors = new ObservableCollection<VendorDto>();
            }
        }

        private void ApplySearch()
        {
            try
            {
                var data = _vendorService.GetVendorDTO();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    data = data
                        .Where(v => v.VendorName != null &&
                                    v.VendorName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                Vendors = new ObservableCollection<VendorDto>(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Không thể tải dữ liệu do DB bị khóa.", "Thông báo");
            }
        }

        public void Delete(VendorDto vendor)
        {
            if (vendor == null) return;

            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa nhà cung cấp: {vendor.VendorName}?",
                                         "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _vendorService.Delete(vendor.Id);
                    Vendors.Remove(vendor);
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Hệ thống đang bận ghi dữ liệu khác, vui lòng thử lại sau.", "Lỗi");
                }
            }
        }

        public void Edit(VendorDto vendor)
        {
            if (vendor == null) return;

            // 🔥 clone
            var temp = new VendorDto
            {
                Id = vendor.Id,
                VendorName = vendor.VendorName
            };

            var dialog = new edit(temp);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 🔥 copy lại khi OK
                    vendor.VendorName = temp.VendorName;

                    _vendorService.Edit(vendor);

                    // 🔥 reload UI
                    LoadData();
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Lỗi: Cơ sở dữ liệu bị khóa, không thể lưu thay đổi.", "Thông báo");
                    LoadData();
                }
            }
        }

        public void Add()
        {
            var vendor = new VendorDto { IsActive = true };
            var dialog = new edit(vendor);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    int newId = _vendorService.Add(vendor);
                    vendor.Id = newId;
                    Vendors.Add(vendor);
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Không thể thêm nhà cung cấp mới lúc này do DB đang bận.", "Lỗi");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
