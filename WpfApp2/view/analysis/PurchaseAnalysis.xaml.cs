using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp2.viewmodel.analysis;

namespace WpfApp2.view.analysis
{
    public partial class PurchaseAnalysis : Page
    {
        public PurchaseAnalysis()
        {
            InitializeComponent();

            // Định dạng hiển thị cho trục Y của CẢ 2 BIỂU ĐỒ
            // Logic: Chia cho 1.000.000 và thêm hậu tố "Tr"
            Func<double, string> trieuFormatter = value => (value / 1000000).ToString("N1") + " Tr";

            // Áp dụng cho biểu đồ Vendor (Bên trái)
            if (ChartVendor != null && ChartVendor.AxisY.Count > 0)
            {
                ChartVendor.AxisY[0].LabelFormatter = trieuFormatter;
            }

            if (ChartVendor != null && ChartVendor.AxisY.Count > 0)
            {
                ChartVendor.AxisY[0].LabelFormatter = trieuFormatter;
            }

            // Áp dụng cho biểu đồ 2 (Trend)
            if (ChartTrend != null && ChartTrend.AxisY.Count > 0)
            {
                ChartTrend.AxisY[0].LabelFormatter = trieuFormatter;
            }
        }

        // --- GIỮ NGUYÊN CÁC HÀM XỬ LÝ SEARCH BOX BÊN DƯỚI ---

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || textBox.Tag == null) return;

            var vm = this.DataContext as PurchaseAnalysisVm;
            if (vm == null) return;

            string type = textBox.Tag.ToString();

            ListBox activeList = type switch
            {
                "M" => lstModel,
                "V" => lstVendor,
                "E" => lstEquip,
                "B" => lstBrand,
                _ => null
            };

            if (activeList == null) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (activeList.Items.Count > 0)
                    {
                        int nextIndex = activeList.SelectedIndex + 1;
                        activeList.SelectedIndex = (nextIndex >= activeList.Items.Count) ? 0 : nextIndex;
                        activeList.ScrollIntoView(activeList.SelectedItem);
                    }
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (activeList.Items.Count > 0)
                    {
                        int prevIndex = activeList.SelectedIndex - 1;
                        activeList.SelectedIndex = (prevIndex < 0) ? activeList.Items.Count - 1 : prevIndex;
                        activeList.ScrollIntoView(activeList.SelectedItem);
                    }
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (activeList.SelectedItem != null)
                    {
                        vm.ConfirmSelection(type);
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    if (type == "M") vm.IsDropDownOpenM = false;
                    else if (type == "V") vm.IsDropDownOpenV = false;
                    else if (type == "E") vm.IsDropDownOpenE = false;
                    else if (type == "B") vm.IsDropDownOpenB = false;
                    e.Handled = true;
                    break;
            }
        }

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            var vm = this.DataContext as PurchaseAnalysisVm;
            if (vm == null) return;

            // Tìm ListBoxItem từ vị trí click
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
                dep = VisualTreeHelper.GetParent(dep);

            if (dep is ListBoxItem item)
            {
                var listBox = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                if (listBox == null) return;

                // QUAN TRỌNG: Ép ListBox chọn đúng item vừa click chuột vào
                listBox.SelectedItem = item.DataContext;

                string type = (listBox.Name == "lstModel") ? "M" :
                              (listBox.Name == "lstVendor") ? "V" :
                                (listBox.Name == "lstEquip") ? "E" : "B";
                // Thực thi logic xác nhận lựa chọn trong ViewModel
                vm.ConfirmSelection(type);

                // Trả lại focus cho TextBox tương ứng để người dùng có thể gõ tiếp hoặc dùng phím mũi tên
                if (type == "M") txtSearchModel.Focus();
                else if (type == "V") txtSearchVendor.Focus();
                else if (type == "E") txtSearchEquip.Focus();
                else if (type == "B") txtSearchBrand.Focus();
                // Đánh dấu là đã xử lý xong để tránh các sự kiện bubbling khác
                e.Handled = true;
            }
        }
    }
}