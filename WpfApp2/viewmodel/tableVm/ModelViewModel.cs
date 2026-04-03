using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; // Cần thiết để dùng FirstOrDefault
using System.Runtime.CompilerServices;
using System.Windows; // Để dùng MessageBox
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.Services.exception;
using WpfApp2.view.dialog;

namespace WpfApp2.viewmodel.tableVm
{
    public class ModelViewModel : INotifyPropertyChanged
    {
        // Khai báo Service dùng chung để tránh khởi tạo nhiều lần
        private readonly ModelService _modelService = new ModelService();

        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }

        private ObservableCollection<ModelDto> _models;
        public ObservableCollection<ModelDto> Models
        {
            get => _models;
            set { _models = value; OnPropertyChanged(); }
        }

        public ModelViewModel()
        {
            LoadData();

            EditCommand = new RelayCommand(x => Edit((ModelDto)x));
            DeleteCommand = new RelayCommand(x => Delete((ModelDto)x));
            AddCommand = new RelayCommand(x => Add());
        }

        private void LoadData()
        {
            try
            {
                var data = _modelService.GetModelDTO();
                Models = new ObservableCollection<ModelDto>(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Dữ liệu Models hiện đang bị khóa bởi tiến trình khác. Vui lòng thử lại sau.", "Thông báo");
                Models = new ObservableCollection<ModelDto>(); // Tránh lỗi null cho UI
            }
        }

        public void Delete(ModelDto model)
        {
            if (model == null) return;

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa Model này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _modelService.Delete(model.Id);
                    Models.Remove(model);
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Database đang bận, không thể thực hiện thao tác xóa.", "Lỗi hệ thống");
                }
            }
        }

        public void Edit(ModelDto model)
        {
            if (model == null) return;

            var dialog = new edit(model);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _modelService.Edit(model);
                    // Thông thường không cần OnPropertyChanged(nameof(Models)) ở đây 
                    // vì object model trong danh sách đã được cập nhật tham chiếu
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Không thể lưu thay đổi cho Model vì Database bị khóa.", "Lỗi");
                    LoadData(); // Reload để đồng bộ lại dữ liệu gốc từ DB
                }
            }
        }

        public void Add()
        {
            var model = new ModelDto { IsActive = true };
            var dialog = new edit(model);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    int newId = _modelService.Add(model);

                    // Lấy lại data đã JOIN hoàn chỉnh (quan trọng đối với Model thường có BrandName, v.v.)
                    var newItem = _modelService.GetModelDTO()
                                             .FirstOrDefault(x => x.Id == newId);

                    if (newItem != null)
                    {
                        Models.Add(newItem);
                    }
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Thêm mới thất bại. Hệ thống đang bận ghi dữ liệu khác.", "Thông báo");
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