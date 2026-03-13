using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{
    [Table("Equipment")]
    public class Equipment
    {
        [Key]
        public int Id { get; set; }
        [Column("EquipmentName")]
        public string EquipmentName { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
    }
}
