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
using WpfApp2.viewmodel;
using WpfApp2.viewmodel.tableVm;
namespace WpfApp2.view.pages
{
    /// <summary>
    /// Interaction logic for pageVendor.xaml
    /// </summary>
    public partial class pageVendor : Page
    {
        public pageVendor()
        {
            InitializeComponent();
            DataContext = new VendorViewModel();
        }
    }
}
