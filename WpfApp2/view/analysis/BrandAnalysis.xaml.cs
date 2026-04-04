using System; // Thêm thư viện này
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

            // Định dạng biểu đồ: Chia giá trị cho 1.000.000 và thêm hậu tố "Tr"
            if (ChartBrandMonthly != null && ChartBrandMonthly.AxisY.Count > 0)
            {
                ChartBrandMonthly.AxisY[0].LabelFormatter = value => (value / 1000000).ToString("N1") + " Tr";
            }
        }

        // --- CÁC HÀM XỬ LÝ SEARCH BOX GIỮ NGUYÊN ---

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is BrandAnalysisVm vm)) return;
            if (!vm.IsSearchDropDownOpen) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (LstSuggestions.SelectedIndex < LstSuggestions.Items.Count - 1)
                        LstSuggestions.SelectedIndex++;
                    LstSuggestions.ScrollIntoView(LstSuggestions.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (LstSuggestions.SelectedIndex > 0)
                        LstSuggestions.SelectedIndex--;
                    LstSuggestions.ScrollIntoView(LstSuggestions.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (LstSuggestions.SelectedItem is SearchResultDto selected)
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

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is BrandAnalysisVm vm)) return;

            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
                dep = VisualTreeHelper.GetParent(dep);

            if (dep is ListBoxItem item && item.Content is SearchResultDto data)
            {
                vm.ConfirmSelection(data);
                // Thêm dòng này để trả focus về TextBox sau khi chọn
                TxtSearch.Focus();
            }
        }
    }
}