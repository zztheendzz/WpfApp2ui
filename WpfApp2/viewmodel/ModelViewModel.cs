using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDTO;
using WpfApp2.Services;
using WpfApp2.Services;
using WpfApp2.view.dialog;
using WpfApp2.view.pages;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace WpfApp2.viewmodel
{
    class ModelViewModel : INotifyPropertyChanged
    {
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ObservableCollection<ModelDto> Models { get; set; }


        public ModelViewModel() {
            ModelService modelService = new ModelService();

            Models = new ObservableCollection<ModelDto>(
                        modelService.GetModelDTO());
            EditCommand = new RelayCommand(x => Edit((ModelDto)x));
            DeleteCommand = new RelayCommand(x => Delete((ModelDto)x));
            AddCommand = new RelayCommand(x => Add());
        }
        public void Delete(ModelDto model)
        {
            ModelService service = new ModelService();

            service.Delete(model.Id);

            Models.Remove(model);
        }

        public void Edit(ModelDto model)
        {
            var dialog = new edit(model);

            if (dialog.ShowDialog() == true)
            {
                ModelService service = new ModelService();
                service.Edit(model);

                OnPropertyChanged(nameof(Models));
            }

        }
        public void Add()
        {
            var model = new ModelDto();   // object mới

            var dialog = new edit(model);

            if (dialog.ShowDialog() == true)
            {
                ModelService service = new ModelService();

                int newId = service.Add(model);

                model.Id = newId;

                Models.Add(model);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}