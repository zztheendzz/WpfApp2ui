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

namespace WpfApp2.viewmodel
{
    class BrandViewModel
    {
        private ObservableCollection<Brand> _brands;

        public ObservableCollection<Brand> Brands
        {
            get => _brands;
            set
            {
                _brands = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Brands)));
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public BrandService BrandService { get; set; }

        public BrandViewModel()
        {
            BrandService = new BrandService();

            Brands = new ObservableCollection<Brand>(BrandService.GetAll());

            AddCommand = new RelayCommand(AddBrand);
            DeleteCommand = new RelayCommand(DeleteBrand);
        }

        void AddBrand(object obj) { }

        void DeleteBrand(object obj) { }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
