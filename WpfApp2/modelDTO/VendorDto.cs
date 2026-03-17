using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.modelDTO
{
    internal class VendorDto
    {
        public int Id { get; set; }


        public string VendorName { get; set; }


        public bool IsActive { get; set; }
    }
}
