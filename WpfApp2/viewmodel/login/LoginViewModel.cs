using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.Services.exception;
using WpfApp2.Services.sessionService;

namespace WpfApp2.viewmodel.login
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService = new UserService();
        public Action LoginSuccessAction { get; set; }
        public Action LogoutAction { get; set; }

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async p => await LoginAsync());
            LogoutCommand = new RelayCommand(p => Logout());
        }

        private void Logout()
        {
            // Clear session
            SessionService.Logout();

            // Trigger UI chuyển màn
            LogoutAction?.Invoke();
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;

                // Kiểm tra DB ở luồng phụ để UI mượt 
                // Sử dụng Task.Run để tránh làm treo UI Thread khi SQLite đang cố gắng kết nối
                var user = await Task.Run(() => _userService.Login(Username, Password));

                if (user != null)
                {
                    // Lưu session
                    SessionService.CurrentUser = user;
                    LoginSuccessAction?.Invoke();
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu.", "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (DatabaseLockedException)
            {
                // Xử lý riêng lỗi SQLite bị khóa
                MessageBox.Show("Hệ thống hiện đang bận xử lý dữ liệu (Database Locked). Vui lòng đợi vài giây và thử đăng nhập lại.",
                                "Thông báo hệ thống", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}