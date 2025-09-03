using System;
using System.Collections.Generic;
using System.Linq;

namespace Jeu_D_echec.Models
{
    public class ChessBoard
    {
        private readonly ChessPiece?[,] _board = new ChessPiece?[8, 8];

        public ChessPiece? this[Position position]
        {
            get => position.IsValid ? _board[position.Row, position.Column] : null;
            set
            {
                if (position.IsValid)
                {
                    _board[position.Row, position.Column] = value;
                }
            }
        }

        public ChessBoard()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Place pawns
            for (int col = 0; col < 8; col++)
            {
                _board[1, col] = new ChessPiece(PieceType.Pawn, PieceColor.Black);
                _board[6, col] = new ChessPiece(PieceType.Pawn, PieceColor.White);
            }

            // Place other pieces
            PieceType[] pieceOrder = new[] { PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook };
            
            for (int col = 0; col < 8; col++)
            {
                _board[0, col] = new ChessPiece(pieceOrder[col], PieceColor.Black);
                _board[7, col] = new ChessPiece(pieceOrder[col], PieceColor.White);
            }
        }

        public ChessBoard Clone()
        {
            var newBoard = new ChessBoard();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = _board[row, col];
                    newBoard._board[row, col] = piece?.Clone();
                }
            }
            return newBoard;
        }

        public bool IsInCheck(PieceColor color, bool ignoreKingSafetyCheck = false)
        {
            var kingPosition = FindKing(color);
            if (!kingPosition.HasValue) return false;

            return IsPositionUnderAttack(kingPosition.Value, color == PieceColor.White ? PieceColor.Black : PieceColor.White, ignoreKingSafetyCheck);
        }

        public bool IsCheckmate(PieceColor color)
        {
            if (!IsInCheck(color)) return false;

            // Check if any move can get out of check
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var position = new Position(row, col);
                    var piece = this[position];
                    
                    if (piece?.Color == color)
                    {
                        var moves = GetValidMoves(position);
                        foreach (var move in moves)
                        {
                            var testBoard = Clone();
                            testBoard.MakeMove(position, move);
                            if (!testBoard.IsInCheck(color))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool IsStalemate(PieceColor color)
        {
            if (IsInCheck(color)) return false;

            // Check if any move is possible
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var position = new Position(row, col);
                    var piece = this[position];
                    
                    if (piece?.Color == color)
                    {
                        var moves = GetValidMoves(position);
                        foreach (var move in moves)
                        {
                            var testBoard = Clone();
                            testBoard.MakeMove(position, move);
                            if (!testBoard.IsInCheck(color))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private Position? FindKing(PieceColor color)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = _board[row, col];
                    if (piece?.Type == PieceType.King && piece.Color == color)
                    {
                        return new Position(row, col);
                    }
                }
            }
            return null;
        }

        public bool IsPositionUnderAttack(Position position, PieceColor attackingColor, bool ignoreKingSafetyCheck = false)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = _board[row, col];
                    if (piece?.Color == attackingColor)
                    {
                        // Get raw moves without king safety check to avoid recursion
                        var moves = GetRawMoves(new Position(row, col));
                        if (moves.Contains(position))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public List<Position> GetValidMoves(Position from, bool ignoreKingSafetyCheck = false)
        {
            var piece = this[from];
            if (piece == null) return new List<Position>();

            var moves = GetRawMoves(from);

            // Filter out moves that would put own king in check (unless we're ignoring king safety)
            if (!ignoreKingSafetyCheck)
            {
                return moves.Where(move => !WouldMovePutKingInCheck(from, move, piece.Color)).ToList();
            }
            
            return moves;
        }

        // Overloaded method to include en passant target
        public List<Position> GetValidMoves(Position from, Position? enPassantTarget, bool ignoreKingSafetyCheck = false)
        {
            var piece = this[from];
            if (piece == null) return new List<Position>();

            var moves = GetRawMoves(from, enPassantTarget);

            // Filter out moves that would put own king in check (unless we're ignoring king safety)
            if (!ignoreKingSafetyCheck)
            {
                return moves.Where(move => !WouldMovePutKingInCheck(from, move, piece.Color)).ToList();
            }
            
            return moves;
        }

        private List<Position> GetRawMoves(Position from)
        {
            var piece = this[from];
            if (piece == null) return new List<Position>();

            var moves = new List<Position>();

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    moves.AddRange(GetPawnMoves(from, piece.Color));
                    break;
                case PieceType.Rook:
                    moves.AddRange(GetRookMoves(from));
                    break;
                case PieceType.Knight:
                    moves.AddRange(GetKnightMoves(from));
                    break;
                case PieceType.Bishop:
                    moves.AddRange(GetBishopMoves(from));
                    break;
                case PieceType.Queen:
                    moves.AddRange(GetRookMoves(from));
                    moves.AddRange(GetBishopMoves(from));
                    break;
                case PieceType.King:
                    moves.AddRange(GetKingMoves(from, piece.Color, true)); // Always ignore king safety for raw moves
                    break;
            }

            return moves;
        }

        // Overloaded method to include en passant target
        private List<Position> GetRawMoves(Position from, Position? enPassantTarget)
        {
            var piece = this[from];
            if (piece == null) return new List<Position>();

            var moves = new List<Position>();

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    moves.AddRange(GetPawnMoves(from, piece.Color, enPassantTarget));
                    break;
                case PieceType.Rook:
                    moves.AddRange(GetRookMoves(from));
                    break;
                case PieceType.Knight:
                    moves.AddRange(GetKnightMoves(from));
                    break;
                case PieceType.Bishop:
                    moves.AddRange(GetBishopMoves(from));
                    break;
                case PieceType.Queen:
                    moves.AddRange(GetRookMoves(from));
                    moves.AddRange(GetBishopMoves(from));
                    break;
                case PieceType.King:
                    moves.AddRange(GetKingMoves(from, piece.Color, true)); // Always ignore king safety for raw moves
                    break;
            }

            return moves;
        }

        private List<Position> GetPawnMoves(Position from, PieceColor color)
        {
            var moves = new List<Position>();
            var direction = color == PieceColor.White ? -1 : 1;
            var startRow = color == PieceColor.White ? 6 : 1;

            // Forward move
            var forward = new Position(from.Row + direction, from.Column);
            if (forward.IsValid && this[forward] == null)
            {
                moves.Add(forward);

                // Double move from starting position
                if (from.Row == startRow)
                {
                    var doubleForward = new Position(from.Row + 2 * direction, from.Column);
                    if (doubleForward.IsValid && this[doubleForward] == null)
                    {
                        moves.Add(doubleForward);
                    }
                }
            }

            // Diagonal captures
            var leftCapture = new Position(from.Row + direction, from.Column - 1);
            var rightCapture = new Position(from.Row + direction, from.Column + 1);

            if (leftCapture.IsValid && this[leftCapture]?.Color != color)
            {
                moves.Add(leftCapture);
            }
            
            if (rightCapture.IsValid && this[rightCapture]?.Color != color)
            {
                moves.Add(rightCapture);
            }

            return moves;
        }

        // Overloaded method to include en passant target
        private List<Position> GetPawnMoves(Position from, PieceColor color, Position? enPassantTarget)
        {
            var moves = GetPawnMoves(from, color);
            
            // Add en passant capture if available
            if (enPassantTarget.HasValue)
            {
                var direction = color == PieceColor.White ? -1 : 1;
                var leftEnPassant = new Position(from.Row + direction, from.Column - 1);
                var rightEnPassant = new Position(from.Row + direction, from.Column + 1);
                
                if (leftEnPassant == enPassantTarget.Value)
                {
                    moves.Add(leftEnPassant);
                }
                if (rightEnPassant == enPassantTarget.Value)
                {
                    moves.Add(rightEnPassant);
                }
            }
            
            return moves;
        }

        private List<Position> GetRookMoves(Position from)
        {
            var moves = new List<Position>();
            var directions = new[] { new Position(0, 1), new Position(0, -1), new Position(1, 0), new Position(-1, 0) };

            foreach (var direction in directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    var target = from + direction * i;
                    if (!target.IsValid) break;

                    var piece = this[target];
                    if (piece == null)
                    {
                        moves.Add(target);
                    }
                    else
                    {
                        if (piece.Color != this[from]?.Color)
                        {
                            moves.Add(target);
                        }
                        break;
                    }
                }
            }

            return moves;
        }

        private List<Position> GetKnightMoves(Position from)
        {
            var moves = new List<Position>();
            var knightMoves = new[]
            {
                new Position(-2, -1), new Position(-2, 1), new Position(-1, -2), new Position(-1, 2),
                new Position(1, -2), new Position(1, 2), new Position(2, -1), new Position(2, 1)
            };

            foreach (var move in knightMoves)
            {
                var target = from + move;
                if (target.IsValid)
                {
                    var piece = this[target];
                    if (piece?.Color != this[from]?.Color)
                    {
                        moves.Add(target);
                    }
                }
            }

            return moves;
        }

        private List<Position> GetBishopMoves(Position from)
        {
            var moves = new List<Position>();
            var directions = new[] { new Position(1, 1), new Position(1, -1), new Position(-1, 1), new Position(-1, -1) };

            foreach (var direction in directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    var target = from + direction * i;
                    if (!target.IsValid) break;

                    var piece = this[target];
                    if (piece == null)
                    {
                        moves.Add(target);
                    }
                    else
                    {
                        if (piece.Color != this[from]?.Color)
                        {
                            moves.Add(target);
                        }
                        break;
                    }
                }
            }

            return moves;
        }

        private List<Position> GetKingMoves(Position from, PieceColor color, bool ignoreKingSafetyCheck = false)
        {
            var moves = new List<Position>();
            var directions = new[]
            {
                new Position(-1, -1), new Position(-1, 0), new Position(-1, 1),
                new Position(0, -1), new Position(0, 1),
                new Position(1, -1), new Position(1, 0), new Position(1, 1)
            };

            foreach (var direction in directions)
            {
                var target = from + direction;
                if (target.IsValid)
                {
                    var piece = this[target];
                    if (piece?.Color != color)
                    {
                        moves.Add(target);
                    }
                }
            }

            // Castling - only check if we're not ignoring king safety (to prevent recursion)
            if (!ignoreKingSafetyCheck && !this[from]?.HasMoved == true && !IsInCheck(color, true))
            {
                // Kingside castling
                if (this[new Position(from.Row, 7)]?.Type == PieceType.Rook && 
                    !this[new Position(from.Row, 7)]?.HasMoved == true &&
                    this[new Position(from.Row, 5)] == null &&
                    this[new Position(from.Row, 6)] == null &&
                    !IsPositionUnderAttack(new Position(from.Row, 5), color == PieceColor.White ? PieceColor.Black : PieceColor.White, true) &&
                    !IsPositionUnderAttack(new Position(from.Row, 6), color == PieceColor.White ? PieceColor.Black : PieceColor.White, true))
                {
                    moves.Add(new Position(from.Row, 6));
                }

                // Queenside castling
                if (this[new Position(from.Row, 0)]?.Type == PieceType.Rook && 
                    !this[new Position(from.Row, 0)]?.HasMoved == true &&
                    this[new Position(from.Row, 1)] == null &&
                    this[new Position(from.Row, 2)] == null &&
                    this[new Position(from.Row, 3)] == null &&
                    !IsPositionUnderAttack(new Position(from.Row, 2), color == PieceColor.White ? PieceColor.Black : PieceColor.White, true) &&
                    !IsPositionUnderAttack(new Position(from.Row, 3), color == PieceColor.White ? PieceColor.Black : PieceColor.White, true))
                {
                    moves.Add(new Position(from.Row, 2));
                }
            }

            return moves;
        }

        private bool WouldMovePutKingInCheck(Position from, Position to, PieceColor color)
        {
            var testBoard = Clone();
            testBoard.MakeMove(from, to);
            return testBoard.IsInCheck(color, true); // Use ignoreKingSafetyCheck = true to prevent recursion
        }

        public void MakeMove(Position from, Position to)
        {
            MakeMove(from, to, PieceType.Queen); // Default promotion to Queen
        }

        public void MakeMove(Position from, Position to, PieceType promotionType)
        {
            var piece = this[from];
            if (piece == null) return;

            // Handle castling
            if (piece.Type == PieceType.King && Math.Abs(to.Column - from.Column) == 2)
            {
                // Move the rook
                if (to.Column > from.Column) // Kingside
                {
                    var rook = this[new Position(from.Row, 7)];
                    this[new Position(from.Row, 7)] = null;
                    this[new Position(from.Row, 5)] = rook;
                    rook!.HasMoved = true;
                }
                else // Queenside
                {
                    var rook = this[new Position(from.Row, 0)];
                    this[new Position(from.Row, 0)] = null;
                    this[new Position(from.Row, 3)] = rook;
                    rook!.HasMoved = true;
                }
            }

            // Move the piece
            this[from] = null;
            this[to] = piece;
            piece.HasMoved = true;

            // Handle pawn promotion
            if (piece.Type == PieceType.Pawn && (to.Row == 0 || to.Row == 7))
            {
                this[to] = new ChessPiece(promotionType, piece.Color);
            }
        }
    }
}
