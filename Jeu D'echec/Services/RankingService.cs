using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Jeu_D_echec.Data;
using Jeu_D_echec.Models;

namespace Jeu_D_echec.Services
{
    public interface IRankingService
    {
        Task<List<Player>> GetRankingsAsync();
        Task<Player?> GetPlayerAsync(int playerId);
        Task<Player?> GetPlayerByEmailAsync(string email);
        Task<Player> CreatePlayerAsync(string name, string email);
        Task<GameResultInfo> RecordGameResultAsync(int whitePlayerId, int blackPlayerId, GameOutcome result, int moveCount, TimeSpan duration);
        Task<List<GameResultInfo>> GetPlayerHistoryAsync(int playerId, int limit = 10);
    }

    public class RankingService : IRankingService
    {
        private readonly ChessDbContext _context;

        public RankingService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<List<Player>> GetRankingsAsync()
        {
            return await _context.Players
                .Where(p => p.TotalGamesPlayed > 0) // Only show players who have played at least one game
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.TotalGamesPlayed)
                .ToListAsync();
        }

        public async Task<Player?> GetPlayerAsync(int playerId)
        {
            return await _context.Players.FindAsync(playerId);
        }

        public async Task<Player?> GetPlayerByEmailAsync(string email)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Player> CreatePlayerAsync(string name, string email)
        {
            var player = new Player
            {
                Name = name,
                Email = email,
                Rating = 1200,
                CreatedDate = DateTime.Now,
                LastPlayed = DateTime.Now
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task<GameResultInfo> RecordGameResultAsync(int whitePlayerId, int blackPlayerId, GameOutcome result, int moveCount, TimeSpan duration)
        {
            var whitePlayer = await _context.Players.FindAsync(whitePlayerId);
            var blackPlayer = await _context.Players.FindAsync(blackPlayerId);

            if (whitePlayer == null || blackPlayer == null)
                throw new ArgumentException("One or both players not found");

            // Get current ratings
            int whiteRatingBefore = whitePlayer.Rating;
            int blackRatingBefore = blackPlayer.Rating;

            // Calculate ELO changes
            var (whiteChange, blackChange) = ELOService.CalculateRatingChanges(whiteRatingBefore, blackRatingBefore, result);

            // Update player ratings
            whitePlayer.UpdateRating(whiteRatingBefore + whiteChange);
            blackPlayer.UpdateRating(blackRatingBefore + blackChange);

            // Update player statistics
            bool whiteWon = result == GameOutcome.WhiteWins;
            bool blackWon = result == GameOutcome.BlackWins;
            bool draw = result == GameOutcome.Draw;

            whitePlayer.UpdateStats(whiteWon, draw);
            blackPlayer.UpdateStats(blackWon, draw);

            // Create game result record
            var gameResult = new GameResultInfo
            {
                WhitePlayerId = whitePlayerId,
                BlackPlayerId = blackPlayerId,
                Result = result,
                WhiteRatingBefore = whiteRatingBefore,
                WhiteRatingAfter = whiteRatingBefore + whiteChange,
                BlackRatingBefore = blackRatingBefore,
                BlackRatingAfter = blackRatingBefore + blackChange,
                MoveCount = moveCount,
                GameDuration = duration,
                PlayedDate = DateTime.Now
            };

            _context.GameResults.Add(gameResult);
            await _context.SaveChangesAsync();

            return gameResult;
        }

        public async Task<List<GameResultInfo>> GetPlayerHistoryAsync(int playerId, int limit = 10)
        {
            return await _context.GameResults
                .Include(gr => gr.WhitePlayer)
                .Include(gr => gr.BlackPlayer)
                .Where(gr => gr.WhitePlayerId == playerId || gr.BlackPlayerId == playerId)
                .OrderByDescending(gr => gr.PlayedDate)
                .Take(limit)
                .ToListAsync();
        }
    }
}
