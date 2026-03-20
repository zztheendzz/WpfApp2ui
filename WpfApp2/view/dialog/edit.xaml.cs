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
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.viewmodel;
using static System.Net.Mime.MediaTypeNames;
using Dapper;

namespace WpfApp2.view.dialog
{
    /// <summary>
    /// Interaction logic for edit.xaml
    /// </summary>
    public partial class edit : Window
    {
        private readonly DatabaseService _db = new();

        // 🔥 CONFIG FK
        private Dictionary<string, (string Table, string Display)> _lookupMap
            = new()
        {
        { "BrandId", ("Brand", "BrandName") },
        { "CategoryId", ("Category", "CategoryName") },
        { "VendorId", ("Vendor", "VendorName") },
        { "EquipmentId", ("Equipment", "EquipmentName") },
        { "UserId", ("User", "UserName") },
        { "CurrencyCode", ("Currency", "Name") } // special: key không phải Id
        };

        // 🔥 CACHE (tránh query nhiều lần)
        private Dictionary<string, List<dynamic>> _cache = new();

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
                if (prop.Name == "Id") continue;
                if (prop.Name.EndsWith("Name"))
                {
                    var fkName = prop.Name.Replace("Name", "Id");

                    if (props.Any(p => p.Name == fkName))
                        continue; // bỏ BrandName, VendorName...
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
            // 🔥 FK → COMBOBOX
            if (_lookupMap.ContainsKey(prop.Name))
            {
                var (table, display) = _lookupMap[prop.Name];
                var data = LoadLookup(table, display);

                var combo = new ComboBox
                {
                    ItemsSource = data,
                    DisplayMemberPath = display,
                    SelectedValuePath = prop.Name == "CurrencyCode" ? "Code" : "Id",
                    Margin = new Thickness(0, 0, 0, 10)
                };

                combo.SetBinding(ComboBox.SelectedValueProperty,
                    new Binding(prop.Name)
                    {
                        Mode = BindingMode.TwoWay
                    });

                return combo;
            }

            // 🔥 BOOL → CHECKBOX
            if (prop.PropertyType == typeof(bool))
            {
                var check = new CheckBox
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };

                check.SetBinding(CheckBox.IsCheckedProperty,
                    new Binding(prop.Name)
                    {
                        Mode = BindingMode.TwoWay
                    });

                return check;
            }

            // 🔥 DATE → DATEPICKER
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

            // 🔥 DEFAULT → TEXTBOX
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

        List<dynamic> LoadLookup(string table, string display)
        {
            string key = $"{table}_{display}";

            if (_cache.ContainsKey(key))
                return _cache[key];

            using var conn = _db.GetConnection();

            var data = conn.Query(
                $"SELECT * FROM {table}"
            ).ToList();

            _cache[key] = data;

            return data;
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
    }
}