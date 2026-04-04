using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp2.modelDTO;
using WpfApp2.viewmodel.analysis;

namespace WpfApp2.view.analysis
{
    public partial class VendorAnalysis : Page
    {
        public VendorAnalysis()
        {
            InitializeComponent();
            // Khởi tạo DataContext là Vendor ViewModel
            DataContext = new VendorAnalysisVm();
        }

        /// <summary>
        /// Xử lý điều hướng bàn phím (Lên, Xuống, Enter, Escape) cho ô tìm kiếm
        /// </summary>
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is VendorAnalysisVm vm)) return;

            // Nếu Dropdown không mở, không cần xử lý các phím điều hướng danh sách
            if (!vm.IsSearchDropDownOpen) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (lstVendor.SelectedIndex < lstVendor.Items.Count - 1)
                        lstVendor.SelectedIndex++;
                    lstVendor.ScrollIntoView(lstVendor.SelectedItem);
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (lstVendor.SelectedIndex > 0)
                        lstVendor.SelectedIndex--;
                    lstVendor.ScrollIntoView(lstVendor.SelectedItem);
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (lstVendor.SelectedItem is SearchResultDto selected)
                    {
                        vm.ConfirmSelection(selected);
                        e.Handled = true; // Chặn Enter kích hoạt lệnh khác của TextBox
                    }
                    break;

                case Key.Escape:
                    vm.IsSearchDropDownOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Xử lý khi click chuột trực tiếp vào một Item trong danh sách gợi ý
        /// </summary>
        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is VendorAnalysisVm vm)) return;

            // Tìm đối tượng ListBoxItem thực sự bị click trong Visual Tree
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
                dep = VisualTreeHelper.GetParent(dep);

            if (dep is ListBoxItem item && item.Content is SearchResultDto data)
            {
                vm.ConfirmSelection(data);
                // Trả focus về TextBox sau khi chọn để người dùng có thể gõ tiếp nếu muốn
                txtSearchVendor.Focus();
            }
        }
    }
}