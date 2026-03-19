using Dapper;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model.modelImportExcel;
using WpfApp2.Services;

namespace WpfApp2.viewmodel.importExcel
{
    public class ImportExcelVm /*: INotifyPropertyChanged*/
    {
        //public ObservableCollection<ImportPurchase> Rows { get; set; } = new();

        //public ICommand LoadFileCommand => new RelayCommand(LoadFile);
        //public ICommand ImportCommand => new RelayCommand(Import);

        //private string _filePath;

        //#region LOAD FILE

        //private void LoadFile(object obj)
        //{
        //    var dialog = new Microsoft.Win32.OpenFileDialog
        //    {
        //        Filter = "Excel Files|*.xlsx"
        //    };

        //    if (dialog.ShowDialog() != true) return;

        //    _filePath = dialog.FileName;
        //    ReadExcel();
        //}

        //#endregion

        //#region READ EXCEL

        //private void ReadExcel()
        //{
        //    Rows.Clear();

        //    using var package = new ExcelPackage(new FileInfo(_filePath));
        //    var ws = package.Workbook.Worksheets[0];

        //    int startRow = 5; // 🔥 quan trọng
        //    int rowCount = ws.Dimension.Rows;

        //    for (int i = startRow; i <= rowCount; i++)
        //    {
        //        // skip dòng trống
        //        if (string.IsNullOrWhiteSpace(ws.Cells[i, 2].Text))
        //            continue;

        //        var row = new ImportEquipmentRow
        //        {
        //            RowNumber = i,

        //            Name = ws.Cells[i, 2].Text,
        //            Brand = ws.Cells[i, 3].Text,
        //            Code = ws.Cells[i, 4].Text,

        //            Vendor = ws.Cells[i, 11].Text,
        //            Note = ws.Cells[i, 12].Text
        //        };

        //        // parse số an toàn
        //        if (int.TryParse(ws.Cells[i, 6].Text, out var qty))
        //            row.Quantity = qty;

        //        if (decimal.TryParse(ws.Cells[i, 7].Text, out var price))
        //            row.UnitPrice = price;

        //        if (int.TryParse(ws.Cells[i, 8].Text, out var mQty))
        //            row.MachineQty = mQty;

        //        if (int.TryParse(ws.Cells[i, 9].Text, out var totalQty))
        //            row.TotalQty = totalQty;

        //        if (decimal.TryParse(ws.Cells[i, 10].Text, out var totalPrice))
        //            row.TotalPrice = totalPrice;

        //        ValidateRow(row);

        //        Rows.Add(row);
        //    }
        //}

        //#endregion

        //#region VALIDATE

        //private void ValidateRow(ImportEquipmentRow row)
        //{
        //    var errors = new List<string>();

        //    if (string.IsNullOrWhiteSpace(row.Name))
        //        errors.Add("Thiếu tên");

        //    if (string.IsNullOrWhiteSpace(row.Brand))
        //        errors.Add("Thiếu brand");

        //    if (row.UnitPrice == null || row.UnitPrice <= 0)
        //        errors.Add("Sai đơn giá");

        //    if (row.Quantity == null || row.Quantity <= 0)
        //        errors.Add("Sai số lượng");

        //    if (string.IsNullOrWhiteSpace(row.Vendor))
        //        errors.Add("Thiếu vendor");

        //    row.Error = string.Join(" | ", errors);
        //}

        //#endregion

        //#region IMPORT

        //private void Import()
        //{
        //    var validRows = Rows.Where(x => x.IsValid).ToList();

        //    using var conn = _db.GetConnection();
        //    using var tran = conn.BeginTransaction();

        //    foreach (var r in validRows)
        //    {
        //        // 1. get or insert Brand
        //        var brandId = conn.ExecuteScalar<int>(@"
        //    INSERT INTO Brand(Name)
        //    SELECT @Name
        //    WHERE NOT EXISTS (SELECT 1 FROM Brand WHERE Name = @Name);

        //    SELECT Id FROM Brand WHERE Name = @Name;
        //", new { Name = r.Brand }, tran);

        //        // 2. get or insert Vendor
        //        var vendorId = conn.ExecuteScalar<int>(@"
        //    INSERT INTO Vendor(Name)
        //    SELECT @Name
        //    WHERE NOT EXISTS (SELECT 1 FROM Vendor WHERE Name = @Name);

        //    SELECT Id FROM Vendor WHERE Name = @Name;
        //", new { Name = r.Vendor }, tran);

        //        // 3. insert Product
        //        conn.Execute(@"
        //    INSERT INTO Product(Name, Code, BrandId)
        //    VALUES (@Name, @Code, @BrandId)
        //", new
        //        {
        //            r.Name,
        //            r.Code,
        //            BrandId = brandId
        //        }, tran);

        //        // 4. insert Purchase
        //        conn.Execute(@"
        //    INSERT INTO Purchase(ProductName, VendorId, Price, Quantity)
        //    VALUES (@Name, @VendorId, @Price, @Qty)
        //", new
        //        {
        //            r.Name,
        //            VendorId = vendorId,
        //            Price = r.UnitPrice,
        //            Qty = r.Quantity
        //        }, tran);
        //    }

        //    tran.Commit();
        //}
        //#endregion

        //#region INotifyPropertyChanged

        //public event PropertyChangedEventHandler PropertyChanged;

        //protected void OnPropertyChanged([CallerMemberName] string name = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        //}


    }
}