using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace backend.Models
{
    [Table("User")]
    public class UserInfo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string UserName { get; set; }

        [Required]
        [StringLength(128)]
        public string Password { get; set; }

        public bool Admin { get; set; }

        public bool Visible { get; set; }

        public bool Approved { get; set; }
    }
}
