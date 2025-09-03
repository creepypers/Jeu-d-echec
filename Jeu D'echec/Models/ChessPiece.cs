using System;

namespace Jeu_D_echec.Models
{
    public class ChessPiece
    {
        public PieceType Type { get; }
        public PieceColor Color { get; }
        public bool HasMoved { get; set; }

        public ChessPiece(PieceType type, PieceColor color)
        {
            Type = type;
            Color = color;
            HasMoved = false;
        }

        public string UnicodeSymbol => Type switch
        {
            PieceType.Pawn => Color == PieceColor.White ? "♙" : "♟",
            PieceType.Rook => Color == PieceColor.White ? "♖" : "♜",
            PieceType.Knight => Color == PieceColor.White ? "♘" : "♞",
            PieceType.Bishop => Color == PieceColor.White ? "♗" : "♝",
            PieceType.Queen => Color == PieceColor.White ? "♕" : "♛",
            PieceType.King => Color == PieceColor.White ? "♔" : "♚",
            _ => "?"
        };

        public ChessPiece Clone()
        {
            return new ChessPiece(Type, Color) { HasMoved = HasMoved };
        }

        public override string ToString()
        {
            return $"{Color} {Type}";
        }
    }
}
