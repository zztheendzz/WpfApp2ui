using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.modelDTO
{
    public class EquipmentDto
    {
        public int Id { get; set; }

        public string EquipmentName { get; set; }

        public bool IsActive { get; set; }
    }
}
