using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace backend.Models
{
    [Table("UserStat")]
    public class UserStat
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int GameId { get; set; }
        public Game game { get; set; }

        public int TrophyId { get; set; }
        public Trophy trophy { get; set; }

        public bool Achieved { get; set; }

        public int Progress { get; set; }
    }
}
