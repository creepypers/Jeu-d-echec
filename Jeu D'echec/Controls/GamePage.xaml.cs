using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Jeu_D_echec.Models;
using Jeu_D_echec.Services;
using Jeu_D_echec.Data;
using Microsoft.EntityFrameworkCore;

namespace Jeu_D_echec.Controls
{
    public sealed partial class GamePage : UserControl
    {
        private ChessGame? _game;
        private GameInfo? _gameInfo;
        private readonly IGameDataService _dataService;

        public event EventHandler<RoutedEventArgs>? SaveGameRequested;
        public event EventHandler<RoutedEventArgs>? BackToMenuRequested;
        public event EventHandler<RoutedEventArgs>? GameSaved;

        public GamePage()
        {
            InitializeComponent();
            
            _dataService = new EntityFrameworkGameDataService();
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
            _game.GameEnded += OnGameEnded;
            
            // Update UI
            UpdatePlayerInfo();
            UpdateGameStatus();
            
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
                    _game.GameEnded += OnGameEnded;
                    
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
                    
                }
                else
                {
                    ShowError("Erreur lors du chargement de la partie.");
                }
            }
            catch (Exception ex)
            {
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

            // Update game state alert
            UpdateGameStateAlert();

            // Update button visibility based on game state
            RequestDrawButton.IsEnabled = _game.State == GameState.Playing;
        }

