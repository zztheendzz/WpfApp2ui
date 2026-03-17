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
using WpfApp2.modelDTO;

namespace WpfApp2.viewmodel
{

    class EquipmentViewModel : INotifyPropertyChanged
    {
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ObservableCollection<EquipmentDto> Equipments { get; set; }


        public EquipmentViewModel()
        {
            EquipmentService equipmentService = new EquipmentService();

            Equipments = new ObservableCollection<EquipmentDto>(
                        equipmentService.GetEquipmentDto());
            EditCommand = new RelayCommand(x => Edit((EquipmentDto)x));
            DeleteCommand = new RelayCommand(x => Delete((EquipmentDto)x));
            AddCommand = new RelayCommand(x => Add());
        }
        public void Delete(EquipmentDto equipment)
        {
            EquipmentService equipmentService = new EquipmentService();

            equipmentService.Delete(equipment.Id);

            Equipments.Remove(equipment);
        }

        public void Edit(EquipmentDto equipment)
        {
            var dialog = new edit(equipment);

            if (dialog.ShowDialog() == true)
            {
                EquipmentService equipmentService = new EquipmentService();
                equipmentService.Edit(equipment);

                OnPropertyChanged(nameof(equipment));
            }

        }
        public void Add()
        {
            var equioment = new EquipmentDto();   // object mới

            var dialog = new edit(equioment);

            if (dialog.ShowDialog() == true)
            {
                EquipmentService equipmentService = new EquipmentService();

                int newId = equipmentService.Add(equioment);

                equioment.Id = newId;

                equipmentService.Add(equioment);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}