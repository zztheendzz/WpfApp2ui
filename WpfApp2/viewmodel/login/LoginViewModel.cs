using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;
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
            // clear session
            SessionService.Logout();

            // trigger UI chuyển màn
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
                var user = await Task.Run(() => _userService.Login(Username, Password));


                if (user != null)
                {
                    // lưu session
                    SessionService.CurrentUser = user;

                    LoginSuccessAction?.Invoke();
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu");
                    LoginSuccessAction?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
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