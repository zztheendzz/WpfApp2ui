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
            // DataContext đã được định nghĩa trong XAML nên không cần khởi tạo lại ở đây
        }

        /// <summary>
        /// Xử lý điều hướng phím (Lên, Xuống, Enter, Esc) cho các ô SearchBox
        /// </summary>
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || textBox.Tag == null) return;

            var vm = this.DataContext as PurchaseAnalysisVm;
            if (vm == null) return;

            string type = textBox.Tag.ToString(); // "M", "V", hoặc "E" lấy từ Tag trong XAML

            // Xác định ListBox tương ứng dựa trên loại tìm kiếm để điều khiển Index
            ListBox activeList = type switch
            {
                "M" => lstModel,
                "V" => lstVendor,
                "E" => lstEquip,
                _ => null
            };

            if (activeList == null) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (activeList.Items.Count > 0)
                    {
                        // Di chuyển xuống trong danh sách gợi ý
                        int nextIndex = activeList.SelectedIndex + 1;
                        activeList.SelectedIndex = (nextIndex >= activeList.Items.Count) ? 0 : nextIndex;
                        activeList.ScrollIntoView(activeList.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (activeList.Items.Count > 0)
                    {
                        // Di chuyển lên trong danh sách gợi ý
                        int prevIndex = activeList.SelectedIndex - 1;
                        activeList.SelectedIndex = (prevIndex < 0) ? activeList.Items.Count - 1 : prevIndex;
                        activeList.ScrollIntoView(activeList.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    // Khi nhấn Enter, xác nhận lựa chọn đang highlight trong ListBox
                    if (activeList.SelectedItem != null)
                    {
                        vm.ConfirmSelection(type);
                        // Chuyển focus sang control tiếp theo để tăng trải nghiệm nhập liệu
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    // Đóng Popup khi nhấn Esc
                    if (type == "M") vm.IsDropDownOpenM = false;
                    else if (type == "V") vm.IsDropDownOpenV = false;
                    else if (type == "E") vm.IsDropDownOpenE = false;
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Xử lý khi click chuột trực tiếp vào một dòng gợi ý trong ListBox của Popup
        /// </summary>
        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            var vm = this.DataContext as PurchaseAnalysisVm;
            if (vm == null) return;

            // Truy tìm ListBoxItem cha của thành phần bị click (OriginalSource)
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
                dep = VisualTreeHelper.GetParent(dep);

            if (dep is ListBoxItem item)
            {
                // Tìm ListBox chứa Item này để biết người dùng đang chọn ở ô nào (Model, Vendor hay Equip)
                var listBox = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                if (listBox == null) return;

                string type = (listBox.Name == "lstModel") ? "M" :
                              (listBox.Name == "lstVendor") ? "V" : "E";

                // Gọi hàm xác nhận trong ViewModel
                vm.ConfirmSelection(type);

                // Sau khi click chọn, trả lại Focus cho TextBox tương ứng để người dùng biết mình đang ở đâu
                if (type == "M") txtSearchModel.Focus();
                else if (type == "V") txtSearchVendor.Focus();
                else if (type == "E") txtSearchEquip.Focus();
            }
        }
    }
}