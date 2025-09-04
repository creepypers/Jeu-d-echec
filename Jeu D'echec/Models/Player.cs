using System;
using System.ComponentModel.DataAnnotations;

namespace Jeu_D_echec.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        // ELO Rating
        public int Rating { get; set; } = 1200; // Starting ELO

        // Statistics
        public int TotalGamesPlayed { get; set; } = 0;
        public int TotalWins { get; set; } = 0;
        public int TotalLosses { get; set; } = 0;
        public int TotalDraws { get; set; } = 0;

        // Timestamps
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastPlayed { get; set; } = DateTime.Now;

        // Computed properties
        public double WinRate => TotalGamesPlayed > 0 ? (double)TotalWins / TotalGamesPlayed * 100 : 0;
        public double TotalPoints => TotalWins * 1.0 + TotalDraws * 0.5; // 1 point for win, 0.5 for draw

        public void UpdateRating(int newRating)
        {
            Rating = newRating;
        }

        public void UpdateStats(bool won, bool draw)
        {
            TotalGamesPlayed++;
            if (won)
                TotalWins++;
            else if (draw)
                TotalDraws++;
            else
                TotalLosses++;
            
            LastPlayed = DateTime.Now;
        }
    }
}
