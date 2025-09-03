using System.ComponentModel.DataAnnotations;
using System;

namespace Jeu_D_echec.Models
{
    public class BoardState
    {
        public int Id { get; set; }
        
        [Required]
        public int SavedGameId { get; set; }
        
        [Required]
        public int Row { get; set; }
        
        [Required]
        public int Column { get; set; }
        
        [MaxLength(20)]
        public string? PieceType { get; set; }
        
        [MaxLength(10)]
        public string? PieceColor { get; set; }
        
        public bool HasMoved { get; set; }
        
        // Navigation property
        public SavedGame SavedGame { get; set; } = null!;
        
        // Computed properties
        public Position Position => new Position(Row, Column);
        public ChessPiece? Piece => 
            PieceType != null && PieceColor != null ? 
            new ChessPiece(Enum.Parse<PieceType>(PieceType), Enum.Parse<PieceColor>(PieceColor)) { HasMoved = HasMoved } : 
            null;
    }
}
