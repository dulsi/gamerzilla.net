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
        public virtual DbSet<UserInfo> Users { get; set; }        
        public virtual DbSet<UserStat> UserStats { get; set; }

        public GamerzillaContext(DbContextOptions<GamerzillaContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Game");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ShortName).IsUnique();
            });

            
            modelBuilder.Entity<Trophy>(entity =>
            {
                entity.ToTable("Trophy");
                entity.HasKey(e => e.Id);

                

                
                entity.HasOne(d => d.game)
                      .WithMany(p => p.Trophies)
                      .HasForeignKey(d => d.GameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            
            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("Image");
                entity.HasKey(e => e.Id);
            });

            
            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(e => e.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();

            });

            
            modelBuilder.Entity<UserStat>(entity =>
            {
                entity.ToTable("UserStat");
                entity.HasKey(e => e.Id);

                
                entity.HasIndex(e => new { e.UserId, e.GameId, e.TrophyId }).IsUnique();

                
                entity.HasOne<UserInfo>()
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                
                entity.HasOne(d => d.game)
                      .WithMany()
                      .HasForeignKey(d => d.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                
                
                entity.HasOne(d => d.trophy)
                      .WithMany(p => p.Stat)  
                      .HasForeignKey(d => d.TrophyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}