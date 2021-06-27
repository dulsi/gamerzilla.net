using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace backend.Models
{
    [Table("Trophy")]
    public class Trophy
    {
        public int Id { get; set; }

        public int GameId { get; set; }
        public Game game { get; set; }

        [Required]
        [StringLength(128)]
        public string TrophyName { get; set; }

        [Required]
        [StringLength(255)]
        public string TrophyDescription { get; set; }

        public int MaxProgress { get; set; }
        
        public UserStat Stat { get; set; }

        public void Import(TrophyApi1 t)
        {
            TrophyName = t.trophy_name;
            TrophyDescription = t.trophy_desc;
            MaxProgress = Int32.Parse(t.max_progress);
        }

        public void Export(TrophyApi1 t)
        {
            t.trophy_name = TrophyName;
            t.trophy_desc = TrophyDescription;
            t.max_progress = MaxProgress.ToString();
            if (Stat != null)
            {
                t.achieved = (Stat.Achieved ? "1" : "0");
                t.progress = Stat.Progress.ToString();
            }
            else
            {
                t.achieved = "0";
                t.progress = "0";
            }
        }
    }
    
    public class TrophyApi1
    {
        public string trophy_name { get; set; }
        public string trophy_desc { get; set; }
        public string achieved { get; set; }
        public string progress { get; set; }
        public string max_progress { get; set; }
    }
}
