using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.view.dialog;

using WpfApp2.view.pages;

namespace WpfApp2.viewmodel
{
    class VendorViewModel:INotifyPropertyChanged
    {
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ObservableCollection<VendorDto> Vendors { get; set; }


        public VendorViewModel()
        {
            VendorService vendorService = new VendorService();

            Vendors = new ObservableCollection<VendorDto>(
                        vendorService.GetVendorDTO());
            EditCommand = new RelayCommand(x => Edit((VendorDto)x));
            DeleteCommand = new RelayCommand(x => Delete((VendorDto)x));
            AddCommand = new RelayCommand(x => Add());
        }
        public void Delete(VendorDto vendor)
        {
            VendorService vendorService = new VendorService();

            vendorService.Delete(vendor.Id);

            Vendors.Remove(vendor);
        }

        public void Edit(VendorDto vendor)
        {
            var dialog = new edit(vendor);

            if (dialog.ShowDialog() == true)
            {
                VendorService vendorService = new VendorService();
                vendorService.Edit(vendor);

                OnPropertyChanged(nameof(vendor));
            }

        }
        public void Add()
        {
            var vendor = new VendorDto();   // object mới

            var dialog = new edit(vendor);

            if (dialog.ShowDialog() == true)
            {
                VendorService vendorService = new VendorService();

                int newId = vendorService.Add(vendor);

                vendor.Id = newId;

                Vendors.Add(vendor);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}