using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace backend.Models
{
    public partial class GamerzillaContext : DbContext
    {
        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<Trophy> Trophies { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<UserStat> UserStats { get; set; }

        public GamerzillaContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .HasMany(e => e.Trophies);
            modelBuilder.Entity<Trophy>()
                .HasOne(e => e.Stat);
        }
    }
}
