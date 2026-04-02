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
using WpfApp2.viewmodel.analysis;


namespace WpfApp2.view.analysis
{
    /// <summary>
    /// Interaction logic for VendorAnalysis.xaml
    /// </summary>
    public partial class VendorAnalysis : Page
    {
        public VendorAnalysis()
        {
            InitializeComponent();
            DataContext =new VendorAnalysisVm();
        }
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is VendorAnalysisVm vm)
            {
                // 1. Nhấn Enter để xác nhận lựa chọn
                if (e.Key == Key.Enter)
                {
                    vm.ConfirmSelection();
                    e.Handled = true;
                    return;
                }

                // 2. Nhấn Down để di chuyển xuống trong ListBox
                if (e.Key == Key.Down && vm.IsSearchDropDownOpen)
                {
                    if (lstVendor.SelectedIndex < lstVendor.Items.Count - 1)
                    {
                        lstVendor.SelectedIndex++;
                        lstVendor.ScrollIntoView(lstVendor.SelectedItem);
                    }
                    e.Handled = true;
                }
                // 3. Nhấn Up để di chuyển lên trong ListBox
                else if (e.Key == Key.Up && vm.IsSearchDropDownOpen)
                {
                    if (lstVendor.SelectedIndex > 0)
                    {
                        lstVendor.SelectedIndex--;
                        lstVendor.ScrollIntoView(lstVendor.SelectedItem);
                    }
                    e.Handled = true;
                }
                // 4. Nhấn Escape để đóng nhanh Popup
                else if (e.Key == Key.Escape)
                {
                    vm.IsSearchDropDownOpen = false;
                    e.Handled = true;
                }
            }
        }

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            // Khi click chuột vào item trong ListBox, gọi hàm xác nhận trong VM
            if (DataContext is VendorAnalysisVm vm)
            {
                vm.ConfirmSelection();
            }
        }
    }
}
