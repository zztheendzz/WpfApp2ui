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
using WpfApp2.view.pages;
using WpfApp2.view.analysis;
namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        Page category = new Category();
        Page puchaseHistory = new PuchaseHistory();
        Page vendor = new Vendor();
        Page Currency = new Currency();
        Page brand = new Brand();
        Page daModel = new DAModel();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dataTable.Navigate(new Category());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            dataTable.Navigate(puchaseHistory);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            dataTable.Navigate(vendor);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DA.Navigate(daModel);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            dataTable.Navigate(brand);
        }
    }
}