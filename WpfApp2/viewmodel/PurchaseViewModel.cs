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
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.view.dialog;

namespace WpfApp2.viewmodel
{

    class PurchaseViewModel: INotifyPropertyChanged
    {
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ObservableCollection<PurchaseDto> purchases { get; set; }


        public PurchaseViewModel()
        {
            PurchaseService purchaseService = new PurchaseService();

            purchases = new ObservableCollection<PurchaseDto>(
                        purchaseService.GetPurchaseDTO());
            EditCommand = new RelayCommand(x => Edit((PurchaseDto)x));
            DeleteCommand = new RelayCommand(x => Delete((PurchaseDto)x));
            AddCommand = new RelayCommand(x => Add());
        }
        public void Delete(PurchaseDto purchase)
        {
            PurchaseService purchaseService = new PurchaseService();

            purchaseService.Delete(purchase.Id);

            purchases.Remove(purchase);
        }

        public void Edit(PurchaseDto purchase)
        {
            var dialog = new edit(purchase);

            if (dialog.ShowDialog() == true)
            {
                PurchaseService purchaseService = new PurchaseService();
                purchaseService.Edit(purchase);

                OnPropertyChanged(nameof(purchases));
            }

        }
        public void Add()
        {
            var purchase = new PurchaseDto();   // object mới

            var dialog = new edit(purchase);

            if (dialog.ShowDialog() == true)
            {
                PurchaseService purchaseService = new PurchaseService();

                int newId = purchaseService.Add(purchase);

                purchase.Id = newId;

                purchases.Add(purchase);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}