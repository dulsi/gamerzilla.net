using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace backend.Models
{
    [Table("Image")]
    public class Image
    {
        public int Id { get; set; }

        public int GameId { get; set; }

        public int TrophyId { get; set; }
        
        public bool Achieved { get; set; }

        public byte[] data { get; set; }
    }
}
