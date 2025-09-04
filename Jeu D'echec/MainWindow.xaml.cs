using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Jeu_D_echec.Models;
using Jeu_D_echec.Controls;
using Jeu_D_echec.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Jeu_D_echec
{
    /// <summary>
    /// Main window for the Chess game application.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private HomePage? _homePage;
        private GamePage? _currentGamePage;
        private RankingPage? _rankingPage;

        public MainWindow()
        {
            InitializeComponent();
            // Delay navigation to ensure XAML is fully loaded
            this.Activated += OnMainWindowActivated;
        }

        private void OnMainWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            // Only navigate once when the window is first activated
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                this.Activated -= OnMainWindowActivated;
                InitializeNavigation();
            }
        }

        private void InitializeNavigation()
        {
            // Navigate to home page
            NavigateToHomePage();
        }

        private void NavigateToHomePage()
        {
            try
            {
                // Ensure MainFrame is available before attempting navigation
                if (MainFrame == null)
                {
                    System.Diagnostics.Debug.WriteLine("MainFrame is null, cannot navigate to home page");
                    return;
                }

                if (_homePage == null)
                {
                    _homePage = new HomePage();
                    _homePage.StartNewGameRequested += OnStartNewGameRequested;
                    _homePage.ResumeGameRequested += OnResumeGameRequested;
                    _homePage.ViewRankingsRequested += OnViewRankingsRequested;
                }

                // Instead of navigation, set content directly since HomePage is a UserControl
                MainFrame.Content = _homePage;


                
                // Load saved games when navigating to home page
                if (_homePage != null)
                {
                    _homePage.LoadOnStart();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to home page: {ex.Message}");
                // Fallback: try to set content directly
                try
                {
                    if (MainFrame != null && _homePage != null)
                    {
                        MainFrame.Content = _homePage;
                    }
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Fallback navigation also failed: {fallbackEx.Message}");
                }
            }
        }

        private void NavigateToGamePage(object parameter)
        {
            // Ensure MainFrame is available before attempting navigation
            if (MainFrame == null)
            {
                System.Diagnostics.Debug.WriteLine("MainFrame is null, cannot navigate to game page");
                return;
            }

            _currentGamePage = new GamePage();
            _currentGamePage.BackToMenuRequested += OnBackToMenuRequested;
            _currentGamePage.GameSaved += OnGameSaved;

            // Initialize the game page with the parameter
            if (parameter is GameInfo gameInfo)
            {
                _currentGamePage.InitializeWithGameInfo(gameInfo);
            }
            else if (parameter is SavedGameInfo savedGameInfo)
            {
                _currentGamePage.InitializeWithSavedGame(savedGameInfo);
            }

            // Set content directly instead of navigation
            MainFrame.Content = _currentGamePage;
        }

        private void OnStartNewGameRequested(object? sender, StartNewGameEventArgs e)
        {
            NavigateToGamePage(e.GameInfo);
        }

        private void OnResumeGameRequested(object? sender, ResumeGameEventArgs e)
        {
            NavigateToGamePage(e.SavedGame);
        }





        private void OnViewRankingsRequested(object? sender, EventArgs e)
        {
            NavigateToRankingPage();
        }

        private void NavigateToRankingPage()
        {
            if (MainFrame == null) return;

            if (_rankingPage == null)
            {
                _rankingPage = new RankingPage();
                _rankingPage.BackRequested += OnRankingBackRequested;
            }

            MainFrame.Content = _rankingPage;
        }

        private void OnRankingBackRequested(object? sender, EventArgs e)
        {
            NavigateToHomePage();
            if (_homePage != null)
            {
                _homePage.RefreshSavedGames();
            }
        }

        private void OnBackToMenuRequested(object? sender, RoutedEventArgs e)
        {
            NavigateToHomePage();
            if (_homePage != null)
            {
                _homePage.RefreshSavedGames();
            }
        }

        private void OnGameSaved(object? sender, RoutedEventArgs e)
        {
            // Refresh saved games list when a game is saved
            if (_homePage != null)
            {
                _homePage.RefreshSavedGames();
            }
        }



        private void OnHelpRequested(object? sender, EventArgs e)
        {
            // TODO: Implement help dialog
            ShowMessage("Aide - Fonctionnalité à venir");
        }

        private void OnNewGameRequested(object? sender, RoutedEventArgs e)
        {
            // Navigate to home page to start a new game
            NavigateToHomePage();
        }

        private void ShowMessage(string message)
        {
            // Simple message display - in a real app, you might want to use a proper dialog
            System.Diagnostics.Debug.WriteLine($"Message: {message}");
            // TODO: Implement proper message display (ContentDialog or Toast)
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation failed: {e.Exception?.Message}");
            // Fallback to home page
            NavigateToHomePage();
        }

        private void MainGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Handle keyboard shortcuts
            if (e.Key == Windows.System.VirtualKey.F1)
            {
                OnHelpRequested(this, EventArgs.Empty);
            }
            else if (e.Key == Windows.System.VirtualKey.N && 
                     Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                OnNewGameRequested(this, new RoutedEventArgs());
            }

        }
    }
}
