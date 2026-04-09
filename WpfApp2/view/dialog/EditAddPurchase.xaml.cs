using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp2.command;
using WpfApp2.modelDTO;

namespace WpfApp2.view.dialog
{
    /// <summary>
    /// Interaction logic for EditAddPurchase.xaml
    /// </summary>
    public partial class EditAddPurchase : Window
    {
        public EditAddPurchase(PurchaseDto purchase)
        {
            InitializeComponent();
            DataContext = purchase;
            SaveCommand = new RelayCommand(OnSave);
        }
        public ICommand SaveCommand { get; }
        private void OnSave(object obj)
        {
            // 🔥 chỉ đóng dialog
            DialogResult = true;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // 🔥 đây là trigger cho ViewModel
        }

        #region ONLY INTEGER (Quantity)

        private void NumberOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void OnPasteNumberOnly(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(text, "^[0-9]+$"))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
     }
        #endregion

    #region DECIMAL (UnitPrice)

    private void DecimalOnly(object sender, TextCompositionEventArgs e)
    {
        TextBox tb = sender as TextBox;

        string fullText = tb.Text.Insert(tb.SelectionStart, e.Text);

        e.Handled = !Regex.IsMatch(fullText, @"^\d*\.?\d*$");
    }

    private void OnPasteDecimalOnly(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            if (!Regex.IsMatch(text, @"^\d*\.?\d*$"))
                e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }

    #endregion
}
}
