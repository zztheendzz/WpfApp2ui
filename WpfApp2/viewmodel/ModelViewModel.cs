using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using WpfApp2.model;
using WpfApp2.command;

namespace WpfApp2.viewmodel
{
    class ModelViewModel: INotifyPropertyChanged
    {
        public ObservableCollection<Model> Models { get; set; }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public ModelViewModel()
        {
            Models = new ObservableCollection<Model>();

            AddCommand = new RelayCommand(AddModel);
            DeleteCommand = new RelayCommand(DeleteModel);

            LoadData();
        }

        void LoadData()
        {
            Models.Add(new Model
            {
                Id = 1,
                ModelCode = "M001",
                ModelName = "Motor A",
                //Brand = "Omron",
                //Category = "Motor",
                //Status = "Active"
            });
        }

        void AddModel(object obj)
        {
            Models.Add(new Model
            {
                Id = Models.Count + 1,
                ModelCode = "New",
                ModelName = "New Model"
            });
        }

        void DeleteModel(object obj)
        {
            if (obj is Model m)
                Models.Remove(m);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }


}
