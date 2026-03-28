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

            // Khởi tạo Commands
            EditCommand = new RelayCommand(x => OpenEdit((UserDto)x));
            DeleteCommand = new RelayCommand(x => Delete((UserDto)x));
            AddCommand = new RelayCommand(x => OpenAdd());
            SaveCommand = new RelayCommand(x => Save(x));
        }

        private bool IsDuplicateUserName(string userName, int currentUserId)
        {
            // Kiểm tra trùng tên, loại trừ ID của người đang được sửa
            return Users.Any(u =>
                u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                && u.Id != currentUserId);
        }

        public void OpenAdd()
        {
            IsEditMode = false;
            CurrentUser = new UserDto
            {
                IsActive = true,
                Role = 0,
                UserName = ""
            };

            var dialog = new UserEditAdd(this);
            dialog.ShowDialog();
        }

        public void OpenEdit(UserDto user)
        {
            if (user == null) return;
            IsEditMode = true;

            // Clone dữ liệu để tránh sửa trực tiếp vào dòng đang hiển thị trên Grid khi chưa nhấn Save
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
                bool success = false;

                if (IsEditMode)
                {
                    success = ExecuteEdit();
                }
                else
                {
                    success = ExecuteAdd();
                }

                // Nếu thực hiện thành công (không trùng tên, đủ pass) thì mới đóng cửa sổ
                if (success && parameter is Window window)
                {
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private bool ExecuteAdd()
        {
            // 1. Kiểm tra đầu vào
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

            // 2. Hash mật khẩu và lưu DB
            CurrentUser.Password = BCrypt.Net.BCrypt.HashPassword(CurrentUser.Password);
            int newId = _userService.Add(CurrentUser);
            CurrentUser.Id = newId;

            // 3. Cập nhật UI: Thêm vào danh sách đang hiển thị
            Users.Add(CurrentUser);
            return true;
        }

        private bool ExecuteEdit()
        {
            // 1. Kiểm tra trùng (Loại trừ chính mình qua ID)
            if (IsDuplicateUserName(CurrentUser.UserName, CurrentUser.Id))
            {
                MessageBox.Show("Tên đăng nhập đã bị người khác sử dụng!");
                return false;
            }

            // 2. Xử lý Password (chỉ hash nếu người dùng có gõ mật khẩu mới)
            if (!string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                CurrentUser.Password = BCrypt.Net.BCrypt.HashPassword(CurrentUser.Password);
            }

            // 3. Gọi Service cập nhật DB
            _userService.Edit(CurrentUser);

            // 4. Cập nhật UI: Tìm vị trí cũ và thay thế bằng Object mới để Grid refresh ngay lập tức
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