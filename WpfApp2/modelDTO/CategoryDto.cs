using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.modelDTO
{
    internal class CategoryDto
    {
        public int  Id { get; set; }
        public string CategoryName { get; set; }
        public int ParentId { get; set; }
        public string Parentname { get; set; }
        public bool IsActive { get; set; }
    }
}
