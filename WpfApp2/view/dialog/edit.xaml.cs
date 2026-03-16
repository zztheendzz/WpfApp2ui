using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
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
using WpfApp2.viewmodel;
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp2.view.dialog
{
    /// <summary>
    /// Interaction logic for edit.xaml
    /// </summary>
    public partial class edit : Window
    {
        public edit(object model)
        {
            InitializeComponent();
            DataContext = model;

            GenerateForm(model);
        }
        void GenerateForm(object model)
        {
            var properties = model.GetType().GetProperties();

            foreach (var prop in properties)
            {
                if (prop.Name == "Id") continue;

                var label = new TextBlock
                {
                    Text = GetDisplayName(prop),
                    Margin = new Thickness(0, 10, 0, 2)
                };

                var textbox = new TextBox
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };

                textbox.SetBinding(TextBox.TextProperty,
                    new Binding(prop.Name)
                    {
                        Mode = BindingMode.TwoWay
                    });

                FormPanel.Children.Add(label);
                FormPanel.Children.Add(textbox);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        public static string SplitName(string name)
        {
            return Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
        }

        string GetDisplayName(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<DisplayNameAttribute>();

            if (attr != null)
                return attr.DisplayName;

            return SplitName(prop.Name);
        }
    }
}
