using Jeu_D_echec.Models;
using Jeu_D_echec.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jeu_D_echec.Services
{
    public interface IGameDataService
    {
        Task<List<SavedGameInfo>> GetSavedGamesAsync();
        Task<ChessGame?> LoadGameAsync(int gameId);
        Task<ChessGame?> LoadGameAsync(string fileName);
        Task<int> SaveGameAsync(ChessGame game, Jeu_D_echec.Controls.GameInfo gameInfo);
        Task<bool> DeleteGameAsync(int gameId);
        Task<bool> DeleteGameAsync(string fileName);
    }
}
