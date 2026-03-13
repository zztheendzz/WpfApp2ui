using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{
    [Table("Model")]
 public   class Model
    {
        [Key]   
        public int Id { get; set; }
        [Column("ModelCode")]
        public string ModelCode { get; set; }
        [Column("ModelName")]
        public string ModelName { get; set; }
        [Column("BrandId")]
        public int BrandId { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
    }
}
