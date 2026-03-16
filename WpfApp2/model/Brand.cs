using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{

    [Table("Brand")]
  public  class Brand
    {
            [DisplayName("Id")]
            [Key]
            public int Id { get; set; }

            [DisplayName("Brand")]
            [Column("BrandName")]
            public string BrandName { get; set; }

            [DisplayName("Active")]
            [Column("IsActive")]
            public int IsActive { get; set; }

        }
}
