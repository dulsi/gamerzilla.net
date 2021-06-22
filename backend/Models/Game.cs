using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace backend.Models
{
    [Table("Game")]
    public class Game
    {
        public Game()
        {
            Trophies = new List<Trophy>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string ShortName { get; set; }

        [Required]
        [StringLength(128)]
        public string GameName { get; set; }

        public int VersionNum { get; set; }

        public virtual IList<Trophy> Trophies { get; set; }
        
        public void Import(GameApi1 g)
        {
            ShortName = g.shortname;
            GameName = g.name;
            VersionNum = g.version;
            foreach (TrophyApi1 t1 in g.trophy)
            {
                bool found = false;
                foreach (Trophy t in Trophies)
                {
                    if (t1.trophy_name == t.TrophyName)
                    {
                        found = true;
                        t.Import(t1);
                    }
                }
                if (!found)
                {
                    Trophy newTrophy = new Trophy();
                    newTrophy.Import(t1);
                    Trophies.Add(newTrophy);
                }
            }
        }

        public void Export(GameApi1 g)
        {
            g.shortname = ShortName;
            g.name = GameName;
            g.version = VersionNum;
            foreach (Trophy t in Trophies)
            {
                TrophyApi1 t1 = new TrophyApi1();
                t.Export(t1);
                g.trophy.Add(t1);
            }
        }
    }

    public class GameSummary
    {
        public string shortname { get; set; }

        public string name { get; set; }

        public int earned { get; set; }

        public int total { get; set; }
    }
    
    public class GameApi1
    {
        public GameApi1()
        {
            trophy = new List<TrophyApi1>();
        }

        public string shortname { get; set; }

        public string name { get; set; }

        public int version { get; set; }
        
        public IList<TrophyApi1> trophy { get; set; }
    }
}