        private void UpdateGameStateAlert()
        {
            if (_game == null) return;

            switch (_game.State)
            {
                case GameState.Check:
                    GameStateAlert.Visibility = Visibility.Visible;
                    GameStateTitle.Text = "Échec !";
                    GameStateMessage.Text = "Votre roi est en échec";
                    GameStateAlert.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
                    break;
                case GameState.Checkmate:
                    GameStateAlert.Visibility = Visibility.Visible;
                    GameStateTitle.Text = "Échec et Mat !";
                    GameStateMessage.Text = "Partie terminée";
                    GameStateAlert.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                    break;
                case GameState.Stalemate:
                    GameStateAlert.Visibility = Visibility.Visible;
                    GameStateTitle.Text = "Pat !";
                    GameStateMessage.Text = "Match nul par pat";
                    GameStateAlert.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
                    break;
                case GameState.Draw:
                    GameStateAlert.Visibility = Visibility.Visible;
                    GameStateTitle.Text = "Match nul";
                    GameStateMessage.Text = "Partie terminée par accord mutuel";
                    GameStateAlert.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue);
                    break;
                default:
                    GameStateAlert.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void OnGameStateChanged(object? sender, GameStateChangedEventArgs e)
        {
            UpdateGameStatus();
            
            // Calculate ELO if game ended with checkmate
            if (e.State == GameState.Checkmate && _gameInfo != null)
            {
                await CalculateAndSaveELO();
            }
        }

        private async Task CalculateAndSaveELO()
        {
            if (_gameInfo == null) return;

            try
            {
                using var context = new ChessDbContext();
                var rankingService = new RankingService(context);

                // Get or create players
                var player1 = await rankingService.GetPlayerByEmailAsync(_gameInfo.Player1Name + "@chess.local");
                if (player1 == null)
                {
                    player1 = await rankingService.CreatePlayerAsync(_gameInfo.Player1Name, _gameInfo.Player1Name + "@chess.local");
                }

                var player2 = await rankingService.GetPlayerByEmailAsync(_gameInfo.Player2Name + "@chess.local");
                if (player2 == null)
                {
                    player2 = await rankingService.CreatePlayerAsync(_gameInfo.Player2Name, _gameInfo.Player2Name + "@chess.local");
                }

                // Determine game outcome
                var gameOutcome = _game!.CurrentPlayer == PieceColor.White ? GameOutcome.BlackWins : GameOutcome.WhiteWins;

                // Record game result (this will calculate and update ELO automatically)
                await rankingService.RecordGameResultAsync(
                    player1.Id, 
                    player2.Id, 
                    gameOutcome, 
                    _game.MoveHistory.Count, 
                    TimeSpan.FromMinutes(30) // Default duration
                );

            }
            catch (Exception ex)
            {
            }
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
            GameStatusText.Text = message;
        }

        public async Task SaveCurrentGameAsync()
        {
            if (_game == null || _gameInfo == null) return;

            try
            {
                int gameId = await _dataService.SaveGameAsync(_game, _gameInfo);
            }
            catch (Exception ex)
            {
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
                
                // Notify that game was saved
                GameSaved?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                
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

        private async void OnGameEnded(object? sender, GameEndedEventArgs e)
        {
            if (_game == null || _gameInfo == null) return;

            try
            {
                // Calculate ELO ratings
                await CalculateAndSaveELO(e.Outcome);
                
                // Show game end dialog
                await ShowGameEndDialog(e.Outcome, e.State);
                
                // Redirect to home page after a short delay
                await Task.Delay(2000);
                BackToMenuRequested?.Invoke(this, new RoutedEventArgs());
            }
            catch (Exception ex)
            {
                // Show error dialog
                var errorDialog = new ContentDialog
                {
                    Title = "Erreur",
                    Content = "Une erreur s'est produite lors de la fin de partie.",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                
                // Still redirect to home page
                BackToMenuRequested?.Invoke(this, new RoutedEventArgs());
            }
        }

        private async Task CalculateAndSaveELO(GameOutcome outcome)
        {
            if (_gameInfo == null) return;

            try
            {
                // Get or create players
                var whitePlayer = await GetOrCreatePlayer(_gameInfo.Player1Name, PieceColor.White);
                var blackPlayer = await GetOrCreatePlayer(_gameInfo.Player2Name, PieceColor.Black);

                // Calculate ELO changes
                var (whiteChange, blackChange) = ELOService.CalculateRatingChanges(
                    whitePlayer.Rating, 
                    blackPlayer.Rating, 
                    outcome
                );

                // Update player ratings
                whitePlayer.Rating += whiteChange;
                blackPlayer.Rating += blackChange;

                // Save updated ratings
                await SavePlayerRatings(whitePlayer, blackPlayer);

                // Save game result
                await SaveGameResult(whitePlayer, blackPlayer, outcome, whiteChange, blackChange);
            }
            catch (Exception ex)
            {
                // Log error but don't break the game flow
                System.Diagnostics.Debug.WriteLine($"Error calculating ELO: {ex.Message}");
            }
        }

        private async Task<Player> GetOrCreatePlayer(string playerName, PieceColor color)
        {
            using var context = new ChessDbContext();
            
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Name.ToLower() == playerName.ToLower());
            
            if (player == null)
            {
                player = new Player
                {
                    Name = playerName,
                    Email = $"{playerName.ToLower()}@chess.local", // Generate a unique email
                    Rating = 1200, // Starting ELO
                    TotalGamesPlayed = 0,
                    TotalWins = 0,
                    TotalLosses = 0,
                    TotalDraws = 0
                };
                context.Players.Add(player);
                await context.SaveChangesAsync();
            }
            
            return player;
        }

        private async Task SavePlayerRatings(Player whitePlayer, Player blackPlayer)
        {
            using var context = new ChessDbContext();
            
            var whitePlayerEntity = await context.Players.FindAsync(whitePlayer.Id);
            var blackPlayerEntity = await context.Players.FindAsync(blackPlayer.Id);
            
            if (whitePlayerEntity != null)
            {
                whitePlayerEntity.Rating = whitePlayer.Rating;
                whitePlayerEntity.TotalGamesPlayed++;
            }
            
            if (blackPlayerEntity != null)
            {
                blackPlayerEntity.Rating = blackPlayer.Rating;
                blackPlayerEntity.TotalGamesPlayed++;
            }
            
            await context.SaveChangesAsync();
        }

        private async Task SaveGameResult(Player whitePlayer, Player blackPlayer, GameOutcome outcome, int whiteChange, int blackChange)
        {
            using var context = new ChessDbContext();
            
            var gameResult = new GameResultInfo
            {
                WhitePlayerId = whitePlayer.Id,
                BlackPlayerId = blackPlayer.Id,
                Result = outcome,
                WhiteRatingBefore = whitePlayer.Rating - whiteChange,
                WhiteRatingAfter = whitePlayer.Rating,
                BlackRatingBefore = blackPlayer.Rating - blackChange,
                BlackRatingAfter = blackPlayer.Rating,
                MoveCount = _game?.MoveHistory.Count ?? 0,
                GameDuration = TimeSpan.Zero, // Could be calculated if we track start time
                PlayedDate = DateTime.Now
            };
            
            context.GameResults.Add(gameResult);
            await context.SaveChangesAsync();
        }

        private async Task ShowGameEndDialog(GameOutcome outcome, GameState state)
        {
            string title = state switch
            {
                GameState.Checkmate => "Échec et Mat !",
                GameState.Stalemate => "Pat - Match nul !",
                GameState.Draw => "Match nul !",
                _ => "Fin de partie"
            };

            string content = outcome switch
            {
                GameOutcome.WhiteWins => $"{_gameInfo?.Player1Name} gagne !",
                GameOutcome.BlackWins => $"{_gameInfo?.Player2Name} gagne !",
                GameOutcome.Draw => "Match nul",
                _ => "Partie terminée"
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "Retour au menu",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void OnBackToMenuClick(object sender, RoutedEventArgs e)
        {
            BackToMenuRequested?.Invoke(this, e);
        }
    }
}
