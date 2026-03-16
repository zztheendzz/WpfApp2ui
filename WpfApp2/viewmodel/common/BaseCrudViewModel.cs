using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.view.dialog;
using WpfApp2.Services;

namespace WpfApp2.viewmodel.common
{
    public class BaseCrudViewModel<T> : INotifyPropertyChanged where T : class
    {
        public BaseService<T> _service;

        private ObservableCollection<T> _items;
        public ObservableCollection<T> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }
        private T _selectedItem;
        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand EditCommand { get; set; }

        public BaseCrudViewModel(BaseService<T> service)
        {
            _service = service;
            LoadData();
            AddCommand = new RelayCommand(Add);
            DeleteCommand = new RelayCommand(Delete);
            UpdateCommand = new RelayCommand(Update);
            EditCommand = new RelayCommand(Edit);
        }
        void Edit(object obj)
        {
            if (obj is T item)
            {
                var dialog = new edit(item);

                if (dialog.ShowDialog() == true)
                {
                    _service.Update(item);
                    LoadData();
                }
            }
        }
        void Add(object obj)
        {
            var item = Activator.CreateInstance<T>();

            var dialog = new edit(item);

            if (dialog.ShowDialog() == true)
            {
                _service.Add(item);
                LoadData();
            }
        }

        void Delete(object obj)
        {
            if (obj is T item)
            {
                _service.Delete(item);
                Items.Remove(item);
            }
        }

        void Update(object obj)
        {
            if (SelectedItem == null) return;

            _service.Update(SelectedItem);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void LoadData()
        {
            Items = new ObservableCollection<T>(_service.GetAll());
        }
    }
}
