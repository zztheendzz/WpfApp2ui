using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{
    [Table("Model")]
 public   class Model
    {
        [DisplayName("Id")]
        [Key]   
        public int Id { get; set; }

        [DisplayName("Model Code")]
        [Column("ModelCode")]
        public string ModelCode { get; set; }

        [DisplayName("Model Name")]
        [Column("ModelName")]
        public string ModelName { get; set; }

        [DisplayName("Brand")]
        [Column("BrandId")]
        public int BrandId { get; set; }

        [DisplayName("Active")]
        [Column("IsActive")]
        public int IsActive { get; set; }
    }
}
