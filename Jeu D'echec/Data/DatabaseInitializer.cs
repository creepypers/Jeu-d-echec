using Microsoft.EntityFrameworkCore;
using Jeu_D_echec.Models;
using System;

namespace Jeu_D_echec.Data
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            try
            {
                using var context = new ChessDbContext();
                
                // Ensure the database is created
                var created = context.Database.EnsureCreated();
                if (created)
                {
                    System.Diagnostics.Debug.WriteLine("Database created successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Database already exists");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
