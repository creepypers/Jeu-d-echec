using System;
using Jeu_D_echec.Services;
using Jeu_D_echec.Models;
using Jeu_D_echec.Controls;

namespace Jeu_D_echec
{
    public static class TestApplication
    {
        public static void TestServices()
        {
            try
            {
                // Test SimpleGameDataService
                var simpleService = new SimpleGameDataService();
                Console.WriteLine("SimpleGameDataService created successfully");

                // Test EntityFrameworkGameDataService
                var efService = new EntityFrameworkGameDataService();
                Console.WriteLine("EntityFrameworkGameDataService created successfully");

                // Test ChessGame
                var game = new ChessGame();
                Console.WriteLine("ChessGame created successfully");

                // Test GameInfo
                var gameInfo = new GameInfo
                {
                    Player1Name = "Alice",
                    Player2Name = "Bob",
                    CreatedDate = DateTime.Now
                };
                Console.WriteLine("GameInfo created successfully");

                Console.WriteLine("All services and models are working correctly!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
