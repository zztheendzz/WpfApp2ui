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
        private bool _clickInsidePopup = false;

        public PurchaseAnalysis()
        {
            InitializeComponent();
            this.PreviewMouseDown += OnGlobalMouseDown;

            // Định dạng hiển thị cho trục Y của BIỂU ĐỒ (VNĐ -> Triệu)
            Func<double, string> trieuFormatter = value => (value / 1000000).ToString("N1") + " Tr";


        }

        #region XỬ LÝ ĐÓNG/MỞ POPUP VÀ LOAD GỢI Ý

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is PurchaseAnalysisVm vm && sender is TextBox tb)
            {
                string type = tb.Tag?.ToString();
                if (string.IsNullOrEmpty(type)) return;

                // Reset trạng thái popup hiện tại để tránh xung đột
                CloseAllPopups(vm);

                // Load dữ liệu gợi ý dựa trên Tag (M, V, E, B)
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    vm.LoadAllSuggestions(type);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void OnGlobalMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not PurchaseAnalysisVm vm) return;

            // Nếu click vào bên trong Popup (scrollbar, item) thì không đóng
            if (_clickInsidePopup)
            {
                _clickInsidePopup = false;
                return;
            }

            DependencyObject clicked = e.OriginalSource as DependencyObject;

            // Nếu click vào chính các TextBox thì không đóng (để GotFocus xử lý)
            if (IsInside(clicked, txtSearchModel) ||
                IsInside(clicked, txtSearchVendor) ||
                IsInside(clicked, txtSearchEquip) ||
                IsInside(clicked, txtSearchBrand))
                return;

            // Click ra ngoài hoàn toàn -> Đóng tất cả popup
            CloseAllPopups(vm);
        }

        private void PopupChild_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _clickInsidePopup = true;
        }

        private void CloseAllPopups(PurchaseAnalysisVm vm)
        {
            vm.IsDropDownOpenM = false;
            vm.IsDropDownOpenV = false;
            vm.IsDropDownOpenE = false;
            vm.IsDropDownOpenB = false;
        }

        private bool IsInside(DependencyObject source, DependencyObject parent)
        {
            while (source != null)
            {
                if (source == parent) return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }

        #endregion

        #region ĐIỀU KHIỂN BÀN PHÍM VÀ CLICK ITEM

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            var vm = this.DataContext as PurchaseAnalysisVm;
            if (vm == null) return;

            // Lấy Type từ Tag, nếu Tag null (do mình vừa sửa XAML) thì mặc định check theo tên Name
            string type = textBox.Tag?.ToString();
            if (string.IsNullOrEmpty(type))
            {
                if (textBox.Name == "txtSearchModel") type = "M";
                else if (textBox.Name == "txtSearchVendor") type = "V";
                else if (textBox.Name == "txtSearchEquip") type = "E";
                else if (textBox.Name == "txtSearchBrand") type = "B";
            }

            // Xác định ListBox tương ứng để điều khiển
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
                        // Mở popup nếu nó đang đóng khi nhấn phím xuống
                        if (type == "M") vm.IsDropDownOpenM = true;
                        else if (type == "V") vm.IsDropDownOpenV = true;
                        else if (type == "E") vm.IsDropDownOpenE = true;
                        else if (type == "B") vm.IsDropDownOpenB = true;

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
                        // Chuyển focus sang control tiếp theo
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    CloseAllPopups(vm);
                    e.Handled = true;
                    break;
            }
        }

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as PurchaseAnalysisVm;
            if (vm == null) return;

            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
                dep = VisualTreeHelper.GetParent(dep);

            if (dep is ListBoxItem item)
            {
                var listBox = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                if (listBox == null) return;

                listBox.SelectedItem = item.DataContext;

                string type = listBox.Name switch
                {
                    "lstModel" => "M",
                    "lstVendor" => "V",
                    "lstEquip" => "E",
                    "lstBrand" => "B",
                    _ => ""
                };

                vm.ConfirmSelection(type);

                // Trả focus về TextBox tương ứng
                Control targetBox = type switch
                {
                    "M" => txtSearchModel,
                    "V" => txtSearchVendor,
                    "E" => txtSearchEquip,
                    "B" => txtSearchBrand,
                    _ => null
                };
                targetBox?.Focus();

                e.Handled = true;
            }
        }

        #endregion
    }
}