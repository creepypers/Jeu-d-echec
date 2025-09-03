using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Jeu_D_echec.Models;
using Jeu_D_echec.Services;

namespace Jeu_D_echec.Controls
{
    public sealed partial class GamePage : UserControl
    {
        private ChessGame? _game;
        private GameInfo? _gameInfo;
        private readonly IGameDataService _dataService;

        public event EventHandler<RoutedEventArgs>? SaveGameRequested;
        public event EventHandler<RoutedEventArgs>? BackToMenuRequested;

        public GamePage()
        {
            InitializeComponent();
            
            try
            {
                _dataService = new EntityFrameworkGameDataService();
                System.Diagnostics.Debug.WriteLine("Using Entity Framework service in GamePage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Entity Framework failed in GamePage, falling back to simple service: {ex.Message}");
                _dataService = new SimpleGameDataService();
            }
        }

        public void InitializeWithGameInfo(GameInfo gameInfo)
        {
            _gameInfo = gameInfo;
            InitializeNewGame();
        }

        public void InitializeWithSavedGame(SavedGameInfo savedGameInfo)
        {
            LoadSavedGame(savedGameInfo);
        }

        private void InitializeNewGame()
        {
            if (_gameInfo == null) return;

            _game = new ChessGame();
            ChessBoardControl.Game = _game;
            
            // Subscribe to game events
            _game.GameStateChanged += OnGameStateChanged;
            _game.MoveMade += OnMoveMade;
            _game.DrawRequested += OnDrawRequested;
            
            // Update UI
            UpdatePlayerInfo();
            UpdateGameStatus();
            
            System.Diagnostics.Debug.WriteLine($"New game started: {_gameInfo.Player1Name} vs {_gameInfo.Player2Name}");
        }

        private async void LoadSavedGame(SavedGameInfo savedGameInfo)
        {
            try
            {
                // Show loading indicator
                GameStatusText.Text = "Chargement...";
                
                var game = await _dataService.LoadGameAsync(savedGameInfo.Id);
                if (game != null)
                {
                    _game = game;
                    ChessBoardControl.Game = _game;
                    
                    // Subscribe to game events
                    _game.GameStateChanged += OnGameStateChanged;
                    _game.MoveMade += OnMoveMade;
                    _game.DrawRequested += OnDrawRequested;
                    
                    // Create game info from saved game
                    _gameInfo = new GameInfo
                    {
                        Player1Name = savedGameInfo.Player1Name,
                        Player2Name = savedGameInfo.Player2Name,
                        CreatedDate = savedGameInfo.CreatedDate,
                        LastPlayed = DateTime.Now,
                        CurrentPlayer = _game.CurrentPlayer,
                        MoveCount = savedGameInfo.MoveCount
                    };
                    
                    // Update UI
                    UpdatePlayerInfo();
                    UpdateGameStatus();
                    
                    System.Diagnostics.Debug.WriteLine($"Game loaded: {_gameInfo.Player1Name} vs {_gameInfo.Player2Name}");
                }
                else
                {
                    ShowError("Erreur lors du chargement de la partie.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading game: {ex.Message}");
                ShowError("Erreur lors du chargement de la partie.");
            }
        }

        private void UpdatePlayerInfo()
        {
            if (_gameInfo == null) return;

            Player1NameText.Text = _gameInfo.Player1Name;
            Player2NameText.Text = _gameInfo.Player2Name;
        }

        private void UpdateGameStatus()
        {
            if (_game == null || _gameInfo == null) return;

            // Update current player indicator
            var currentPlayerName = _game.CurrentPlayer == PieceColor.White ? _gameInfo.Player1Name : _gameInfo.Player2Name;
            CurrentPlayerText.Text = $"Tour de {currentPlayerName}";
            
            // Update move count
            MoveCountText.Text = $"Coup {_game.MoveHistory.Count}";
            
            // Update game status
            GameStatusText.Text = _game.State switch
            {
                GameState.Playing => "En cours",
                GameState.Check => "Échec !",
                GameState.Checkmate => "Mat !",
                GameState.Stalemate => "Pat",
                GameState.Draw => "Match nul",
                _ => "État inconnu"
            };

            // Update button visibility based on game state
            RequestDrawButton.IsEnabled = _game.State == GameState.Playing;
        }

        private void OnGameStateChanged(object? sender, GameStateChangedEventArgs e)
        {
            UpdateGameStatus();
        }

        private void OnMoveMade(object? sender, MoveMadeEventArgs e)
        {
            if (_gameInfo != null)
            {
                _gameInfo.CurrentPlayer = _game!.CurrentPlayer;
                _gameInfo.MoveCount = _game.MoveHistory.Count;
                _gameInfo.LastPlayed = DateTime.Now;
            }
            UpdateGameStatus();
        }

        private async void OnDrawRequested(object? sender, DrawRequestedEventArgs e)
        {
            if (_gameInfo == null) return;

            var requestingPlayerName = e.RequestingPlayer == PieceColor.White ? _gameInfo.Player1Name : _gameInfo.Player2Name;
            var otherPlayerName = e.RequestingPlayer == PieceColor.White ? _gameInfo.Player2Name : _gameInfo.Player1Name;

            var dialog = new ContentDialog
            {
                Title = "Demande de match nul",
                Content = $"{requestingPlayerName} demande un match nul.\n{otherPlayerName}, acceptez-vous ?",
                PrimaryButtonText = "Accepter",
                SecondaryButtonText = "Refuser",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _game!.AcceptDraw();
            }
        }

        private void ShowError(string message)
        {
            // Simple error display - in a real app, you might want to use a proper dialog
            System.Diagnostics.Debug.WriteLine($"Error: {message}");
            GameStatusText.Text = message;
        }

        public async Task SaveCurrentGameAsync()
        {
            if (_game == null || _gameInfo == null) return;

            try
            {
                int gameId = await _dataService.SaveGameAsync(_game, _gameInfo);
                System.Diagnostics.Debug.WriteLine($"Game saved successfully with ID: {gameId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game: {ex.Message}");
                ShowError("Erreur lors de la sauvegarde.");
            }
        }



        private async void OnRequestDrawClick(object sender, RoutedEventArgs e)
        {
            if (_game == null) return;

            var dialog = new ContentDialog
            {
                Title = "Demander match nul",
                Content = "Voulez-vous vraiment demander un match nul ?",
                PrimaryButtonText = "Oui",
                SecondaryButtonText = "Non",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _game.RequestDraw();
            }
        }

        private async void OnSaveGameClick(object sender, RoutedEventArgs e)
        {
            if (_game == null || _gameInfo == null) return;

            try
            {
                int gameId = await _dataService.SaveGameAsync(_game, _gameInfo);
                
                var dialog = new ContentDialog
                {
                    Title = "Partie sauvegardée",
                    Content = $"Votre partie a été sauvegardée avec succès !\nID: {gameId}",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
                System.Diagnostics.Debug.WriteLine($"Game saved successfully with ID: {gameId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game: {ex.Message}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Erreur de sauvegarde",
                    Content = "Une erreur s'est produite lors de la sauvegarde de la partie.",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                await errorDialog.ShowAsync();
            }
        }

        private void OnBackToMenuClick(object sender, RoutedEventArgs e)
        {
            BackToMenuRequested?.Invoke(this, e);
        }
    }
}
