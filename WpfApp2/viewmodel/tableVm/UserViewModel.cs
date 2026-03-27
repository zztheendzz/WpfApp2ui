using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.view.dialog;


namespace WpfApp2.viewmodel.tableVm
{
    public class UserViewModel : INotifyPropertyChanged
    {
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public bool isEditCommand{get; set;}
        public ObservableCollection<UserDto> Users { get; set; }

        public UserDto userAdd = new UserDto();
        public UserViewModel()
        {
            UserService userService = new UserService();

            Users = new ObservableCollection<UserDto>(
                        userService.GetUserDTO());
            EditCommand = new RelayCommand(x => OpenEdit((UserDto)x));
            DeleteCommand = new RelayCommand(x => Delete((UserDto)x));
            AddCommand = new RelayCommand(x => OpenAdd());
            SaveCommand = new RelayCommand(x => Save(x));
        }
        // UserDto user userDto

        private UserDto _currentUser;
        public UserDto CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }
        public void Delete(UserDto user)
        {
            UserService userService = new UserService();
            userService.Delete(user.Id);
            Users.Remove(user);
        }
        public void Save(object parameter)
        {
            if (isEditCommand)
            {
                Edit();
            }
            else
            {
                Add();
            }
            if (parameter is Window window)
            {
                // Gán DialogResult = true để các hàm OpenAdd/OpenEdit biết là đã lưu thành công
                // window.DialogResult = true; 
                window.Close();
            }
        }

        public void Add()
        {
            UserService userService = new UserService();

            // Mã hóa mật khẩu trước khi ném xuống Service
            if (!string.IsNullOrEmpty(CurrentUser.Password))
            {
                CurrentUser.Password = BCrypt.Net.BCrypt.HashPassword(CurrentUser.Password);
            }

            int newId = userService.Add(CurrentUser);

            CurrentUser.Id = newId;
            Users.Add(CurrentUser); // Đưa vào Grid hiển thị
        }

        public void Edit()
        {
            MessageBox.Show("edit");
            UserService userService = new UserService();

            // Nếu mật khẩu bị sửa, cần mã hóa lại (Bạn cần xử lý logic check xem pass có đổi hay không)
            // userService.Edit của bạn nhận (UserDto user)
            userService.Edit(CurrentUser);

            // Cập nhật UI nhanh chóng bằng IndexOf
            int index = Users.IndexOf(CurrentUser);
            if (index != -1)
            {
                Users[index] = CurrentUser;
            }
        }

        public void OpenEdit(UserDto user)
        {
            isEditCommand = true;
            // Tạo bản sao để tránh việc sửa trực tiếp trên list khi chưa nhấn Save (tùy chọn)
            CurrentUser = user;

            var dialog = new UserEditAdd(this);
            dialog.ShowDialog();

        }

        public void OpenAdd()
        {
            isEditCommand = false;
            CurrentUser = new UserDto { IsActive = true };

            var dialog = new UserEditAdd(this);
            dialog.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}