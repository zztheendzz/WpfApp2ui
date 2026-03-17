using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.modelDTO
{
    public class ModelDto
    {
        public int Id { get; set; }


        public string ModelCode { get; set; }

        public string ModelName { get; set; }

        public string BrandId { get; set; }

        public bool IsActive { get; set; }
        public string BrandName { get; set; }
    }
}
