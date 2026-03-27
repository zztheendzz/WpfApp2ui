using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.modelDTO
{
    public class UserDto
    {
        public int Id { get; set; }

        public string Password{ get; set; }

        public string UserName { get; set; }

        //public string Role { get; set; }
        public int Role { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; }
        public string RoleName
        {
            get => ((UserRole)Role).ToString();
            set { } // ❌ hack, không nên
        }
    }
}
