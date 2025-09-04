using System;
using Jeu_D_echec.Models;

namespace Jeu_D_echec.Services
{
    public class ELOService
    {
        private const int K_FACTOR = 32; // Standard K-factor for ELO calculation
        private const int STARTING_ELO = 1200;

        /// <summary>
        /// Calculate ELO rating changes for both players
        /// </summary>
        /// <param name="whiteRating">White player's current rating</param>
        /// <param name="blackRating">Black player's current rating</param>
        /// <param name="result">Game result from white player's perspective</param>
        /// <returns>Tuple of (whiteRatingChange, blackRatingChange)</returns>
        public static (int whiteChange, int blackChange) CalculateRatingChanges(
            int whiteRating, 
            int blackRating, 
            GameOutcome result)
        {
            // Calculate expected scores
            double whiteExpectedScore = CalculateExpectedScore(whiteRating, blackRating);
            double blackExpectedScore = CalculateExpectedScore(blackRating, whiteRating);

            // Determine actual scores based on result
            double whiteActualScore = GetActualScore(result, true);
            double blackActualScore = GetActualScore(result, false);

            // Calculate rating changes
            int whiteChange = (int)Math.Round(K_FACTOR * (whiteActualScore - whiteExpectedScore));
            int blackChange = (int)Math.Round(K_FACTOR * (blackActualScore - blackExpectedScore));

            return (whiteChange, blackChange);
        }

        /// <summary>
        /// Calculate expected score for a player against an opponent
        /// </summary>
        private static double CalculateExpectedScore(int playerRating, int opponentRating)
        {
            return 1.0 / (1.0 + Math.Pow(10, (opponentRating - playerRating) / 400.0));
        }

        /// <summary>
        /// Get actual score based on game result
        /// </summary>
        private static double GetActualScore(GameOutcome result, bool isWhite)
        {
            return result switch
            {
                GameOutcome.WhiteWins => isWhite ? 1.0 : 0.0,
                GameOutcome.BlackWins => isWhite ? 0.0 : 1.0,
                GameOutcome.Draw => 0.5,
                GameOutcome.Abandoned => 0.5, // Treat abandoned games as draws for ELO
                _ => 0.5
            };
        }

        /// <summary>
        /// Get ELO category based on rating
        /// </summary>
        public static string GetELOCategory(int rating)
        {
            return rating switch
            {
                >= 2800 => "Grand Maître",
                >= 2600 => "Maître International",
                >= 2400 => "Maître FIDE",
                >= 2200 => "Candidat Maître",
                >= 2000 => "Expert",
                >= 1800 => "Classe A",
                >= 1600 => "Classe B",
                >= 1400 => "Classe C",
                >= 1200 => "Classe D",
                _ => "Débutant"
            };
        }

        /// <summary>
        /// Get ELO category color for UI
        /// </summary>
        public static string GetELOCategoryColor(int rating)
        {
            return rating switch
            {
                >= 2800 => "#FFD700", // Gold
                >= 2600 => "#C0C0C0", // Silver
                >= 2400 => "#CD7F32", // Bronze
                >= 2200 => "#FF6B6B", // Red
                >= 2000 => "#4ECDC4", // Teal
                >= 1800 => "#45B7D1", // Blue
                >= 1600 => "#96CEB4", // Green
                >= 1400 => "#FFEAA7", // Yellow
                >= 1200 => "#DDA0DD", // Plum
                _ => "#A0A0A0" // Gray
            };
        }
    }
}
