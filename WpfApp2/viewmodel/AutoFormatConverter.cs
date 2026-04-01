using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp2.viewmodel.tableVm { 
public class AutoFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return "";

        if (double.TryParse(value.ToString(), out double num))
            return num.ToString("N0", new CultureInfo("vi-VN"));

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException(); // hoặc xử lý nếu cần edit
    }
}
}