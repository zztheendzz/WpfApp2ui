using System; // Thêm thư viện này
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

            // Khởi tạo DataContext
            DataContext = new VendorAnalysisVm();

            // Định dạng biểu đồ: Chia cho 1.000.000 và thêm chữ "Tr"
            if (ChartMonthlySpend != null && ChartMonthlySpend.AxisY.Count > 0)
            {
                ChartMonthlySpend.AxisY[0].LabelFormatter = value => (value / 1000000).ToString("N1") + " Tr";
            }
        }

        /// <summary>
        /// Xử lý điều hướng bàn phím (Lên, Xuống, Enter, Escape) cho ô tìm kiếm
        /// </summary>
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is VendorAnalysisVm vm)) return;

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
                        e.Handled = true;
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

            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
                dep = VisualTreeHelper.GetParent(dep);

            if (dep is ListBoxItem item && item.Content is SearchResultDto data)
            {
                vm.ConfirmSelection(data);
                txtSearchVendor.Focus();
            }
        }
    }
}