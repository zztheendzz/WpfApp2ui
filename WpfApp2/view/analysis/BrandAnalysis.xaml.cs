using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp2.modelDTO;
using WpfApp2.viewmodel.analysis;

namespace WpfApp2.view.analysis
{
    public partial class BrandAnalysis : Page
    {
        public BrandAnalysis()
        {
            InitializeComponent();
            DataContext = new BrandAnalysisVm();
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is BrandAnalysisVm vm)) return;

            if (e.Key == Key.Enter)
            {
                vm.ConfirmSelection(); // Chọn item đang highlight
                e.Handled = true;
            }
            else if (e.Key == Key.Down && vm.IsSearchDropDownOpen)
            {
                if (lstBrand.SelectedIndex < lstBrand.Items.Count - 1)
                {
                    lstBrand.SelectedIndex++;
                    lstBrand.ScrollIntoView(lstBrand.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up && vm.IsSearchDropDownOpen)
            {
                if (lstBrand.SelectedIndex > 0)
                {
                    lstBrand.SelectedIndex--;
                    lstBrand.ScrollIntoView(lstBrand.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.IsSearchDropDownOpen = false;
                e.Handled = true;
            }
        }

        // CẬP NHẬT: Xử lý lấy item chính xác khi click chuột
        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is BrandAnalysisVm vm)) return;

            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is ListBoxItem item && item.Content is SearchResultDto data)
            {
                vm.ConfirmSelection(data); // Truyền dữ liệu dòng được click vào VM
            }
        }
    }
}