using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.view.dialog;

namespace WpfApp2.viewmodel.tableVm
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private UserDto _currentUser;

        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public bool IsEditMode { get; set; }
        public ObservableCollection<UserDto> Users { get; set; }

        public UserDto CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public UserViewModel()
        {
            _userService = new UserService();

            // Load danh sách ban đầu từ DB
            Users = new ObservableCollection<UserDto>(_userService.GetUserDTO());

            // Init Commands
            EditCommand = new RelayCommand(x => OpenEdit((UserDto)x));
            DeleteCommand = new RelayCommand(x => Delete((UserDto)x));
            AddCommand = new RelayCommand(x => OpenAdd());
            SaveCommand = new RelayCommand(x => Save(x));
        }

        public void OpenAdd()
        {
            IsEditMode = false;
            // Khởi tạo user mới khớp với DB (mặc định Role = 0, Active = 1)
            CurrentUser = new UserDto
            {
                IsActive = true,
                Role = 0,
            };

            var dialog = new UserEditAdd(this);
            dialog.ShowDialog();
        }

        public void OpenEdit(UserDto user)
        {
            if (user == null) return;
            IsEditMode = true;

            // Clone dữ liệu để tránh sửa trực tiếp vào Grid
            // Giữ lại Password cũ (không hiển thị) để xử lý logic update
            CurrentUser = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Role = user.Role,
                IsActive = user.IsActive,
                Password = "" // Để trống để kiểm tra xem có nhập pass mới không
            };

            var dialog = new UserEditAdd(this);
            dialog.ShowDialog();
        }

        public void Save(object parameter)
        {
            try
            {
                if (IsEditMode)
                {
                    ExecuteEdit();
                }
                else
                {
                    ExecuteAdd();
                }

                if (parameter is Window window)
                {
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void ExecuteAdd()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu cho người dùng mới!");
                return;
            }

            // Hash mật khẩu
            CurrentUser.Password = BCrypt.Net.BCrypt.HashPassword(CurrentUser.Password);

            int newId = _userService.Add(CurrentUser);
            CurrentUser.Id = newId;

            Users.Add(CurrentUser);
        }

        private void ExecuteEdit()
        {
            // Logic: Nếu Password để trống -> Không đổi pass. Nếu có chữ -> Hash pass mới.
            if (!string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                CurrentUser.Password = BCrypt.Net.BCrypt.HashPassword(CurrentUser.Password);
                MessageBox.Show(CurrentUser.Password);
            }

            _userService.Edit(CurrentUser);

            // Cập nhật lại UI Table
            var userInList = Users.FirstOrDefault(u => u.Id == CurrentUser.Id);
            if (userInList != null)
            {
                userInList.UserName = CurrentUser.UserName;
                userInList.Role = CurrentUser.Role;
                userInList.IsActive = CurrentUser.IsActive;
                // Không gán ngược Password đã hash vào UI List để đảm bảo an toàn
            }
        }

        public void Delete(UserDto user)
        {
            if (user == null) return;
            var result = MessageBox.Show($"Xóa user {user.UserName}?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _userService.Delete(user.Id);
                Users.Remove(user);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}