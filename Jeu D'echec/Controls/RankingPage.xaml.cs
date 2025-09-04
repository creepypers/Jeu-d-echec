using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Jeu_D_echec.Models;
using Jeu_D_echec.Services;

namespace Jeu_D_echec.Controls
{
    public sealed partial class RankingPage : UserControl
    {
        public event EventHandler? BackRequested;

        private readonly IRankingService _rankingService;
        private ObservableCollection<PlayerRankingViewModel> _rankings;

        public RankingPage()
        {
            InitializeComponent();
            _rankings = new ObservableCollection<PlayerRankingViewModel>();
            RankingsListView.ItemsSource = _rankings;

            // Initialize ranking service
            try
            {
                var context = new Data.ChessDbContext();
                _rankingService = new RankingService(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing ranking service: {ex.Message}");
                // Fallback to null service
                _rankingService = null!;
            }

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadRankingsAsync();
        }



        private async Task LoadRankingsAsync()
        {
            if (_rankingService == null)
            {
                ShowNoDataMessage();
                return;
            }

            try
            {
                var players = await _rankingService.GetRankingsAsync();
                
                _rankings.Clear();
                
                if (players.Any())
                {
                    int rank = 1;
                    foreach (var player in players)
                    {
                        var viewModel = new PlayerRankingViewModel
                        {
                            Rank = rank++,
                            Player = player,
                            Rating = player.Rating
                        };
                        _rankings.Add(viewModel);
                    }
                    
                    RankingsListView.Visibility = Visibility.Visible;
                    NoDataText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowNoDataMessage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading rankings: {ex.Message}");
                ShowNoDataMessage();
            }
        }

        private void ShowNoDataMessage()
        {
            RankingsListView.Visibility = Visibility.Collapsed;
            NoDataText.Visibility = Visibility.Visible;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class PlayerRankingViewModel
    {
        public int Rank { get; set; }
        public Player Player { get; set; } = null!;
        public int Rating { get; set; }

        public string Name => Player.Name;
        public string Email => Player.Email;
        public int CurrentRating => Rating;
        public int TotalGamesPlayed => Player.TotalGamesPlayed;
        public string WinRateText => $"{Player.WinRate:F1}%";
        public string Category => ELOService.GetELOCategory(Rating);
        public string RatingColor => ELOService.GetELOCategoryColor(Rating);
        public string CategoryColor => ELOService.GetELOCategoryColor(Rating);
    }
}
