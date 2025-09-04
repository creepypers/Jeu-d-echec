using System;
using System.Collections.Generic;
using System.Linq;

namespace Jeu_D_echec.Models
{
    public enum GameState
    {
        Playing,
        Check,
        Checkmate,
        Stalemate,
        Draw
    }

    public class ChessGame
    {
        public Models.ChessBoard Board { get; private set; }
        public PieceColor CurrentPlayer { get; private set; }
        public GameState State { get; private set; }
        public Position? SelectedPosition { get; private set; }
        public List<Position> ValidMoves { get; private set; }
        public List<ChessMove> MoveHistory { get; private set; }
        public Position? EnPassantTarget { get; private set; } // Position where en passant capture can occur

        public event EventHandler<PieceSelectedEventArgs>? PieceSelected;
        public event EventHandler<MoveMadeEventArgs>? MoveMade;
        public event EventHandler<GameStateChangedEventArgs>? GameStateChanged;
        public event EventHandler<PawnPromotionEventArgs>? PawnPromotionRequired;
        public event EventHandler<DrawRequestedEventArgs>? DrawRequested;
        public event EventHandler<GameEndedEventArgs>? GameEnded;

        public ChessGame()
        {
            Board = new Models.ChessBoard();
            CurrentPlayer = PieceColor.White;
            State = GameState.Playing;
            ValidMoves = new List<Position>();
            MoveHistory = new List<ChessMove>();
        }

        public bool SelectPiece(Position position)
        {
            var piece = Board[position];
            // System.Diagnostics.Debug.WriteLine($"SelectPiece at {position.Row},{position.Column} - Piece: {piece?.Type} {piece?.Color}, CurrentPlayer: {CurrentPlayer}");
            
            if (piece?.Color != CurrentPlayer) 
            {
                // System.Diagnostics.Debug.WriteLine("SelectPiece failed - wrong color");
                return false;
            }

            SelectedPosition = position;
            ValidMoves = Board.GetValidMoves(position, EnPassantTarget);
            // System.Diagnostics.Debug.WriteLine($"SelectPiece successful - Valid moves: {ValidMoves.Count}");
            PieceSelected?.Invoke(this, new PieceSelectedEventArgs(position, ValidMoves));
            return true;
        }

        public bool MakeMove(Position to)
        {
            // System.Diagnostics.Debug.WriteLine($"MakeMove to {to.Row},{to.Column}");
            // System.Diagnostics.Debug.WriteLine($"SelectedPosition: {SelectedPosition?.Row},{SelectedPosition?.Column}");
            // System.Diagnostics.Debug.WriteLine($"ValidMoves contains target: {ValidMoves.Contains(to)}");
            
            if (!SelectedPosition.HasValue || !ValidMoves.Contains(to))
            {
                // System.Diagnostics.Debug.WriteLine("MakeMove failed - no selection or invalid move");
                return false;
            }

            var from = SelectedPosition.Value;
            var piece = Board[from];
            if (piece?.Color != CurrentPlayer) 
            {
                // System.Diagnostics.Debug.WriteLine("MakeMove failed - wrong color");
                return false;
            }

            // Check if move is valid
            if (!ValidMoves.Contains(to)) 
            {
                // System.Diagnostics.Debug.WriteLine("MakeMove failed - move not in valid moves");
                return false;
            }

            // Handle en passant capture
            var capturedPiece = Board[to];
            if (piece.Type == PieceType.Pawn && to == EnPassantTarget)
            {
                // Capture the pawn that moved two squares
                var capturedPawnRow = CurrentPlayer == PieceColor.White ? to.Row + 1 : to.Row - 1;
                capturedPiece = Board[new Position(capturedPawnRow, to.Column)];
                Board[new Position(capturedPawnRow, to.Column)] = null;
                // System.Diagnostics.Debug.WriteLine($"En passant capture at {capturedPawnRow},{to.Column}");
            }

            // Check for pawn promotion
            if (piece.Type == PieceType.Pawn && (to.Row == 0 || to.Row == 7))
            {
                // For pawn promotion, we need to return false and let the UI handle the promotion
                // The UI will call MakeMoveWithPromotion after getting the user's choice
                // System.Diagnostics.Debug.WriteLine("Pawn promotion required - returning false to let UI handle it");
                return false;
            }
            else
            {
                // Make the move normally
                // System.Diagnostics.Debug.WriteLine($"Making move from {from.Row},{from.Column} to {to.Row},{to.Column}");
                Board.MakeMove(from, to);
            }

            // Set en passant target for next move (if pawn moved two squares)
            EnPassantTarget = null;
            if (piece.Type == PieceType.Pawn && Math.Abs(to.Row - from.Row) == 2)
            {
                var enPassantRow = CurrentPlayer == PieceColor.White ? to.Row + 1 : to.Row - 1;
                EnPassantTarget = new Position(enPassantRow, to.Column);
                // System.Diagnostics.Debug.WriteLine($"En passant target set at {enPassantRow},{to.Column}");
            }

            // Record the move
            var move = new ChessMove(from, to, piece, capturedPiece);
            MoveHistory.Add(move);

            // Clear selection
            SelectedPosition = null;
            ValidMoves.Clear();

            // Switch players
            CurrentPlayer = CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            // System.Diagnostics.Debug.WriteLine($"Player switched to: {CurrentPlayer}");

            // Update game state
            UpdateGameState();

            MoveMade?.Invoke(this, new MoveMadeEventArgs(move));
            // System.Diagnostics.Debug.WriteLine("MakeMove successful");
            return true;
        }

