using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    public class ModelViewModel : INotifyPropertyChanged
    {
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

        public ModelViewModel()
        {
            Models = new ObservableCollection<ModelDto>();
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
                Models = new ObservableCollection<ModelDto>();
            }
        }

        private void ApplySearch()
        {
            try
            {
                var data = _modelService.GetModelDTO();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    data = data
                        .Where(m => m.ModelCode != null &&
                                    m.ModelCode.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                Models = new ObservableCollection<ModelDto>(data);
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Không thể tải dữ liệu do DB bị khóa.", "Thông báo");
            }
        }

        public void Delete(ModelDto model)
        {
            if (model == null) return;

            var confirm = MessageBox.Show("Bạn có chắc chắn muốn xóa Model này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
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

            // 🔥 clone dữ liệu
            var temp = new ModelDto
            {
                Id = model.Id,
                ModelName = model.ModelName,
                ModelCode = model.ModelCode,
                BrandName = model.BrandName
            };

            var dialog = new edit(temp);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    MessageBox.Show(temp.Image ?? "NULL");
                    MessageBox.Show(File.Exists(temp.Image).ToString());
                    if (!string.IsNullOrEmpty(temp.Image) && File.Exists(temp.Image))
                    {
                        string folder = @"Z:\Nguyen Lam Long Trong\Image";

                        // tạo folder nếu chưa có
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        // lấy đuôi file (.png, .jpg...)
                        string ext = Path.GetExtension(temp.Image);

                        // tên mới = ModelCode + extension
                        string newFileName = temp.ModelCode + ext;

                        string destPath = Path.Combine(folder, newFileName);

                        // copy + overwrite nếu tồn tại
                        File.Copy(temp.Image, destPath, true);

                        // 🔥 lưu lại path mới (quan trọng)
                        temp.Image = destPath;

                        MessageBox.Show("destPath = " + destPath);

                    }
                    // 🔥 copy lại khi OK
                    model.ModelName = temp.ModelName;
                    model.ModelCode = temp.ModelCode;
                    model.BrandName = temp.BrandName;
                    model.Image = temp.Image;

                    MessageBox.Show("vm = " + model.Image);

                    _modelService.Edit(model);

                    // 🔥 refresh UI
                    LoadData();
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Không thể lưu thay đổi cho Model vì Database bị khóa.", "Lỗi");
                    LoadData();
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
