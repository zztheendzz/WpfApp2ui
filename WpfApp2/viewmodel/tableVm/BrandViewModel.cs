using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDto;
using WpfApp2.Services;
using WpfApp2.Services.exception;
using WpfApp2.view.dialog;


namespace WpfApp2.viewmodel.tableVm
{
    public class BrandViewModel : INotifyPropertyChanged
    {
        private readonly BrandService _brandService = new BrandService(); // Khởi tạo một lần để dùng chung
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }

        private ObservableCollection<BrandDto> _brands;
        public ObservableCollection<BrandDto> Brands
        {
            get => _brands;
            set { _brands = value; OnPropertyChanged(); }
        }

        public BrandViewModel()
        {
            // Khởi tạo collection trống trước
            Brands = new ObservableCollection<BrandDto>();

            // Gọi hàm load dữ liệu
            LoadData();

            EditCommand = new RelayCommand(x => Edit((BrandDto)x));
            DeleteCommand = new RelayCommand(x => Delete((BrandDto)x));
            AddCommand = new RelayCommand(x => Add());
        }

        private void LoadData()
        {
            try
            {
                var data = _brandService.GetBrandDTO();
                Brands = new ObservableCollection<BrandDto>(data);
            }
            catch (DatabaseLockedException)
            {
                System.Windows.MessageBox.Show("Dữ liệu đang bận xử lý, vui lòng mở lại cửa sổ sau giây lát.", "Thông báo");
            }
        }

        public void Delete(BrandDto brand)
        {
            if (brand == null) return;

            try
            {
                _brandService.Delete(brand.Id);
                Brands.Remove(brand);
            }
            catch (DatabaseLockedException)
            {
                System.Windows.MessageBox.Show("Không thể xóa vì cơ sở dữ liệu đang bị khóa. Hãy thử lại sau.", "Lỗi");
            }
        }

        public void Edit(BrandDto brand)
        {
            var dialog = new edit(brand);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _brandService.Edit(brand);
                    // OnPropertyChanged(nameof(Brands)); // Thường không cần vì BrandDto đã được update ref
                }
                catch (DatabaseLockedException)
                {
                    System.Windows.MessageBox.Show("Lưu thay đổi thất bại do DB bị khóa.", "Lỗi");
                    LoadData(); // Load lại để tránh sai lệch dữ liệu trên giao diện
                }
            }
        }

        public void Add()
        {
            var brand = new BrandDto { IsActive = true };
            var dialog = new edit(brand);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    int newId = _brandService.Add(brand);
                    brand.Id = newId;
                    Brands.Add(brand);
                }
                catch (DatabaseLockedException)
                {
                    System.Windows.MessageBox.Show("Không thể thêm mới lúc này. Vui lòng thử lại.", "Lỗi");
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