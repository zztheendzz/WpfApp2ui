using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{
    [Table("Vendor")]
    public class Vendor
    {
        [Key]
        public int Id { get; set; }

        [Column("VendorName")]
        public string? VendorName { get; set; }

        [Column("IsActive")]
        public int IsActive { get; set; }
    }
}
