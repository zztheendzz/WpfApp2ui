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
            DataContext = new VendorAnalysisVm();
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is VendorAnalysisVm vm)) return;

            if (e.Key == Key.Enter)
            {
                vm.ConfirmSelection(); // Dùng item đang được chọn (highlight)
                e.Handled = true;
            }
            else if (e.Key == Key.Down && vm.IsSearchDropDownOpen)
            {
                if (lstVendor.SelectedIndex < lstVendor.Items.Count - 1)
                {
                    lstVendor.SelectedIndex++;
                    lstVendor.ScrollIntoView(lstVendor.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up && vm.IsSearchDropDownOpen)
            {
                if (lstVendor.SelectedIndex > 0)
                {
                    lstVendor.SelectedIndex--;
                    lstVendor.ScrollIntoView(lstVendor.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.IsSearchDropDownOpen = false;
                e.Handled = true;
            }
        }

        // Chỉnh sửa: Lấy chính xác Item bị Click chuột
        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is VendorAnalysisVm vm)) return;

            // Tìm ListBoxItem chứa điểm click
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is ListBoxItem item && item.Content is SearchResultDto data)
            {
                vm.ConfirmSelection(data); // Truyền trực tiếp data của dòng bị click
            }
        }
    }
}