        public bool MakeMoveWithPromotion(Position to, PieceType promotedTo)
        {
            // System.Diagnostics.Debug.WriteLine($"MakeMoveWithPromotion to {to.Row},{to.Column} promoting to {promotedTo}");
            
            if (!SelectedPosition.HasValue || !ValidMoves.Contains(to))
            {
                // System.Diagnostics.Debug.WriteLine("MakeMoveWithPromotion failed - no selection or invalid move");
                return false;
            }

            var from = SelectedPosition.Value;
            var piece = Board[from];
            if (piece?.Color != CurrentPlayer) 
            {
                // System.Diagnostics.Debug.WriteLine("MakeMoveWithPromotion failed - wrong color");
                return false;
            }

            // Check if move is valid
            if (!ValidMoves.Contains(to)) 
            {
                // System.Diagnostics.Debug.WriteLine("MakeMoveWithPromotion failed - move not in valid moves");
                return false;
            }

            // Handle en passant capture
            var capturedPiece = Board[to];
            if (piece.Type == PieceType.Pawn && to == EnPassantTarget)
            {
                // Capture the pawn that moved two squares
                var capturedPawnRow = CurrentPlayer == PieceColor.White ? to.Row + 1 : to.Row - 1;
                capturedPiece = Board[new Position(capturedPawnRow, to.Column)];
                Board[new Position(capturedPawnRow, to.Column)] = null;
                // System.Diagnostics.Debug.WriteLine($"En passant capture at {capturedPawnRow},{to.Column}");
            }

            // Make the move with promotion
            // System.Diagnostics.Debug.WriteLine($"Making move from {from.Row},{from.Column} to {to.Row},{to.Column} with promotion to {promotedTo}");
            Board.MakeMove(from, to, promotedTo);

            // Set en passant target for next move (if pawn moved two squares)
            EnPassantTarget = null;
            if (piece.Type == PieceType.Pawn && Math.Abs(to.Row - from.Row) == 2)
            {
                var enPassantRow = CurrentPlayer == PieceColor.White ? to.Row + 1 : to.Row - 1;
                EnPassantTarget = new Position(enPassantRow, to.Column);
                // System.Diagnostics.Debug.WriteLine($"En passant target set at {enPassantRow},{to.Column}");
            }

            // Record the move
            var move = new ChessMove(from, to, piece, capturedPiece);
            MoveHistory.Add(move);

            // Clear selection
            SelectedPosition = null;
            ValidMoves.Clear();

            // Switch players
            CurrentPlayer = CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            // System.Diagnostics.Debug.WriteLine($"Player switched to: {CurrentPlayer}");

            // Update game state
            UpdateGameState();

            MoveMade?.Invoke(this, new MoveMadeEventArgs(move));
            // System.Diagnostics.Debug.WriteLine("MakeMoveWithPromotion successful");
            return true;
        }

        private void UpdateGameState()
        {
            var previousState = State;

            if (Board.IsCheckmate(CurrentPlayer))
            {
                State = GameState.Checkmate;
            }
            else if (Board.IsStalemate(CurrentPlayer))
            {
                State = GameState.Stalemate;
            }
            else if (Board.IsInCheck(CurrentPlayer))
            {
                State = GameState.Check;
            }
            else
            {
                State = GameState.Playing;
            }

            if (State != previousState)
            {
                GameStateChanged?.Invoke(this, new GameStateChangedEventArgs(State, CurrentPlayer));
                
                // Check if game has ended
                if (IsGameEnded())
                {
                    var outcome = GetGameOutcome();
                    GameEnded?.Invoke(this, new GameEndedEventArgs(outcome, State));
                }
            }
        }

        private bool IsGameEnded()
        {
            return State == GameState.Checkmate || State == GameState.Stalemate || State == GameState.Draw;
        }

