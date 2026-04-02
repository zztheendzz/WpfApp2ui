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
    /// Interaction logic for EquipmentAnalysis.xaml
    /// </summary>
    public partial class EquipmentAnalysis : Page
    {
        public EquipmentAnalysis()
        {
            InitializeComponent();
            DataContext= new EquipmentAnalysisVm();
        }
        /// <summary>
        /// Xử lý điều hướng bằng phím mũi tên và xác nhận bằng phím Enter trên TextBox tìm kiếm
        /// </summary>
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is EquipmentAnalysisVm vm)
            {
                // Nhấn Enter để xác nhận lựa chọn từ danh sách gợi ý
                if (e.Key == Key.Enter)
                {
                    vm.ConfirmSelection();
                    e.Handled = true;
                }
                // Nhấn mũi tên xuống (Down) để duyệt danh sách gợi ý
                else if (e.Key == Key.Down && vm.IsSearchDropDownOpen)
                {
                    if (lstSearchResults.SelectedIndex < lstSearchResults.Items.Count - 1)
                    {
                        lstSearchResults.SelectedIndex++;
                        lstSearchResults.ScrollIntoView(lstSearchResults.SelectedItem);
                    }
                    e.Handled = true;
                }
                // Nhấn mũi tên lên (Up) để duyệt ngược danh sách gợi ý
                else if (e.Key == Key.Up && vm.IsSearchDropDownOpen)
                {
                    if (lstSearchResults.SelectedIndex > 0)
                    {
                        lstSearchResults.SelectedIndex--;
                        lstSearchResults.ScrollIntoView(lstSearchResults.SelectedItem);
                    }
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi người dùng click chuột trực tiếp vào một item trong danh sách gợi ý
        /// </summary>
        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is EquipmentAnalysisVm vm)
            {
                vm.ConfirmSelection();
            }
        }

        /// <summary>
        /// Nếu bạn vẫn dùng ComboBox ở đâu đó, hàm này giúp hủy bôi đen tự động khi chọn item
        /// </summary>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var textBox = cb?.Template.FindName("PART_EditableTextBox", cb) as TextBox;
            if (textBox != null)
            {
                textBox.SelectionLength = 0;
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
    }
}
