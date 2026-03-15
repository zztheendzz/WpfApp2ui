using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.Services;

namespace WpfApp2.model
{
    [Table("PurchaseHistory")]
    public class PurchaseHistory
    {
        [Key]
        public int Id { get; set; }

        [Column("ModelId")]
        public int ModelId { get; set; }
        [Column("VendorId")]
        public int VendorId { get; set; }
        [Column("EquipmentId")]
        public int EquipmentId { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
        [Column("Quantity")]
        public int Quantity { get; set; }
        [Column("UnitPrice")]
        public double UnitPrice { get; set; }
        [Column("TotalPrice")]
        public double TotalPrice { get; set; }
        [Column("CurrencyCode")]
        public string CurrencyCode { get; set; }
        [Column("PurchaseDate")]
        public string PurchaseDate { get; set; }
        [Column("CreatAt")]
        public string CreatAt { get; set; }
        [Column("UserId")]
        public int UserId { get; set; }
        [Column("Note")]
        public string Note { get; set; }

    }
}
