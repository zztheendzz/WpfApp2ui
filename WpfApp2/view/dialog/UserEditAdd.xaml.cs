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
using WpfApp2.modelDTO;
using WpfApp2.viewmodel.tableVm;
namespace WpfApp2.view.dialog
{
    /// <summary>
    /// Interaction logic for UserEditAdd.xaml
    /// </summary>
    public partial class UserEditAdd : Window
    {
        public UserEditAdd(UserViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
