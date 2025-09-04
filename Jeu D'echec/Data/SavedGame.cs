using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using Jeu_D_echec.Data;

namespace Jeu_D_echec.Models
{
    public class SavedGame
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Player1Name { get; set; } = "";
        
        [Required]
        [MaxLength(100)]
        public string Player2Name { get; set; } = "";
        
        [Required]
        public DateTime CreatedDate { get; set; }
        
        [Required]
        public DateTime LastPlayed { get; set; }
        
        [Required]
        public int MoveCount { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string CurrentPlayer { get; set; } = "White";
        
        [Required]
        [MaxLength(50)]
        public string GameState { get; set; } = "Playing";
        
        // Navigation properties
        public List<SavedChessMove> Moves { get; set; } = new List<SavedChessMove>();
        public List<BoardState> BoardStates { get; set; } = new List<BoardState>();
        
        // Computed properties
        public string GameTitle => $"{Player1Name} vs {Player2Name}";
        public string PlayerInfo => $"Blanc: {Player1Name} | Noir: {Player2Name}";
        public string LastPlayedText => $"Dernière partie: {LastPlayed:dd/MM/yyyy HH:mm}";
    }
}
