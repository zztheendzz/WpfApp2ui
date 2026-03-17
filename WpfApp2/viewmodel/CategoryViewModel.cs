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


namespace WpfApp2.viewmodel
{

    class CategoryViewModel
    {
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ObservableCollection<CategoryDto> categorys { get; set; }


        public CategoryViewModel()
        {
            CategoryService modelService = new CategoryService();

            categorys = new ObservableCollection<CategoryDto>(
                        modelService.GetCategoryDTO());
            EditCommand = new RelayCommand(x => Edit((CategoryDto)x));
            DeleteCommand = new RelayCommand(x => Delete((CategoryDto)x));
            AddCommand = new RelayCommand(x => Add());
        }
        public void Delete(CategoryDto category)
        {
            CategoryService service = new CategoryService();

            service.Delete(category.Id);

            categorys.Remove(category);
        }

        public void Edit(CategoryDto category)
        {
            var dialog = new edit(category);

            if (dialog.ShowDialog() == true)
            {
                CategoryService service = new CategoryService();
                service.Edit(category);

                OnPropertyChanged(nameof(category));
            }

        }
        public void Add()
        {
            var category = new CategoryDto();   // object mới

            var dialog = new edit(category);

            if (dialog.ShowDialog() == true)
            {
                CategoryService service = new CategoryService();

                int newId = service.Add(category);

                category.Id = newId;

                categorys.Add(category);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}