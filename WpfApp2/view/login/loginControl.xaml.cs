using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2.viewmodel.login;

namespace WpfApp2.view.login
{
    /// <summary>
    /// Interaction logic for loginControl.xaml
    /// </summary>
    public partial class loginControl : UserControl
    {
        public loginControl()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
            var vm = new LoginViewModel();
            DataContext = vm;

            vm.LoginSuccessAction = () =>
            {
                var main = new MainWindow();
                main.Show();

                Window.GetWindow(this)?.Close();
            };
            vm.LogoutAction = () =>
            {
                var login = new loginControl(); // hoặc LoginControl host
                //login.Show();

                //this.Close();
            };
        }
    }
}
