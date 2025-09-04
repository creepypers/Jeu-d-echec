using System;
using System.ComponentModel.DataAnnotations;

namespace Jeu_D_echec.Models
{
    public enum GameOutcome
    {
        WhiteWins,
        BlackWins,
        Draw,
        Abandoned
    }

    public class GameResultInfo
    {
        public int Id { get; set; }

        [Required]
        public int WhitePlayerId { get; set; }
        public Player WhitePlayer { get; set; } = null!;

        [Required]
        public int BlackPlayerId { get; set; }
        public Player BlackPlayer { get; set; } = null!;



        [Required]
        public GameOutcome Result { get; set; }

        // ELO Changes
        public int WhiteRatingBefore { get; set; }
        public int WhiteRatingAfter { get; set; }
        public int BlackRatingBefore { get; set; }
        public int BlackRatingAfter { get; set; }

        // Game details
        public int MoveCount { get; set; }
        public TimeSpan GameDuration { get; set; }
        public DateTime PlayedDate { get; set; } = DateTime.Now;

        // Computed properties
        public int WhiteRatingChange => WhiteRatingAfter - WhiteRatingBefore;
        public int BlackRatingChange => BlackRatingAfter - BlackRatingBefore;

        public string GetResultDescription()
        {
            return Result switch
            {
                GameOutcome.WhiteWins => $"{WhitePlayer.Name} gagne",
                GameOutcome.BlackWins => $"{BlackPlayer.Name} gagne",
                GameOutcome.Draw => "Match nul",
                GameOutcome.Abandoned => "Partie abandonnée",
                _ => "Résultat inconnu"
            };
        }


    }
}
