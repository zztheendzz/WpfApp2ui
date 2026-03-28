using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpfApp2.model
{
    [Table("User")]
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Column("PasswordHash")]
        public string Password { get; set; }
        [Column("UserName")]
        public string UserName { get; set; }
        [Column("Role")]
        public int Role { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
        [Column("CreatedAt")]
        public string CreatedAt { get; set; }
    }
}
