using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp2.command;
using WpfApp2.Services.improtExcel;
using WpfApp2.Services.sessionService;

namespace WpfApp2.viewmodel.importExcel
{
    public class ImportExcelVm : INotifyPropertyChanged
    {
        // ====== SERVICE ======
        private readonly PurchaseExcelSv _purchaseExcelSv = new PurchaseExcelSv();

        // ====== PROPERTIES ======

        private string _filePath;
        public string FilePath
        {
           
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        // ====== COMMAND ======

        public ICommand BrowseFileCommand { get; set; }
        public ICommand InsertCommand { get; set; }

        // ====== CONSTRUCTOR ======
        public ImportExcelVm()
        {
            // chọn file
            BrowseFileCommand = new RelayCommand(_ => BrowseFile());

            // insert data (không có canExecute)
            InsertCommand = new RelayCommand(_ => InsertData());
        }

        // ====== METHODS ======

        private void BrowseFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }

        private void InsertData()
        {
            // tự check thay vì dùng canExecute
            if (string.IsNullOrEmpty(FilePath))
            {
                Message = "Vui lòng chọn file!";
                return;
            }

            try
            {
                _purchaseExcelSv.inSertData(FilePath);
                Message = "Import thành công!";
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        // ====== INotifyPropertyChanged ======

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}