using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.Services.WpfApp2.Services;
using WpfApp2.view.pages;

namespace WpfApp2.viewmodel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public ICommand ShowModelPageCommand { get; set; }
        public ICommand ShowBrandPageCommand { get; set; }

        private object _currentPage;
        public object CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(CurrentPage)));
            }
        }

        public MainViewModel()
        {
            ShowModelPageCommand = new RelayCommand(OpenModelPage);
            ShowBrandPageCommand = new RelayCommand(OpenBrandPage);

            //CurrentPage = new newModel(); // page mặc định
        }

        void OpenModelPage(object obj)
        {
            CurrentPage = new newModel();
        }

        void OpenBrandPage(object obj)
        {
            CurrentPage = new pageBrand();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}