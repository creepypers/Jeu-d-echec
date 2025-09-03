using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jeu_D_echec.Models;
using Jeu_D_echec.Controls;

namespace Jeu_D_echec.Services
{
    public class SimpleGameDataService : IGameDataService
    {
        private static List<SavedGameInfo> _savedGames = new List<SavedGameInfo>();
        private static int _nextId = 1;

        public Task<List<SavedGameInfo>> GetSavedGamesAsync()
        {
            return Task.FromResult(new List<SavedGameInfo>(_savedGames));
        }

        public Task<ChessGame?> LoadGameAsync(int gameId)
        {
            // For now, return a new game
            return Task.FromResult<ChessGame?>(new ChessGame());
        }

        public Task<ChessGame?> LoadGameAsync(string fileName)
        {
            if (int.TryParse(fileName, out int gameId))
            {
                return LoadGameAsync(gameId);
            }
            return Task.FromResult<ChessGame?>(null);
        }

        public Task<int> SaveGameAsync(ChessGame game, Jeu_D_echec.Controls.GameInfo gameInfo)
        {
            var savedGame = new SavedGameInfo
            {
                Id = _nextId++,
                Player1Name = gameInfo.Player1Name,
                Player2Name = gameInfo.Player2Name,
                CreatedDate = gameInfo.CreatedDate,
                LastPlayed = DateTime.Now,
                CurrentPlayer = game.CurrentPlayer,
                MoveCount = game.MoveHistory.Count
            };

            _savedGames.Add(savedGame);
            return Task.FromResult(savedGame.Id);
        }

        public Task<bool> DeleteGameAsync(int gameId)
        {
            var game = _savedGames.Find(g => g.Id == gameId);
            if (game != null)
            {
                _savedGames.Remove(game);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteGameAsync(string fileName)
        {
            if (int.TryParse(fileName, out int gameId))
            {
                return DeleteGameAsync(gameId);
            }
            return Task.FromResult(false);
        }
    }
}
