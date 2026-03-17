using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO
{
    public class PurchaseDto
    {
        public int Id { get; set; }


        public int ModelId { get; set; }
        public string ModelName { get; set; }

        public int VendorId { get; set; }
        public string VendorName { get; set; }


        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }

        public bool IsActive { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string CurrencyCode { get; set; }

        public string PurchaseDate { get; set; }

        public string CreateAt { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }

        public string Note { get; set; }
    }
}
