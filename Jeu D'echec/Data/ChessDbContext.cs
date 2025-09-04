using Microsoft.EntityFrameworkCore;
using Jeu_D_echec.Models;
using System;
using System.IO;

namespace Jeu_D_echec.Data
{
    public class ChessDbContext : DbContext
    {
        public DbSet<SavedGame> SavedGames { get; set; }
        public DbSet<SavedChessMove> ChessMoves { get; set; }
        public DbSet<BoardState> BoardStates { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<GameResultInfo> GameResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use SQL Server LocalDB for development
            // For production, you would typically use a connection string from configuration
            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=ChessGameDB;Trusted_Connection=true;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure SavedGame entity
            modelBuilder.Entity<SavedGame>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Player1Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Player2Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.GameState).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CurrentPlayer).IsRequired().HasMaxLength(10);
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.LastPlayed).IsRequired();
                entity.Property(e => e.MoveCount).IsRequired();
                
                // Configure relationship with moves
                entity.HasMany(e => e.Moves)
                      .WithOne(m => m.SavedGame)
                      .HasForeignKey(m => m.SavedGameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SavedChessMove entity
            modelBuilder.Entity<SavedChessMove>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FromRow).IsRequired();
                entity.Property(e => e.FromColumn).IsRequired();
                entity.Property(e => e.ToRow).IsRequired();
                entity.Property(e => e.ToColumn).IsRequired();
                entity.Property(e => e.PieceType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PieceColor).IsRequired().HasMaxLength(10);
                entity.Property(e => e.MoveNumber).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                
                // Configure relationship with saved game
                entity.HasOne(m => m.SavedGame)
                      .WithMany(s => s.Moves)
                      .HasForeignKey(m => m.SavedGameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BoardState entity
            modelBuilder.Entity<BoardState>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Row).IsRequired();
                entity.Property(e => e.Column).IsRequired();
                entity.Property(e => e.PieceType).HasMaxLength(20);
                entity.Property(e => e.PieceColor).HasMaxLength(10);
                entity.Property(e => e.HasMoved).IsRequired();
                
                // Configure relationship with saved game
                entity.HasOne(b => b.SavedGame)
                      .WithMany(s => s.BoardStates)
                      .HasForeignKey(b => b.SavedGameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Player entity
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.LastPlayed).IsRequired();
                
                // Create unique index on email
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure GameResultInfo entity
            modelBuilder.Entity<GameResultInfo>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Result).IsRequired().HasConversion<string>();
                entity.Property(e => e.WhiteRatingBefore).IsRequired();
                entity.Property(e => e.WhiteRatingAfter).IsRequired();
                entity.Property(e => e.BlackRatingBefore).IsRequired();
                entity.Property(e => e.BlackRatingAfter).IsRequired();
                entity.Property(e => e.MoveCount).IsRequired();
                entity.Property(e => e.GameDuration).IsRequired();
                entity.Property(e => e.PlayedDate).IsRequired();
                
                // Configure relationships with players
                entity.HasOne(e => e.WhitePlayer)
                      .WithMany()
                      .HasForeignKey(e => e.WhitePlayerId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.BlackPlayer)
                      .WithMany()
                      .HasForeignKey(e => e.BlackPlayerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
