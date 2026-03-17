using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.modelDto
{
    internal class BrandDto
    {

        public int Id { get; set; }
        public string BrandName { get; set; }
        public bool IsActive { get; set; }
    }
}
