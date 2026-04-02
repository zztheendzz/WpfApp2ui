using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp2.command;
using WpfApp2.Services.improtExcel;

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

        // --- MỚI: Danh sách các Sheet ---
        private List<string> _sheetList;
        public List<string> SheetList
        {
            get => _sheetList;
            set
            {
                _sheetList = value;
                OnPropertyChanged();
            }
        }

        // --- MỚI: Sheet được chọn ---
        private string _selectedSheet;
        public string SelectedSheet
        {
            get => _selectedSheet;
            set
            {
                _selectedSheet = value;
                OnPropertyChanged();
            }
        }

        // ====== COMMAND ======
        public ICommand BrowseFileCommand { get; set; }
        public ICommand InsertCommand { get; set; }

        // ====== CONSTRUCTOR ======
        public ImportExcelVm()
        {
            BrowseFileCommand = new RelayCommand(_ => BrowseFile());
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
                Message = ""; // Xóa thông báo cũ

                try
                {
                    // Tự động load danh sách Sheet khi chọn file xong
                    var sheets = _purchaseExcelSv.GetSheetNames(FilePath);
                    SheetList = sheets;

                    // Mặc định chọn sheet đầu tiên
                    if (SheetList != null && SheetList.Count > 0)
                    {
                        SelectedSheet = SheetList.FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    Message = "Không thể đọc danh sách Sheet: " + ex.Message;
                }
            }
        }

        private void InsertData()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                Message = "Vui lòng chọn file!";
                return;
            }

            if (string.IsNullOrEmpty(SelectedSheet))
            {
                Message = "Vui lòng chọn một Sheet để import!";
                return;
            }

            try
            {
                // Truyền cả FilePath và SelectedSheet vào Service
                _purchaseExcelSv.inSertData(FilePath, SelectedSheet);
                Message = "Import thành công!";
            }
            catch (Exception ex)
            {
                Message = "Lỗi: " + ex.Message;
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