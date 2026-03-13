using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;

namespace WpfApp2.viewmodel
{
    class PurchaseViewModel: INotifyPropertyChanged
    {

        public ObservableCollection<PurchaseHistory> puchaseHistories { get; set; }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ModelService ModelService { get; set; }

        public PurchaseViewModel()
        {
            puchaseHistories = new ObservableCollection<PurchaseHistory>();
            ModelService = new ModelService();
            AddCommand = new RelayCommand(AddModel);
            DeleteCommand = new RelayCommand(DeleteModel);
            LoadData();
        }

        void LoadData()
        {

        }

        void AddModel(object obj)
        {


        }

        void DeleteModel(object obj)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

