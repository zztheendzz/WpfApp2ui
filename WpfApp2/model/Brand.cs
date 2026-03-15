using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{

    [Table("Brand")]
  public  class Brand
    {
            [Key]
            public int Id { get; set; }
            [Column("BrandName")]
            public string BrandName { get; set; }
            [Column("IsActive")]
            public int IsActive { get; set; }

        }
}
