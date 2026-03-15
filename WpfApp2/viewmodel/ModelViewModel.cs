using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.view.pages;

namespace WpfApp2.viewmodel
{
    class ModelViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Model> _models;

        public ObservableCollection<Model> Models
        {
            get => _models;
            set
            {
                _models = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Models)));
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public ModelService ModelService { get; set; }

        public ModelViewModel()
        {
            ModelService = new ModelService();

            Models = new ObservableCollection<Model>(ModelService.GetAll());

            AddCommand = new RelayCommand(AddModel);
            DeleteCommand = new RelayCommand(DeleteModel);
        }

        void AddModel(object obj) { }

        void DeleteModel(object obj) { }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
