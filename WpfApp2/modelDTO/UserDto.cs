using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.modelDTO
{
    public class UserDto
    {
        public int Id { get; set; }

        public string LoginId { get; set; }

        public string PasswordHash { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        public string CreatedAt { get; set; }
    }
}