        private GameOutcome GetGameOutcome()
        {
            return State switch
            {
                GameState.Checkmate => CurrentPlayer == PieceColor.White ? GameOutcome.BlackWins : GameOutcome.WhiteWins,
                GameState.Stalemate => GameOutcome.Draw,
                GameState.Draw => GameOutcome.Draw,
                _ => GameOutcome.Draw
            };
        }

        public void NewGame()
        {
            Board = new Models.ChessBoard();
            CurrentPlayer = PieceColor.White;
            State = GameState.Playing;
            SelectedPosition = null;
            ValidMoves.Clear();
            MoveHistory.Clear();
            EnPassantTarget = null;
            GameStateChanged?.Invoke(this, new GameStateChangedEventArgs(State, CurrentPlayer));
        }

        // Method to restore game state from saved data
        public void RestoreGameState(Models.ChessBoard board, List<ChessMove> moveHistory, Position? enPassantTarget, PieceColor currentPlayer, GameState state)
        {
            Board = board;
            MoveHistory = moveHistory ?? new List<ChessMove>();
            EnPassantTarget = enPassantTarget;
            SetCurrentPlayer(currentPlayer);
            SetState(state);
            SelectedPosition = null;
            ValidMoves.Clear();
        }

        // Internal methods to set private properties
        internal void SetCurrentPlayer(PieceColor currentPlayer)
        {
            CurrentPlayer = currentPlayer;
        }

        internal void SetState(GameState state)
        {
            State = state;
        }

        public bool CanUndo()
        {
            return MoveHistory.Count > 0;
        }

        public void RequestDraw()
        {
            if (State != GameState.Playing) return;
            
            DrawRequested?.Invoke(this, new DrawRequestedEventArgs(CurrentPlayer));
        }

        public void AcceptDraw()
        {
            if (State != GameState.Playing) return;
            
            State = GameState.Draw;
            GameStateChanged?.Invoke(this, new GameStateChangedEventArgs(State, CurrentPlayer));
        }

        public void UndoMove()
        {
            if (!CanUndo()) return;

            var lastMove = MoveHistory.Last();
            MoveHistory.RemoveAt(MoveHistory.Count - 1);

            // Restore the board state
            Board = new ChessBoard();
            foreach (var move in MoveHistory)
            {
                Board.MakeMove(move.From, move.To);
            }

            // Switch back to previous player
            CurrentPlayer = CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;

            // Clear selection
            SelectedPosition = null;
            ValidMoves.Clear();

            // Update game state
            UpdateGameState();
        }
    }

    public class ChessMove
    {
        public Position From { get; }
        public Position To { get; }
        public ChessPiece Piece { get; }
        public ChessPiece? CapturedPiece { get; }

        public ChessMove(Position from, Position to, ChessPiece piece, ChessPiece? capturedPiece = null)
        {
            From = from;
            To = to;
            Piece = piece;
            CapturedPiece = capturedPiece;
        }

        public override string ToString()
        {
            var capture = CapturedPiece != null ? "x" : "";
            return $"{Piece.Type} {From}{capture}{To}";
        }
    }

    public class PieceSelectedEventArgs : EventArgs
    {
        public Position Position { get; }
        public List<Position> ValidMoves { get; }

        public PieceSelectedEventArgs(Position position, List<Position> validMoves)
        {
            Position = position;
            ValidMoves = validMoves;
        }
    }

    public class MoveMadeEventArgs : EventArgs
    {
        public ChessMove Move { get; }

        public MoveMadeEventArgs(ChessMove move)
        {
            Move = move;
        }
    }

    public class GameStateChangedEventArgs : EventArgs
    {
        public GameState State { get; }
        public PieceColor CurrentPlayer { get; }

        public GameStateChangedEventArgs(GameState state, PieceColor currentPlayer)
        {
            State = state;
            CurrentPlayer = currentPlayer;
        }
    }

    public class PawnPromotionEventArgs : EventArgs
    {
        public Position Position { get; }
        public PieceColor Color { get; }
        public PieceType PromotedTo { get; set; } = PieceType.Queen; // Default to Queen

        public PawnPromotionEventArgs(Position position, PieceColor color)
        {
            Position = position;
            Color = color;
        }
    }

    public class DrawRequestedEventArgs : EventArgs
    {
        public PieceColor RequestingPlayer { get; }

        public DrawRequestedEventArgs(PieceColor requestingPlayer)
        {
            RequestingPlayer = requestingPlayer;
        }
    }

    public class GameEndedEventArgs : EventArgs
    {
        public GameOutcome Outcome { get; }
        public GameState State { get; }

        public GameEndedEventArgs(GameOutcome outcome, GameState state)
        {
            Outcome = outcome;
            State = state;
        }
    }
}
