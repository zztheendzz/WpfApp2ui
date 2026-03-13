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
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.view.analysis;
using WpfApp2.view.pages;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModelService modelService;
        public MainWindow()
        {
            InitializeComponent();
            modelService = new ModelService();

        }


        Page daModel = new DAModel();


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new newModel());
        }
    }
}