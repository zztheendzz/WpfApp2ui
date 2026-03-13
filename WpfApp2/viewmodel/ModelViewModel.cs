using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;

namespace WpfApp2.viewmodel
{
    class ModelViewModel: INotifyPropertyChanged
    {
        public ObservableCollection<Model> Models { get; set; }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ModelService ModelService { get; set; }

        public ModelViewModel()
        {
            Models = new ObservableCollection<Model>();
            ModelService = new ModelService();
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
                BrandId = 1
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
                ModelName = "New Model",
                BrandId = 1
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
