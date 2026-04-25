using System.Data;
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
using System.Xml.Xsl;
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.view.analysis;
using WpfApp2.view.login;
using WpfApp2.view.pages;
using WpfApp2.viewmodel;
using WpfApp2.viewmodel.login;
using WpfApp2.viewmodel.tableVm;
namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isMenuOpen = true;

        private void ToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            //if (isMenuOpen)
            //    SidebarColumn.Width = new GridLength(0);
            //else
            //    SidebarColumn.Width = new GridLength(280);

            //isMenuOpen = !isMenuOpen;
        }
        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainViewModel();
            DataContext = vm;

            vm.LogoutAction = () =>
            {
                var login = new Login();
                login.Show();
                this.Close();
            };
        }
    }
}