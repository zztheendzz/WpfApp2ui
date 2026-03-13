using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{


    [Table("Category")]
    class Category
{
    [Key]
    public int Id { get; set; }
    [Column("CategoryName")]
    public string CategoryName { get; set; }
    [Column("IsActive")]
    public int IsActive { get; set; }

    [Column("ParentId")]
    public int ParentId { get; set; }

    }
}