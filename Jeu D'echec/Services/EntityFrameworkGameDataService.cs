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
        public EntityFrameworkGameDataService()
        {
            try
            {
                // Initialize database on first use
                using var context = new ChessDbContext();
                DatabaseInitializer.InitializeDatabase();
            }
            catch (Exception ex)
            {
                // Fallback to simple service if EF fails
                throw new InvalidOperationException("Failed to initialize Entity Framework service", ex);
            }
        }

        public async Task<List<SavedGameInfo>> GetSavedGamesAsync()
        {
            try
            {
                // Clean up duplicates first
                await CleanupDuplicateGamesAsync();
                
                using var context = new ChessDbContext();
                var savedGames = await context.SavedGames
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
                return new List<SavedGameInfo>();
            }
        }

        public async Task<ChessGame?> LoadGameAsync(int gameId)
        {
            try
            {
                using var context = new ChessDbContext();
                var savedGame = await context.SavedGames
                    .Include(g => g.Moves)
                    .Include(g => g.BoardStates)
                    .FirstOrDefaultAsync(g => g.Id == gameId);

                if (savedGame == null)
                    return null;

                return await LoadGameFromSavedGameAsync(savedGame);
            }
            catch (Exception ex)
            {
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
                using var context = new ChessDbContext();
                
                // Clean up duplicates first
                await CleanupDuplicateGamesAsync();
                
                // Check if a game with the same players already exists (case insensitive)
                var existingGame = await context.SavedGames
                    .FirstOrDefaultAsync(g => 
                        g.Player1Name.ToLower() == gameInfo.Player1Name.ToLower() && 
                        g.Player2Name.ToLower() == gameInfo.Player2Name.ToLower());

                SavedGame savedGame;
                
                if (existingGame != null)
                {
                    // Update existing game
                    existingGame.LastPlayed = DateTime.Now;
                    existingGame.MoveCount = game.MoveHistory.Count;
                    existingGame.CurrentPlayer = game.CurrentPlayer.ToString();
                    existingGame.GameState = game.State.ToString();
                    
                    // Remove old board states and moves
                    var oldBoardStates = await context.BoardStates.Where(bs => bs.SavedGameId == existingGame.Id).ToListAsync();
                    var oldMoves = await context.ChessMoves.Where(cm => cm.SavedGameId == existingGame.Id).ToListAsync();
                    
                    context.BoardStates.RemoveRange(oldBoardStates);
                    context.ChessMoves.RemoveRange(oldMoves);
                    
                    // Save changes to remove old data
                    await context.SaveChangesAsync();
                    
                    savedGame = existingGame;
                }
                else
                {
                    // Create new game
                    savedGame = new SavedGame
                    {
                        Player1Name = gameInfo.Player1Name,
                        Player2Name = gameInfo.Player2Name,
                        CreatedDate = gameInfo.CreatedDate,
                        LastPlayed = DateTime.Now,
                        MoveCount = game.MoveHistory.Count,
                        CurrentPlayer = game.CurrentPlayer.ToString(),
                        GameState = game.State.ToString()
                    };
                    context.SavedGames.Add(savedGame);
                }

                await context.SaveChangesAsync();

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

                        context.BoardStates.Add(boardState);
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

                    context.ChessMoves.Add(chessMove);
                }

                await context.SaveChangesAsync();
                return savedGame.Id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteGameAsync(int gameId)
        {
            try
            {
                using var context = new ChessDbContext();
                var savedGame = await context.SavedGames.FindAsync(gameId);
                if (savedGame == null)
                    return false;

                context.SavedGames.Remove(savedGame);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
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

        public async Task CleanupDuplicateGamesAsync()
        {
            try
            {
                using var context = new ChessDbContext();
                
                // Get all saved games
                var allGames = await context.SavedGames.ToListAsync();
                
                // Group by player names (case insensitive)
                var gameGroups = allGames
                    .GroupBy(g => new { 
                        Player1 = g.Player1Name?.ToLowerInvariant(), 
                        Player2 = g.Player2Name?.ToLowerInvariant() 
                    })
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var group in gameGroups)
                {
                    // Keep only the most recent game, delete the others
                    var games = group.OrderByDescending(g => g.LastPlayed).ToList();
                    var gameToKeep = games.First();
                    var gamesToDelete = games.Skip(1).ToList();

                    foreach (var gameToDelete in gamesToDelete)
                    {
                        // Remove associated data
                        var boardStates = await context.BoardStates.Where(bs => bs.SavedGameId == gameToDelete.Id).ToListAsync();
                        var moves = await context.ChessMoves.Where(cm => cm.SavedGameId == gameToDelete.Id).ToListAsync();
                        
                        context.BoardStates.RemoveRange(boardStates);
                        context.ChessMoves.RemoveRange(moves);
                        context.SavedGames.Remove(gameToDelete);
                    }
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Silently handle errors to avoid breaking the app
            }
        }

        private async Task<ChessGame> LoadGameFromSavedGameAsync(SavedGame savedGame)
        {
            var game = new ChessGame();
            
            // Load board states and moves from database
            using var context = new ChessDbContext();
            var boardStates = await context.BoardStates
                .Where(bs => bs.SavedGameId == savedGame.Id)
                .ToListAsync();
            
            var moves = await context.ChessMoves
                .Where(cm => cm.SavedGameId == savedGame.Id)
                .OrderBy(cm => cm.MoveNumber)
                .ToListAsync();
            
            // Clear the board first to avoid conflicts with initial pieces
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    game.Board[new Position(row, col)] = null;
                }
            }
            
            // Restore the board state directly from saved board states
            // This is the final state after all moves, so we don't need to replay moves
            foreach (var boardState in boardStates)
            {
                var position = new Position(boardState.Row, boardState.Column);
                if (boardState.PieceType != null && boardState.PieceColor != null)
                {
                    var pieceType = Enum.Parse<PieceType>(boardState.PieceType);
                    var pieceColor = Enum.Parse<PieceColor>(boardState.PieceColor);
                    var piece = new ChessPiece(pieceType, pieceColor) { HasMoved = boardState.HasMoved };
                    game.Board[position] = piece;
                }
            }
            
            // Restore move history for reference only (don't replay moves)
            // The board state already contains the final positions
            foreach (var move in moves)
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
            
            // Restore game state
            game.SetCurrentPlayer(Enum.Parse<PieceColor>(savedGame.CurrentPlayer));
            game.SetState(Enum.Parse<GameState>(savedGame.GameState));
            
            return game;
        }

        public void Dispose()
        {
            // No longer needed since we use using statements
        }
    }
}
