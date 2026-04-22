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
using System.Windows.Shapes;
using WpfApp2.viewmodel.login;

namespace WpfApp2.view.login
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();

            var vm = new LoginViewModel();
            DataContext = vm;

            vm.LoginSuccessAction = () =>
            {
                var main = new MainWindow();
                main.Show();
                this.Close();
            };

        }

    }
}
