using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Jeu_D_echec.Models;
using Jeu_D_echec.Services;

namespace Jeu_D_echec.Controls
{
    public sealed partial class HomePage : UserControl
    {
        public event EventHandler<StartNewGameEventArgs>? StartNewGameRequested;
        public event EventHandler<ResumeGameEventArgs>? ResumeGameRequested;

        private ObservableCollection<SavedGameInfo> _savedGames;
        private readonly IGameDataService _dataService;

        public HomePage()
        {
            InitializeComponent();
            _savedGames = new ObservableCollection<SavedGameInfo>();
            SavedGamesListView.ItemsSource = _savedGames;
            
            try
            {
                _dataService = new EntityFrameworkGameDataService();
                System.Diagnostics.Debug.WriteLine("Using Entity Framework service");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Entity Framework failed, falling back to simple service: {ex.Message}");
                _dataService = new SimpleGameDataService();
            }
        }

        public void LoadOnStart()
        {
            LoadSavedGames();
        }

        private async void OnStartNewGameClick(object sender, RoutedEventArgs e)
        {
            var player1Name = Player1NameTextBox.Text.Trim();
            var player2Name = Player2NameTextBox.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(player1Name))
            {
                ShowMessage("Veuillez entrer le nom du joueur blanc.");
                Player1NameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (string.IsNullOrEmpty(player2Name))
            {
                ShowMessage("Veuillez entrer le nom du joueur noir.");
                Player2NameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (player1Name.Equals(player2Name, StringComparison.OrdinalIgnoreCase))
            {
                ShowMessage("Les noms des joueurs doivent être différents.");
                return;
            }

            // Create new game info
            var gameInfo = new GameInfo
            {
                Player1Name = player1Name,
                Player2Name = player2Name,
                CreatedDate = DateTime.Now,
                LastPlayed = DateTime.Now
            };

            StartNewGameRequested?.Invoke(this, new StartNewGameEventArgs(gameInfo));
        }

        private void OnResumeGameClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SavedGameInfo savedGame)
            {
                ResumeGameRequested?.Invoke(this, new ResumeGameEventArgs(savedGame));
            }
        }

        private async void OnDeleteGameClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SavedGameInfo savedGame)
            {
                var dialog = new ContentDialog
                {
                    Title = "Supprimer la partie",
                    Content = $"Êtes-vous sûr de vouloir supprimer la partie entre {savedGame.Player1Name} et {savedGame.Player2Name} ?",
                    PrimaryButtonText = "Supprimer",
                    SecondaryButtonText = "Annuler",
                    DefaultButton = ContentDialogButton.Secondary,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await DeleteSavedGame(savedGame);
                }
            }
        }



        private async void LoadSavedGames()
        {
            try
            {
                _savedGames.Clear();
                var savedGames = await _dataService.GetSavedGamesAsync();
                
                foreach (var game in savedGames)
                {
                    _savedGames.Add(game);
                }

                NoSavedGamesText.Visibility = _savedGames.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved games: {ex.Message}");
                ShowMessage("Erreur lors du chargement des parties sauvegardées.");
            }
        }



        private async Task DeleteSavedGame(SavedGameInfo savedGame)
        {
            try
            {
                bool success = await _dataService.DeleteGameAsync(savedGame.Id);
                if (success)
                {
                    _savedGames.Remove(savedGame);
                    NoSavedGamesText.Visibility = _savedGames.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    ShowMessage("Partie supprimée avec succès.");
                }
                else
                {
                    ShowMessage("Erreur lors de la suppression de la partie.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting saved game: {ex.Message}");
                ShowMessage("Erreur lors de la suppression de la partie.");
            }
        }

        private void ShowMessage(string message)
        {
            // Simple message display - in a real app, you might want to use a proper dialog
            System.Diagnostics.Debug.WriteLine($"Message: {message}");
            // TODO: Implement proper message display (ContentDialog or Toast)
        }

        public void RefreshSavedGames()
        {
            LoadSavedGames();
        }
    }

    // Event Args Classes
    public class StartNewGameEventArgs : EventArgs
    {
        public GameInfo GameInfo { get; }

        public StartNewGameEventArgs(GameInfo gameInfo)
        {
            GameInfo = gameInfo;
        }
    }

    public class ResumeGameEventArgs : EventArgs
    {
        public SavedGameInfo SavedGame { get; }

        public ResumeGameEventArgs(SavedGameInfo savedGame)
        {
            SavedGame = savedGame;
        }
    }

    // Data Models
    public class GameInfo
    {
        public string Player1Name { get; set; } = "";
        public string Player2Name { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime LastPlayed { get; set; }
        public PieceColor CurrentPlayer { get; set; } = PieceColor.White;
        public int MoveCount { get; set; } = 0;
        public string? GameState { get; set; }
    }

    public class SavedGameInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string Player1Name { get; set; } = "";
        public string Player2Name { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime LastPlayed { get; set; }
        public PieceColor CurrentPlayer { get; set; } = PieceColor.White;
        public int MoveCount { get; set; } = 0;

        public string GameTitle => $"{Player1Name} vs {Player2Name}";
        public string PlayerInfo => $"Blanc: {Player1Name} | Noir: {Player2Name}";
        public string LastPlayedText => $"Dernière partie: {LastPlayed:dd/MM/yyyy HH:mm}";
    }
}
