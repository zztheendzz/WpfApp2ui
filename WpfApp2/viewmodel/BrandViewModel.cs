using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.Services;
using WpfApp2.Services;
using WpfApp2.view.dialog;


namespace WpfApp2.viewmodel
{

    class BrandViewModel 
    {
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ObservableCollection<BrandDto> Brands { get; set; }


        public BrandViewModel()
        {
            BrandService brandService = new BrandService();

            Brands = new ObservableCollection<BrandDto>(
                        brandService.GetBrandDTO());
            EditCommand = new RelayCommand(x => Edit((BrandDto)x));
            DeleteCommand = new RelayCommand(x => Delete((BrandDto)x));
            AddCommand = new RelayCommand(x => Add());
        }
        public void Delete(BrandDto brand)
        {
            BrandService service = new BrandService();

            service.Delete(brand.Id);

            Brands.Remove(brand);
        }

        public void Edit(BrandDto brand)
        {
            var dialog = new edit(brand);

            if (dialog.ShowDialog() == true)
            {
                BrandService service = new BrandService();
                service.Edit(brand);

                OnPropertyChanged(nameof(Brand));
            }

        }
        public void Add()
        {
            var brand = new BrandDto();   // object mới

            var dialog = new edit(brand);

            if (dialog.ShowDialog() == true)
            {
                BrandService service = new BrandService();

                int newId = service.Add(brand);

                brand.Id = newId;

                Brands.Add(brand);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}