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
namespace WpfApp2.view.pages
{
    /// <summary>
    /// Interaction logic for pageBrand.xaml
    /// </summary>
    public partial class pageBrand : Page
    {
        public pageBrand()
        {
            InitializeComponent();
            DataContext = new BrandViewModel();
        }
    }
}
