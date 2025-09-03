using System.ComponentModel.DataAnnotations;
using System;

namespace Jeu_D_echec.Models
{
    public class SavedChessMove
    {
        public int Id { get; set; }
        
        [Required]
        public int SavedGameId { get; set; }
        
        [Required]
        public int FromRow { get; set; }
        
        [Required]
        public int FromColumn { get; set; }
        
        [Required]
        public int ToRow { get; set; }
        
        [Required]
        public int ToColumn { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string PieceType { get; set; } = "";
        
        [Required]
        [MaxLength(10)]
        public string PieceColor { get; set; } = "";
        
        [MaxLength(20)]
        public string? CapturedPieceType { get; set; }
        
        [MaxLength(10)]
        public string? CapturedPieceColor { get; set; }
        
        [MaxLength(20)]
        public string? PromotionType { get; set; }
        
        [Required]
        public int MoveNumber { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        // Navigation property
        public SavedGame SavedGame { get; set; } = null!;
        
        // Computed properties
        public Position FromPosition => new Position(FromRow, FromColumn);
        public Position ToPosition => new Position(ToRow, ToColumn);
    }
}
