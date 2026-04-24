using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using WpfApp2.Properties;
using Xceed.Wpf.AvalonDock.Properties;
using System.Threading;
namespace WpfApp2.Services.langService
{
    public class LanguageService : INotifyPropertyChanged
    {
        private static LanguageService _instance;
        public static LanguageService Instance => _instance ??= new LanguageService();

        public string this[string key]
            => Resources.ResourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture);

        public event PropertyChangedEventHandler PropertyChanged;

        public void ChangeLanguage(string culture)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            // trigger update toàn bộ binding
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
