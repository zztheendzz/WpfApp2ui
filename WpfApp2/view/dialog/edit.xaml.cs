using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfApp2.view.dialog
{
    public partial class edit : Window
    {
        // 🔥 CONFIG FK (chỉ để biết field nào là FK)
        private Dictionary<string, string> _lookupMap = new()
        {
            { "ModelId", "ModelName" },
            { "BrandId", "BrandName" },
            { "CategoryId", "CategoryName" },
            { "VendorId", "VendorName" },
            { "EquipmentId", "EquipmentName" },
            { "UserId", "UserName" },
            { "CurrencyCode", "Name" }
        };

        public edit(object model)
        {
            InitializeComponent();
            DataContext = model;

            GenerateForm(model);
        }

        void GenerateForm(object model)
        {
            var props = model.GetType().GetProperties();

            foreach (var prop in props)
            {
                // ❌ bỏ field không cần
                if (prop.PropertyType == typeof(bool)) continue;
                if (prop.Name == "Id") continue;
                //if (prop.Name == "UserName") continue;
                if (prop.Name == "CreateAt") continue;


                // ❌ bỏ Name nếu có Id tương ứng
                if (prop.Name.EndsWith("Id"))
                {
                    var fkName = prop.Name.Replace("Name", "Id");
                    if (props.Any(p => p.Name == fkName))
                        continue;
                }

                // LABEL
                FormPanel.Children.Add(new TextBlock
                {
                    Text = SplitName(prop.Name),
                    Margin = new Thickness(0, 10, 0, 2)
                });

                // CONTROL
                FormPanel.Children.Add(CreateControl(prop));
            }
        }

        FrameworkElement CreateControl(PropertyInfo prop)
        {
            // 🔥 FK → ComboBox (NHƯNG chỉ bind Name)
            if (_lookupMap.ContainsKey(prop.Name))
            {
                var nameProp = prop.Name.Replace("Id", "Name");

                var combo = new ComboBox
                {
                    IsEditable = true,
                    IsTextSearchEnabled = true,
                    StaysOpenOnEdit = true,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                combo.SetBinding(ComboBox.TextProperty,
                    new Binding(nameProp)
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });

                return combo;
            }

            // 🔥 DATE
            if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                var date = new DatePicker
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };

                date.SetBinding(DatePicker.SelectedDateProperty,
                    new Binding(prop.Name)
                    {
                        Mode = BindingMode.TwoWay
                    });

                return date;
            }

            // 🔥 DEFAULT
            var textbox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

            textbox.SetBinding(TextBox.TextProperty,
                new Binding(prop.Name)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

            return textbox;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 🔥 Không xử lý gì → DTO đã có data
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}