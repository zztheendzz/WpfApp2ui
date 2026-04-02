using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2.viewmodel.analysis;

namespace WpfApp2.view.analysis
{
    /// <summary>
    /// Interaction logic for PurchaseAnalysis.xaml
    /// </summary>
    public partial class PurchaseAnalysis : Page
    {
        public PurchaseAnalysis()
        {
            InitializeComponent();
            DataContext = new PurchaseAnalysisVm();
        }
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var type = textBox.Tag.ToString();
            var vm = (PurchaseAnalysisVm)this.DataContext;

            // Xác định ListBox nào cần điều khiển
            ListBox activeList = type == "M" ? lstModel : (type == "V" ? lstVendor : lstEquip);

            if (e.Key == Key.Down)
            {
                if (activeList.SelectedIndex < activeList.Items.Count - 1) activeList.SelectedIndex++;
                activeList.ScrollIntoView(activeList.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (activeList.SelectedIndex > 0) activeList.SelectedIndex--;
                activeList.ScrollIntoView(activeList.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                vm.ConfirmSelection(type);
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            // Lấy ListBox cha của item bị click
            var item = sender as ListBoxItem;
            if (item != null)
            {
                var listBox = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                var type = (listBox.Name == "lstModel") ? "M" : (listBox.Name == "lstVendor" ? "V" : "E");
                var vm = (PurchaseAnalysisVm)this.DataContext;

                // Dùng Dispatcher để đảm bảo SelectedItem đã cập nhật trước khi Confirm
                Dispatcher.BeginInvoke(new Action(() => vm.ConfirmSelection(type)));
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb != null)
            {
                cb.IsDropDownOpen = true;
            }
        }
        private void ComboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null) return;

            if (!cb.IsKeyboardFocusWithin)
            {
                cb.Focus();
                e.Handled = true; // chỉ handle khi chưa focus
            }

            cb.IsDropDownOpen = true;
        }
        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            comboBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                comboBox.ApplyTemplate();

                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                if (textBox != null)
                {
                    textBox.Focus(); // quan trọng
                    textBox.SelectionLength = 0;
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }
}
