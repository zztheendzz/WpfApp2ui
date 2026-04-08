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
    public class EquipmentViewModel : INotifyPropertyChanged
    {
        private readonly EquipmentService _equipmentService = new EquipmentService();

        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }

        private ObservableCollection<EquipmentDto> _equipments;
        public ObservableCollection<EquipmentDto> Equipments
        {
            get => _equipments;
            set { _equipments = value; OnPropertyChanged(); }
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

        public EquipmentViewModel()
        {
            Equipments = new ObservableCollection<EquipmentDto>();
            LoadData();

            EditCommand = new RelayCommand(x => Edit((EquipmentDto)x));
            DeleteCommand = new RelayCommand(x => Delete((EquipmentDto)x));
            AddCommand = new RelayCommand(x => Add());
        }

        private void LoadData()
        {
            try
            {
                var data = _equipmentService.GetEquipmentDto();
                Equipments = new ObservableCollection<EquipmentDto>(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Không thể tải danh sách thiết bị do cơ sở dữ liệu đang bận.", "Thông báo");
                Equipments = new ObservableCollection<EquipmentDto>();
            }
        }

        private void ApplySearch()
        {
            try
            {
                var data = _equipmentService.GetEquipmentDto();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    data = data
                        .Where(e => e.EquipmentName != null &&
                                    e.EquipmentName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                Equipments = new ObservableCollection<EquipmentDto>(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Không thể tải dữ liệu do DB bị khóa.", "Thông báo");
            }
        }

        public void Delete(EquipmentDto equipment)
        {
            if (equipment == null) return;

            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa thiết bị này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _equipmentService.Delete(equipment.Id);
                    Equipments.Remove(equipment);
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Hệ thống đang bận xử lý tác vụ khác, chưa thể xóa ngay lúc này.", "Lỗi");
                }
            }
        }

        public void Edit(EquipmentDto equipment)
        {
            if (equipment == null) return;

            var dialog = new edit(equipment);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _equipmentService.Edit(equipment);
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Không thể lưu thay đổi. Cơ sở dữ liệu đang bị khóa.", "Lỗi");
                    LoadData();
                }
            }
        }

        public void Add()
        {
            var equipment = new EquipmentDto { IsActive = true };
            var dialog = new edit(equipment);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    int newId = _equipmentService.Add(equipment);
                    var newItem = _equipmentService.GetEquipmentDto()
                                                   .FirstOrDefault(x => x.Id == newId);

                    if (newItem != null)
                    {
                        Equipments.Add(newItem);
                    }
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Thêm thiết bị thất bại vì cơ sở dữ liệu đang bận. Vui lòng thử lại sau.", "Lỗi");
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
