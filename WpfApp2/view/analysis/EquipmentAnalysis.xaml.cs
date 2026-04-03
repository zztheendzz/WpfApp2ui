using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp2.modelDTO;
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
            DataContext = new EquipmentAnalysisVm();
        }

        /// <summary>
        /// Xử lý điều hướng bằng phím mũi tên và xác nhận bằng phím Enter trên TextBox tìm kiếm
        /// </summary>
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is EquipmentAnalysisVm vm)) return;

            // 1. Nhấn Enter để xác nhận lựa chọn từ danh sách gợi ý (Sử dụng item đang highlight)
            if (e.Key == Key.Enter)
            {
                vm.ConfirmSelection();
                e.Handled = true;
            }
            // 2. Nhấn mũi tên xuống (Down) để duyệt danh sách gợi ý
            else if (e.Key == Key.Down && vm.IsSearchDropDownOpen)
            {
                if (lstSearchResults.SelectedIndex < lstSearchResults.Items.Count - 1)
                {
                    lstSearchResults.SelectedIndex++;
                    lstSearchResults.ScrollIntoView(lstSearchResults.SelectedItem);
                }
                e.Handled = true;
            }
            // 3. Nhấn mũi tên lên (Up) để duyệt ngược danh sách gợi ý
            else if (e.Key == Key.Up && vm.IsSearchDropDownOpen)
            {
                if (lstSearchResults.SelectedIndex > 0)
                {
                    lstSearchResults.SelectedIndex--;
                    lstSearchResults.ScrollIntoView(lstSearchResults.SelectedItem);
                }
                e.Handled = true;
            }
            // 4. Nhấn Escape để đóng nhanh DropDown
            else if (e.Key == Key.Escape)
            {
                vm.IsSearchDropDownOpen = false;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi người dùng click chuột trực tiếp vào một item trong danh sách gợi ý.
        /// Tìm chính xác DataContext của ListBoxItem bị click để truyền vào ViewModel.
        /// </summary>
        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is EquipmentAnalysisVm vm)) return;

            // Tìm đối tượng ListBoxItem từ điểm click chuột (OriginalSource)
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            // Nếu tìm thấy ListBoxItem và nó chứa dữ liệu SearchResultDto
            if (dep is ListBoxItem item && item.Content is SearchResultDto data)
            {
                vm.ConfirmSelection(data); // Truyền trực tiếp item được click vào VM
            }
        }

        /// <summary>
        /// Hủy bôi đen tự động và đưa con trỏ về cuối TextBox khi Selection thay đổi (nếu dùng ComboBox)
        /// </summary>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                var textBox = cb.Template.FindName("PART_EditableTextBox", cb) as TextBox;
                if (textBox != null)
                {
                    textBox.SelectionLength = 0;
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }
    }
}