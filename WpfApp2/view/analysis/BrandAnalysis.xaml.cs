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
    /// Interaction logic for BrandAnalysis.xaml
    /// </summary>
    public partial class BrandAnalysis : Page
    {
        public BrandAnalysis()
        {
            InitializeComponent();
            DataContext = new BrandAnalysisVm();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            // Tìm TextBox bên trong ComboBox và hủy bôi đen
            var textBox = cb?.Template.FindName("PART_EditableTextBox", cb) as TextBox;
            if (textBox != null)
            {
                textBox.SelectionLength = 0; // Hủy bôi đen
                textBox.CaretIndex = textBox.Text.Length; // Đưa con trỏ về cuối
            }
        }
    }
}
