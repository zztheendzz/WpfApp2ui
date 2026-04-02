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
    /// Interaction logic for BrandAnalysis.xaml
    /// </summary>
    public partial class BrandAnalysis : Page
    {
        public BrandAnalysis()
        {
            InitializeComponent();
            DataContext = new BrandAnalysisVm();
        }
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is BrandAnalysisVm vm)
            {
                // Nhấn Enter để chọn
                if (e.Key == Key.Enter)
                {
                    vm.ConfirmSelection();
                    e.Handled = true;
                }
                // Nhấn Down để chọn item phía dưới trong ListBox
                else if (e.Key == Key.Down && vm.IsSearchDropDownOpen)
                {
                    if (lstBrand.SelectedIndex < lstBrand.Items.Count - 1)
                    {
                        lstBrand.SelectedIndex++;
                        lstBrand.ScrollIntoView(lstBrand.SelectedItem);
                    }
                    e.Handled = true;
                }
                // Nhấn Up để chọn item phía trên
                else if (e.Key == Key.Up && vm.IsSearchDropDownOpen)
                {
                    if (lstBrand.SelectedIndex > 0)
                    {
                        lstBrand.SelectedIndex--;
                        lstBrand.ScrollIntoView(lstBrand.SelectedItem);
                    }
                    e.Handled = true;
                }
            }
        }

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BrandAnalysisVm vm)
            {
                vm.ConfirmSelection();
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            // Tìm TextBox bên trong ComboBox và hủy bôi đen
            var textBox = cb?.Template.FindName("PART_EditableTextBox", cb) as TextBox;
            if (textBox != null)
            {
                textBox.SelectionLength = 0; // Hủy bôi đen
                textBox.CaretIndex = textBox.Text.Length; // Đưa con trỏ về cuối
            }
        }
    }
}
