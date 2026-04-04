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