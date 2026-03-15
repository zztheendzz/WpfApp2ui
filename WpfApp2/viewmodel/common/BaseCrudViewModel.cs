using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;

namespace WpfApp2.viewmodel.common
{
    public class BaseCrudViewModel<T> : INotifyPropertyChanged where T : class
    {
        protected BaseService<T> _service;

        public ObservableCollection<T> Items { get; set; }

        public T SelectedItem { get; set; }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand UpdateCommand { get; set; }

        public BaseCrudViewModel(BaseService<T> service)
        {
            _service = service;

            Items = new ObservableCollection<T>(_service.GetAll());

            AddCommand = new RelayCommand(Add);
            DeleteCommand = new RelayCommand(Delete);
            UpdateCommand = new RelayCommand(Update);
        }

        void Add(object obj)
        {
            if (SelectedItem == null) return;

            _service.Add(SelectedItem);
            Items.Add(SelectedItem);
        }

        void Delete(object obj)
        {
            if (SelectedItem == null) return;

            _service.Delete(SelectedItem);
            Items.Remove(SelectedItem);
        }

        void Update(object obj)
        {
            if (SelectedItem == null) return;

            _service.Update(SelectedItem);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
