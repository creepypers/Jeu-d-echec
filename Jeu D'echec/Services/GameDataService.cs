using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Jeu_D_echec.Models;
using Jeu_D_echec.Controls;

namespace Jeu_D_echec.Services
{
    public class GameDataService
    {
        private const string SAVED_GAMES_FOLDER = "SavedGames";
        private const string GAME_DATA_FILE_EXTENSION = ".chess";

        public async Task SaveGameAsync(ChessGame game, GameInfo gameInfo)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var savedGamesFolder = await localFolder.CreateFolderAsync(SAVED_GAMES_FOLDER, CreationCollisionOption.OpenIfExists);

                // Create filename with timestamp
                var fileName = $"{gameInfo.Player1Name}_{gameInfo.Player2Name}_{DateTime.Now:yyyyMMdd_HHmmss}{GAME_DATA_FILE_EXTENSION}";
                var file = await savedGamesFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                // Create save data
                var saveData = new GameSaveData
                {
                    GameInfo = gameInfo,
                    BoardState = SerializeBoardState(game.Board),
                    MoveHistory = game.MoveHistory,
                    EnPassantTarget = game.EnPassantTarget,
                    CurrentPlayer = game.CurrentPlayer,
                    State = game.State
                };

                // Serialize and save
                var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await FileIO.WriteTextAsync(file, json);

                System.Diagnostics.Debug.WriteLine($"Game saved successfully: {fileName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game: {ex.Message}");
                throw new Exception("Erreur lors de la sauvegarde de la partie.", ex);
            }
        }

        public async Task<ChessGame?> LoadGameAsync(string fileName)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var savedGamesFolder = await localFolder.GetFolderAsync(SAVED_GAMES_FOLDER);
                var file = await savedGamesFolder.GetFileAsync(fileName);

                var json = await FileIO.ReadTextAsync(file);
                var saveData = JsonSerializer.Deserialize<GameSaveData>(json);

                if (saveData == null)
                {
                    throw new Exception("Données de sauvegarde invalides.");
                }

                // Recreate the game
                var game = new ChessGame();
                var board = DeserializeBoardState(saveData.BoardState);
                game.RestoreGameState(board, saveData.MoveHistory, saveData.EnPassantTarget, saveData.CurrentPlayer, saveData.State);

                // Update game info
                saveData.GameInfo.LastPlayed = DateTime.Now;
                await SaveGameAsync(game, saveData.GameInfo);

                System.Diagnostics.Debug.WriteLine($"Game loaded successfully: {fileName}");
                return game;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading game: {ex.Message}");
                throw new Exception("Erreur lors du chargement de la partie.", ex);
            }
        }

        public async Task<List<GameInfo>> GetSavedGamesInfoAsync()
        {
            var gamesInfo = new List<GameInfo>();

            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var savedGamesFolder = await localFolder.CreateFolderAsync(SAVED_GAMES_FOLDER, CreationCollisionOption.OpenIfExists);
                
                var files = await savedGamesFolder.GetFilesAsync();
                
                foreach (var file in files)
                {
                    try
                    {
                        var json = await FileIO.ReadTextAsync(file);
                        var saveData = JsonSerializer.Deserialize<GameSaveData>(json);
                        
                        if (saveData?.GameInfo != null)
                        {
                            gamesInfo.Add(saveData.GameInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error reading saved game {file.Name}: {ex.Message}");
                    }
                }
                
                // Sort by last played date (most recent first)
                gamesInfo.Sort((x, y) => y.LastPlayed.CompareTo(x.LastPlayed));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting saved games info: {ex.Message}");
            }
            
            return gamesInfo;
        }

        public async Task DeleteGameAsync(string fileName)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var savedGamesFolder = await localFolder.GetFolderAsync(SAVED_GAMES_FOLDER);
                var file = await savedGamesFolder.GetFileAsync(fileName);
                await file.DeleteAsync();

                System.Diagnostics.Debug.WriteLine($"Game deleted successfully: {fileName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting game: {ex.Message}");
                throw new Exception("Erreur lors de la suppression de la partie.", ex);
            }
        }

        private string SerializeBoardState(Models.ChessBoard board)
        {
            var boardData = new BoardStateData();
            
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var position = new Position(row, col);
                    var piece = board[position];
                    
                    if (piece != null)
                    {
                        boardData.Pieces.Add(new PieceData
                        {
                            Row = row,
                            Column = col,
                            Type = piece.Type,
                            Color = piece.Color,
                            HasMoved = piece.HasMoved
                        });
                    }
                }
            }
            
            return JsonSerializer.Serialize(boardData);
        }

        private Models.ChessBoard DeserializeBoardState(string boardStateJson)
        {
            var boardData = JsonSerializer.Deserialize<BoardStateData>(boardStateJson);
            var board = new Models.ChessBoard();
            
            if (boardData?.Pieces != null)
            {
                foreach (var pieceData in boardData.Pieces)
                {
                    var position = new Position(pieceData.Row, pieceData.Column);
                    var piece = new ChessPiece(pieceData.Type, pieceData.Color)
                    {
                        HasMoved = pieceData.HasMoved
                    };
                    board[position] = piece;
                }
            }
            
            return board;
        }
    }

    // Data classes for serialization
    public class GameSaveData
    {
        public GameInfo GameInfo { get; set; } = new();
        public string BoardState { get; set; } = "";
        public List<ChessMove> MoveHistory { get; set; } = new();
        public Position? EnPassantTarget { get; set; }
        public PieceColor CurrentPlayer { get; set; } = PieceColor.White;
        public GameState State { get; set; } = GameState.Playing;
    }

    public class BoardStateData
    {
        public List<PieceData> Pieces { get; set; } = new();
    }

    public class PieceData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public PieceType Type { get; set; }
        public PieceColor Color { get; set; }
        public bool HasMoved { get; set; }
    }
}
