using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{
    [Table("Currency")]
    public class Currency
    {
        [Key]
        public string Code { get; set; }

        [Column("Name")]
        public string Name { get; set; }
        [Column("Symbol")]
        public string Symbol { get; set; }
    }
}
