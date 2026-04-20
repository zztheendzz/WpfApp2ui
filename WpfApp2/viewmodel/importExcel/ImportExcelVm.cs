using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.Services; // Giả định DatabaseLockedException nằm ở đây
using WpfApp2.Services.improtExcel;
using WpfApp2.Services.exception;

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
            set { _filePath = value; OnPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        private List<string> _sheetList;
        public List<string> SheetList
        {
            get => _sheetList;
            set { _sheetList = value; OnPropertyChanged(); }
        }

        private string _selectedSheet;
        public string SelectedSheet
        {
            get => _selectedSheet;
            set { _selectedSheet = value; OnPropertyChanged(); }
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
                Message = "";

                try
                {
                    var sheets = _purchaseExcelSv.GetSheetNames(FilePath);
                    SheetList = sheets;

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
                Message = "Đang xử lý dữ liệu..."; // Thông báo trạng thái chờ

                // Thực hiện gọi Service để Insert
                _purchaseExcelSv.inSertData(FilePath, SelectedSheet);

                //Message = "Import thành công!";
                //MessageBox.Show("Dữ liệu đã được nhập thành công vào hệ thống.", "Hoàn tất");
            }
            catch (DatabaseLockedException)
            {
                Message = "Lỗi: Cơ sở dữ liệu đang bị khóa.";
                MessageBox.Show("Cơ sở dữ liệu đang bận xử lý một tác vụ khác (có thể là Backup hoặc một lượt Import khác). Vui lòng đợi vài giây rồi thử lại.", "Hệ thống bận");
            }
            catch (Exception ex)
            {
                Message = "Lỗi: " + ex.Message;
                MessageBox.Show("Có lỗi xảy ra trong quá trình Import: " + ex.Message, "Lỗi");
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