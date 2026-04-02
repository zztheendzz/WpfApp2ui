
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
using WpfApp2.modelDTO;
using WpfApp2.viewmodel.analysis;
namespace WpfApp2.view.analysis
{
    /// <summary>
    /// Interaction logic for ModelAnalysis.xaml
    /// </summary>
    public partial class ModelAnalysis : Page
    {
        public ModelAnalysis()
        {
            InitializeComponent();
            DataContext = new ModelAnalysisVm();
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ModelAnalysisVm vm)
            {
                // 1. Nhấn Enter để chốt chọn
                if (e.Key == Key.Enter)
                {
                    vm.ConfirmSelection();
                    txtSearchModel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                    return;
                }

                // 2. Nhấn Down để xuống item dưới
                if (e.Key == Key.Down)
                {
                    if (vm.IsSearchDropDownOpen && lstModel.SelectedIndex < lstModel.Items.Count - 1)
                    {
                        lstModel.SelectedIndex++;
                        lstModel.ScrollIntoView(lstModel.SelectedItem);
                    }
                    e.Handled = true;
                }
                // 3. Nhấn Up để lên item trên
                else if (e.Key == Key.Up)
                {
                    if (vm.IsSearchDropDownOpen && lstModel.SelectedIndex > 0)
                    {
                        lstModel.SelectedIndex--;
                        lstModel.ScrollIntoView(lstModel.SelectedItem);
                    }
                    e.Handled = true;
                }
                // 4. Nhấn Escape để đóng nhanh popup
                else if (e.Key == Key.Escape)
                {
                    vm.IsSearchDropDownOpen = false;
                    e.Handled = true;
                }
            }
        }

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            // Khi click chuột vào item
            if (sender is ListBoxItem item && DataContext is ModelAnalysisVm vm)
            {
                vm.SelectedSearchResult = item.DataContext as SearchResultDto;
                vm.ConfirmSelection();
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
