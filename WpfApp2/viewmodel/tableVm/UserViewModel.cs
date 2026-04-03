using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.modelDTO; // Đảm bảo đúng namespace
using WpfApp2.Services;
using WpfApp2.Services.exception;
using WpfApp2.view.dialog;

namespace WpfApp2.viewmodel.tableVm
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService = new UserService();
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
            LoadData();

            EditCommand = new RelayCommand(x => OpenEdit((UserDto)x));
            DeleteCommand = new RelayCommand(x => Delete((UserDto)x));
            AddCommand = new RelayCommand(x => OpenAdd());
            SaveCommand = new RelayCommand(x => Save(x));
        }

        private void LoadData()
        {
            try
            {
                Users = new ObservableCollection<UserDto>(_userService.GetUserDTO());
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Hệ thống bận, không thể tải danh sách người dùng.", "Thông báo");
                Users = new ObservableCollection<UserDto>();
            }
        }

        private bool IsDuplicateUserName(string userName, int currentUserId)
        {
            return Users.Any(u =>
                u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                && u.Id != currentUserId);
        }

        public void OpenAdd()
        {
            IsEditMode = false;
            CurrentUser = new UserDto { IsActive = true, Role = 0, UserName = "" };
            var dialog = new UserEditAdd(this);
            dialog.ShowDialog();
        }

        public void OpenEdit(UserDto user)
        {
            if (user == null) return;
            IsEditMode = true;
            CurrentUser = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Role = user.Role,
                IsActive = user.IsActive,
                Password = ""
            };
            var dialog = new UserEditAdd(this);
            dialog.ShowDialog();
        }

        public void Save(object parameter)
        {
            try
            {
                bool success = IsEditMode ? ExecuteEdit() : ExecuteAdd();

                if (success && parameter is Window window)
                {
                    window.Close();
                }
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Cơ sở dữ liệu đang bị khóa bởi một tác vụ khác. Vui lòng thử lại sau giây lát.", "Lỗi SQLite");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định: {ex.Message}");
            }
        }

        private bool ExecuteAdd()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.UserName))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!");
                return false;
            }

            if (IsDuplicateUserName(CurrentUser.UserName, 0))
            {
                MessageBox.Show("Tên đăng nhập này đã tồn tại!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu cho người dùng mới!");
                return false;
            }

            // Thực hiện Hash và Lưu (Phần này có thể bị ném DatabaseLockedException)
            string rawPassword = CurrentUser.Password;
            CurrentUser.Password = BCrypt.Net.BCrypt.HashPassword(rawPassword);

            int newId = _userService.Add(CurrentUser);
            CurrentUser.Id = newId;

            Users.Add(CurrentUser);
            return true;
        }

        private bool ExecuteEdit()
        {
            if (IsDuplicateUserName(CurrentUser.UserName, CurrentUser.Id))
            {
                MessageBox.Show("Tên đăng nhập đã bị người khác sử dụng!");
                return false;
            }

            // Chỉ hash nếu có nhập pass mới
            if (!string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                CurrentUser.Password = BCrypt.Net.BCrypt.HashPassword(CurrentUser.Password);
            }

            // Gọi service (Có thể bị ném DatabaseLockedException)
            _userService.Edit(CurrentUser);

            // Cập nhật UI
            var userInList = Users.FirstOrDefault(u => u.Id == CurrentUser.Id);
            if (userInList != null)
            {
                int index = Users.IndexOf(userInList);
                Users[index] = CurrentUser;
            }

            return true;
        }

        public void Delete(UserDto user)
        {
            if (user == null) return;
            var result = MessageBox.Show($"Xóa user {user.UserName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userService.Delete(user.Id);
                    Users.Remove(user);
                }
                catch (DatabaseLockedException)
                {
                    MessageBox.Show("Hệ thống đang bận xử lý dữ liệu, chưa thể xóa user này.", "Thông báo");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}