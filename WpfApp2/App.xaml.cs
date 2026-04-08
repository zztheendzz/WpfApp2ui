using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 1. Fix lỗi parse số (Dấu chấm/phẩy khi đọc DB)
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

            // 2. Fix lỗi hiển thị định dạng trên UI WPF
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage("en-US")));
        }
    }

}
