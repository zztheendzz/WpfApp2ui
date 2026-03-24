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
