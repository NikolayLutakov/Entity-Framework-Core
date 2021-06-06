using Microsoft.EntityFrameworkCore;
using P03_FootballBetting.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P03_FootballBetting.Data
{
    public class FootballBettingContext : DbContext
    {
        public FootballBettingContext()
        {
        }

        public FootballBettingContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Town> Towns { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<PlayerStatistic> PlayerStatistics { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=192.168.0.105,1433;Database=StudentSystem;User ID=nlyutakov;Password=A123456a;");
            }
        }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlayerStatistic>(
                playerStatistic =>
                {
                    playerStatistic.HasKey(ps => new { ps.PlayerId, ps.GameId });
                });

            modelBuilder.Entity<Game>(
                game =>
                {
                    game.HasKey(g => g.GameId);

                    game.HasOne(g => g.HomeTeam)
                        .WithMany(ht => ht.HomeGames)
                        .HasForeignKey(g => g.HomeTeamId)
                        .OnDelete(DeleteBehavior.Restrict);

                    game.HasOne(g => g.AwayTeam)
                        .WithMany(at => at.AwayGames)
                        .HasForeignKey(g => g.AwayTeamId)
                        .OnDelete(DeleteBehavior.Restrict);

                    game.Property(g => g.Result)
                        .IsRequired(true)
                        .IsUnicode(true)
                        .HasMaxLength(10);
                });

            modelBuilder.Entity<Team>(team =>
           {

               team
                   .HasOne(e => e.PrimaryKitColor)
                   .WithMany(c => c.PrimaryKitTeams)
                   .HasForeignKey(e => e.PrimaryKitColorId)
                   .OnDelete(DeleteBehavior.Restrict);

               team
                   .HasOne(e => e.SecondaryKitColor)
                   .WithMany(c => c.SecondaryKitTeams)
                   .HasForeignKey(e => e.SecondaryKitColorId)
                   .OnDelete(DeleteBehavior.Restrict);

               team
                   .HasOne(e => e.Town)
                   .WithMany(t => t.Teams)
                   .HasForeignKey(e => e.TownId)
                   .OnDelete(DeleteBehavior.Restrict);

           });
           
        }
    }
}

