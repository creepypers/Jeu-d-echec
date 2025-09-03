using Microsoft.EntityFrameworkCore;
using Jeu_D_echec.Data;
using Jeu_D_echec.Models;
using Jeu_D_echec.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jeu_D_echec.Services
{
    public class EntityFrameworkGameDataService : IGameDataService
    {
        private readonly ChessDbContext _context;

        public EntityFrameworkGameDataService()
        {
            try
            {
                _context = new ChessDbContext();
                DatabaseInitializer.InitializeDatabase();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating EntityFrameworkGameDataService: {ex.Message}");
                // Fallback to simple service if EF fails
                throw new InvalidOperationException("Failed to initialize Entity Framework service", ex);
            }
        }

        public async Task<List<SavedGameInfo>> GetSavedGamesAsync()
        {
            try
            {
                var savedGames = await _context.SavedGames
                    .OrderByDescending(g => g.LastPlayed)
                    .ToListAsync();

                return savedGames.Select(g => new SavedGameInfo
                {
                    Id = g.Id,
                    Player1Name = g.Player1Name,
                    Player2Name = g.Player2Name,
                    CreatedDate = g.CreatedDate,
                    LastPlayed = g.LastPlayed,
                    CurrentPlayer = Enum.Parse<PieceColor>(g.CurrentPlayer),
                    MoveCount = g.MoveCount
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting saved games: {ex.Message}");
                return new List<SavedGameInfo>();
            }
        }

        public async Task<ChessGame?> LoadGameAsync(int gameId)
        {
            try
            {
                var savedGame = await _context.SavedGames
                    .Include(g => g.Moves)
                    .Include(g => g.BoardStates)
                    .FirstOrDefaultAsync(g => g.Id == gameId);

                if (savedGame == null)
                    return null;

                return await LoadGameFromSavedGameAsync(savedGame);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading game {gameId}: {ex.Message}");
                return null;
            }
        }

        public async Task<ChessGame?> LoadGameAsync(string fileName)
        {
            // For backward compatibility, try to parse fileName as int
            if (int.TryParse(fileName, out int gameId))
            {
                return await LoadGameAsync(gameId);
            }
            return null;
        }

        public async Task<int> SaveGameAsync(ChessGame game, Jeu_D_echec.Controls.GameInfo gameInfo)
        {
            try
            {
                var savedGame = new SavedGame
                {
                    Player1Name = gameInfo.Player1Name,
                    Player2Name = gameInfo.Player2Name,
                    CreatedDate = gameInfo.CreatedDate,
                    LastPlayed = DateTime.Now,
                    MoveCount = game.MoveHistory.Count,
                    CurrentPlayer = game.CurrentPlayer.ToString(),
                    GameState = game.State.ToString()
                };

                _context.SavedGames.Add(savedGame);
                await _context.SaveChangesAsync();

                // Save current board state
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        var position = new Position(row, col);
                        var piece = game.Board[position];
                        
                        var boardState = new BoardState
                        {
                            SavedGameId = savedGame.Id,
                            Row = row,
                            Column = col,
                            PieceType = piece?.Type.ToString(),
                            PieceColor = piece?.Color.ToString(),
                            HasMoved = piece?.HasMoved ?? false
                        };

                        _context.BoardStates.Add(boardState);
                    }
                }

                // Save moves
                foreach (var move in game.MoveHistory)
                {
                    var chessMove = new SavedChessMove
                    {
                        SavedGameId = savedGame.Id,
                        FromRow = move.From.Row,
                        FromColumn = move.From.Column,
                        ToRow = move.To.Row,
                        ToColumn = move.To.Column,
                        PieceType = move.Piece.Type.ToString(),
                        PieceColor = move.Piece.Color.ToString(),
                        CapturedPieceType = move.CapturedPiece?.Type.ToString(),
                        CapturedPieceColor = move.CapturedPiece?.Color.ToString(),
                        PromotionType = null, // ChessMove doesn't have PromotionType
                        MoveNumber = game.MoveHistory.IndexOf(move) + 1,
                        Timestamp = DateTime.Now
                    };

                    _context.ChessMoves.Add(chessMove);
                }

                await _context.SaveChangesAsync();
                return savedGame.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteGameAsync(int gameId)
        {
            try
            {
                var savedGame = await _context.SavedGames.FindAsync(gameId);
                if (savedGame == null)
                    return false;

                _context.SavedGames.Remove(savedGame);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting game {gameId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteGameAsync(string fileName)
        {
            if (int.TryParse(fileName, out int gameId))
            {
                return await DeleteGameAsync(gameId);
            }
            return false;
        }

        private async Task<ChessGame> LoadGameFromSavedGameAsync(SavedGame savedGame)
        {
            var game = new ChessGame();
            
            // Restore the board state directly from saved board states
            foreach (var boardState in savedGame.BoardStates)
            {
                var position = new Position(boardState.Row, boardState.Column);
                if (boardState.Piece != null)
                {
                    game.Board[position] = boardState.Piece;
                }
            }
            
            // Restore move history
            foreach (var move in savedGame.Moves.OrderBy(m => m.MoveNumber))
            {
                var fromPosition = new Position(move.FromRow, move.FromColumn);
                var toPosition = new Position(move.ToRow, move.ToColumn);
                
                // Create the piece
                var pieceType = Enum.Parse<PieceType>(move.PieceType);
                var pieceColor = Enum.Parse<PieceColor>(move.PieceColor);
                var piece = new ChessPiece(pieceType, pieceColor);
                
                // Create the move
                var capturedPiece = move.CapturedPieceType != null ? 
                    new ChessPiece(Enum.Parse<PieceType>(move.CapturedPieceType), 
                                 Enum.Parse<PieceColor>(move.CapturedPieceColor!)) : null;
                
                var chessMove = new ChessMove(fromPosition, toPosition, piece, capturedPiece);
                game.MoveHistory.Add(chessMove);
            }
            
            return game;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
