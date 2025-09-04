using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using Jeu_D_echec.Models;
using System.Threading.Tasks;

namespace Jeu_D_echec.Controls
{
    public sealed partial class ChessBoard : UserControl
    {
        private ChessSquare[,] _squares = new ChessSquare[8, 8];
        private ChessGame? _game;
        private Position? _selectedPosition;
        private List<Position> _validMoves = new List<Position>();

        public static readonly DependencyProperty GameProperty =
            DependencyProperty.Register(nameof(Game), typeof(ChessGame), typeof(ChessBoard), 
                new PropertyMetadata(null, OnGameChanged));

        public ChessGame? Game
        {
            get => (ChessGame?)GetValue(GameProperty);
            set => SetValue(GameProperty, value);
        }

        public event EventHandler<ChessMoveEventArgs>? MoveMade;

        public ChessBoard()
        {
            InitializeComponent();
            InitializeSquares();
            // BoardEntranceAnimation.Begin(); // Supprimé pour affichage immédiat
        }

        private static void OnGameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChessBoard board)
            {
                board.OnGameChanged(e.OldValue as ChessGame, e.NewValue as ChessGame);
            }
        }

        private void OnGameChanged(ChessGame? oldGame, ChessGame? newGame)
        {
            if (oldGame != null)
            {
                oldGame.PieceSelected -= OnPieceSelected;
                oldGame.MoveMade -= OnMoveMade;
                oldGame.GameStateChanged -= OnGameStateChanged;
            }

            _game = newGame;
            if (_game != null)
            {
                _game.PieceSelected += OnPieceSelected;
                _game.MoveMade += OnMoveMade;
                _game.GameStateChanged += OnGameStateChanged;
                UpdateBoard();
            }
        }

        private void InitializeSquares()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var square = new ChessSquare
                    {
                        Position = new Position(row, col),
                        SquareColor = (row + col) % 2 == 0 ? 
                            (Brush)Resources["LightSquareBrush"] : 
                            (Brush)Resources["DarkSquareBrush"]
                    };

                    square.SquareClicked += OnSquareClicked;

                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, col);
                    ChessGrid.Children.Add(square);

                    _squares[row, col] = square;
                }
            }
        }

        private void OnSquareClicked(object sender, ChessSquareClickedEventArgs e)
        {
            if (_game == null) 
            {
                // System.Diagnostics.Debug.WriteLine("OnSquareClicked: _game is null");
                return;
            }

            if (e?.Position == null)
            {
                // System.Diagnostics.Debug.WriteLine("OnSquareClicked: Position is null");
                return;
            }

            // System.Diagnostics.Debug.WriteLine($"OnSquareClicked: Clicked position {e.Position.Row},{e.Position.Column}");
            // System.Diagnostics.Debug.WriteLine($"OnSquareClicked: Current player {_game.CurrentPlayer}");
            // System.Diagnostics.Debug.WriteLine($"OnSquareClicked: Selected position {_selectedPosition?.Row},{_selectedPosition?.Column}");
            // System.Diagnostics.Debug.WriteLine($"OnSquareClicked: Valid moves count {_validMoves.Count}");

            if (_selectedPosition.HasValue)
            {
                // System.Diagnostics.Debug.WriteLine($"OnSquareClicked: Has selected position, checking if move is valid");
                // Try to make a move
                if (_validMoves.Contains(e.Position) && _selectedPosition.HasValue)
                {
                    // System.Diagnostics.Debug.WriteLine($"OnSquareClicked: Move is valid, attempting to make move from {_selectedPosition.Value.Row},{_selectedPosition.Value.Column} to {e.Position.Row},{e.Position.Column}");
                    var fromPosition = _selectedPosition.Value; // Sauvegarder la position avant MakeMove
                    var pieceToAnimate = _squares[fromPosition.Row, fromPosition.Column].Piece; // Sauvegarder la pièce avant le mouvement
                    var capturedPiece = _squares[e.Position.Row, e.Position.Column].Piece; // Sauvegarder la pièce capturée avant le mouvement
                    
                    if (_game.MakeMove(e.Position))
                    {
                        // System.Diagnostics.Debug.WriteLine("OnSquareClicked: Move successful! Updating interface");
                        
                        // Mettre à jour l'interface immédiatement - pas d'animation pour l'instant
                        UpdateSelection();
                        
                        // Animation temporairement désactivée pour éviter les conflits
                        // AnimateMoveWithPiece(fromPosition, e.Position, pieceToAnimate, capturedPiece);
                        
                        MoveMade?.Invoke(this, new ChessMoveEventArgs(fromPosition, e.Position));
                        // System.Diagnostics.Debug.WriteLine("OnSquareClicked: Move completed successfully");
                    }
                    else
                    {
                        // Check if this is a pawn promotion case
                        if (pieceToAnimate?.Type == PieceType.Pawn && (e.Position.Row == 0 || e.Position.Row == 7))
                        {
                            // System.Diagnostics.Debug.WriteLine("OnSquareClicked: Pawn promotion required, showing dialog");
                            // Handle pawn promotion asynchronously
                            _ = HandlePawnPromotion(fromPosition, e.Position, pieceToAnimate, capturedPiece);
                        }
                        else
                        {
                            // System.Diagnostics.Debug.WriteLine("OnSquareClicked: Move failed in _game.MakeMove");
                        }
                    }
                }
                else
                {
                    // System.Diagnostics.Debug.WriteLine($"OnSquareClicked: Move not valid, trying to select new piece at {e.Position.Row},{e.Position.Column}");
                    // Try to select a new piece
                    if (_game.SelectPiece(e.Position))
                    {
                        // System.Diagnostics.Debug.WriteLine("OnSquareClicked: New piece selected successfully");
                        UpdateSelection();
                    }
                    else
                    {
                        // System.Diagnostics.Debug.WriteLine("OnSquareClicked: Failed to select new piece");
                    }
                }
            }
            else
            {
                // System.Diagnostics.Debug.WriteLine($"OnSquareClicked: No selected position, trying to select piece at {e.Position.Row},{e.Position.Column}");
                // Try to select a piece
                if (_game.SelectPiece(e.Position))
                {
                    // System.Diagnostics.Debug.WriteLine("OnSquareClicked: Piece selected successfully");
                    UpdateSelection();
                }
                else
                {
                    // System.Diagnostics.Debug.WriteLine("OnSquareClicked: Failed to select piece");
                }
            }
        }

        private void OnPieceSelected(object sender, PieceSelectedEventArgs e)
        {
            UpdateSelection();
        }

        private void OnMoveMade(object sender, MoveMadeEventArgs e)
        {
            UpdateBoard();
            UpdateSelection();
        }

        private void OnGameStateChanged(object sender, GameStateChangedEventArgs e)
        {
            UpdateBoard();
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (_game == null) 
            {
                // System.Diagnostics.Debug.WriteLine("UpdateSelection: _game is null");
                return;
            }

            _selectedPosition = _game.SelectedPosition;
            _validMoves = _game.ValidMoves.ToList();

            // System.Diagnostics.Debug.WriteLine($"UpdateSelection: Selected position {_selectedPosition?.Row},{_selectedPosition?.Column}");
            // System.Diagnostics.Debug.WriteLine($"UpdateSelection: Valid moves count {_validMoves.Count}");
            foreach (var move in _validMoves)
            {
                // System.Diagnostics.Debug.WriteLine($"UpdateSelection: Valid move {move.Row},{move.Column}");
            }

            // Update board pieces first
            // System.Diagnostics.Debug.WriteLine("UpdateSelection: Calling UpdateBoard");
            UpdateBoard();

            // Update all squares
            // System.Diagnostics.Debug.WriteLine("UpdateSelection: Updating square states");
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var position = new Position(row, col);
                    var square = _squares[row, col];

                    square.IsSelected = _selectedPosition == position;
                    square.IsValidMove = _validMoves.Contains(position);
                }
            }
            // System.Diagnostics.Debug.WriteLine("UpdateSelection: Completed");
        }

        private void UpdateBoard()
        {
            if (_game?.Board == null) 
            {
                // System.Diagnostics.Debug.WriteLine("UpdateBoard: _game or _game.Board is null");
                return;
            }

            // System.Diagnostics.Debug.WriteLine("UpdateBoard: Starting to update board pieces");
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var position = new Position(row, col);
                    var square = _squares[row, col];
                    var piece = _game.Board[position];

                    var oldPiece = square.Piece;
                    square.Piece = piece;
                    square.IsInCheck = IsKingInCheck(position, piece);

                    if (oldPiece != piece)
                    {
                        // System.Diagnostics.Debug.WriteLine($"UpdateBoard: Piece changed at {row},{col} from {oldPiece?.Type} {oldPiece?.Color} to {piece?.Type} {piece?.Color}");
                    }
                }
            }
            // System.Diagnostics.Debug.WriteLine("UpdateBoard: Completed");
        }

        private bool IsKingInCheck(Position position, ChessPiece? piece)
        {
            if (piece?.Type != PieceType.King || _game == null)
                return false;

            return _game.Board.IsInCheck(piece.Color);
        }

        private void AnimateMoveWithPiece(Position from, Position to, ChessPiece? piece, ChessPiece? capturedPiece = null)
        {
            // System.Diagnostics.Debug.WriteLine($"AnimateMoveWithPiece: Starting animation from {from.Row},{from.Column} to {to.Row},{to.Column}");
            
            if (from.Row < 0 || from.Row >= 8 || from.Column < 0 || from.Column >= 8 ||
                to.Row < 0 || to.Row >= 8 || to.Column < 0 || to.Column >= 8)
            {
                // System.Diagnostics.Debug.WriteLine("AnimateMoveWithPiece: Invalid position coordinates");
                return;
            }
            
            var fromSquare = _squares[from.Row, from.Column];
            var toSquare = _squares[to.Row, to.Column];

            if (piece == null) 
            {
                // System.Diagnostics.Debug.WriteLine("AnimateMoveWithPiece: No piece provided");
                return;
            }

            // System.Diagnostics.Debug.WriteLine($"AnimateMoveWithPiece: Moving {piece.Type} {piece.Color}");
            // System.Diagnostics.Debug.WriteLine($"AnimateMoveWithPiece: Captured piece: {capturedPiece?.Type} {capturedPiece?.Color}");

            // Check if there's a capture using the saved captured piece
            if (capturedPiece != null)
            {
                // System.Diagnostics.Debug.WriteLine($"AnimateMoveWithPiece: Capturing {capturedPiece.Type} {capturedPiece.Color}");
                // Temporarily disable capture animation to avoid conflicts
                // AnimateCapture(toSquare, capturedPiece);
                // System.Diagnostics.Debug.WriteLine("AnimateMoveWithPiece: Capture animation temporarily disabled");
            }
            else
            {
                // System.Diagnostics.Debug.WriteLine("AnimateMoveWithPiece: No capture - simple move");
            }

            // Hide the piece at the destination temporarily during animation
            var originalOpacity = toSquare.Opacity;
            toSquare.Opacity = 0.0;

            // Set up the animated piece
            AnimatedPiece.Text = piece.UnicodeSymbol;
            AnimatedPiece.Foreground = piece.Color == PieceColor.White ? 
                new SolidColorBrush(Microsoft.UI.Colors.White) : 
                new SolidColorBrush(Microsoft.UI.Colors.Black);
            AnimatedPiece.Opacity = 1.0;

            // Calculate positions
            var fromRect = fromSquare.TransformToVisual(ChessGrid).TransformBounds(
                new Windows.Foundation.Rect(0, 0, fromSquare.ActualWidth, fromSquare.ActualHeight));
            var toRect = toSquare.TransformToVisual(ChessGrid).TransformBounds(
                new Windows.Foundation.Rect(0, 0, toSquare.ActualWidth, toSquare.ActualHeight));

            // Position the animated piece
            var transformGroup = (TransformGroup)AnimatedPiece.RenderTransform;
            var translateTransform = (TranslateTransform)transformGroup.Children[0];
            var scaleTransform = (CompositeTransform)transformGroup.Children[1];
            
            translateTransform.X = fromRect.X + fromRect.Width / 2 - 20;
            translateTransform.Y = fromRect.Y + fromRect.Height / 2 - 20;

            // Set up the animation
            MoveXAnimation.From = translateTransform.X;
            MoveXAnimation.To = toRect.X + toRect.Width / 2 - 20;
            MoveYAnimation.From = translateTransform.Y;
            MoveYAnimation.To = toRect.Y + toRect.Height / 2 - 20;

            Storyboard.SetTarget(MoveXAnimation, translateTransform);
            Storyboard.SetTargetProperty(MoveXAnimation, "X");
            Storyboard.SetTarget(MoveYAnimation, translateTransform);
            Storyboard.SetTargetProperty(MoveYAnimation, "Y");
            
            Storyboard.SetTarget(MoveScaleAnimation, scaleTransform);
            Storyboard.SetTargetProperty(MoveScaleAnimation, "ScaleX");
            Storyboard.SetTarget(MoveScaleYAnimation, scaleTransform);
            Storyboard.SetTargetProperty(MoveScaleYAnimation, "ScaleY");

            // Stop any existing move animation first
            MoveAnimation.Stop();
            
            // Start the animation
            MoveAnimation.Completed += OnMoveAnimationCompleted;
            MoveAnimation.Begin();
        }

        private void AnimateMove(Position from, Position to)
        {
            // Legacy method - redirect to new method
            // Note: This method should not be used as it can't properly detect captures
            // after the model has been updated. Use AnimateMoveWithPiece directly.
            // System.Diagnostics.Debug.WriteLine("WARNING: AnimateMove legacy method called - this may cause issues");
            var fromSquare = _squares[from.Row, from.Column];
            var piece = fromSquare.Piece;
            
            // Simple animation without capture detection to avoid conflicts
            if (piece != null)
            {
                // System.Diagnostics.Debug.WriteLine($"AnimateMove legacy: Moving {piece.Type} {piece.Color} without capture detection");
                AnimateMoveWithPiece(from, to, piece, null); // No capture detection in legacy method
            }
        }

        private void AnimateCapture(ChessSquare capturedSquare, ChessPiece? capturedPiece = null)
        {
            if (capturedPiece == null) 
            {
                capturedPiece = capturedSquare.Piece;
                if (capturedPiece == null) 
                {
                    // System.Diagnostics.Debug.WriteLine("AnimateCapture: No piece to capture, skipping animation");
                    return;
                }
            }

            // System.Diagnostics.Debug.WriteLine($"AnimateCapture: Starting capture animation for {capturedPiece.Type} {capturedPiece.Color}");

            // Create a temporary element for capture animation
            var captureElement = new TextBlock
            {
                Text = capturedPiece.UnicodeSymbol,
                FontSize = 32,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = capturedPiece.Color == PieceColor.White ? 
                    new SolidColorBrush(Microsoft.UI.Colors.White) : 
                    new SolidColorBrush(Microsoft.UI.Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5)
            };

            captureElement.RenderTransform = new CompositeTransform();

            // Position the capture element
            var rect = capturedSquare.TransformToVisual(ChessGrid).TransformBounds(
                new Windows.Foundation.Rect(0, 0, capturedSquare.ActualWidth, capturedSquare.ActualHeight));
            
            Canvas.SetLeft(captureElement, rect.X + rect.Width / 2 - 16);
            Canvas.SetTop(captureElement, rect.Y + rect.Height / 2 - 16);

            // Add to a temporary canvas or overlay
            var overlay = new Canvas();
            overlay.Children.Add(captureElement);
            BoardContainer.Children.Add(overlay);

            // Stop any existing capture animation first
            CaptureAnimation.Stop();

            // Set up capture animation
            CaptureScaleAnimation.From = 1.0;
            CaptureScaleAnimation.To = 0.0;
            CaptureRotationAnimation.From = 0;
            CaptureRotationAnimation.To = 180;

            Storyboard.SetTarget(CaptureScaleAnimation, captureElement.RenderTransform);
            Storyboard.SetTargetProperty(CaptureScaleAnimation, "ScaleX");
            Storyboard.SetTarget(CaptureScaleYAnimation, captureElement.RenderTransform);
            Storyboard.SetTargetProperty(CaptureScaleYAnimation, "ScaleY");
            Storyboard.SetTarget(CaptureRotationAnimation, captureElement.RenderTransform);
            Storyboard.SetTargetProperty(CaptureRotationAnimation, "Rotation");

            CaptureAnimation.Completed += (s, e) =>
            {
                BoardContainer.Children.Remove(overlay);
            };

            CaptureAnimation.Begin();
        }

        private void OnMoveAnimationCompleted(object sender, object e)
        {
            MoveAnimation.Completed -= OnMoveAnimationCompleted;
            AnimatedPiece.Opacity = 0.0;
            
            // Restaurer l'opacité de toutes les cases (au cas où certaines seraient cachées)
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    _squares[row, col].Opacity = 1.0;
                }
            }
            
            // System.Diagnostics.Debug.WriteLine("OnMoveAnimationCompleted: Animation finished, board restored");
        }
        private async Task HandlePawnPromotion(Position fromPosition, Position toPosition, ChessPiece? piece, ChessPiece? capturedPiece)
        {
            // System.Diagnostics.Debug.WriteLine($"HandlePawnPromotion: Promoting pawn at {fromPosition.Row},{fromPosition.Column} to {toPosition.Row},{toPosition.Column}");
            
            var promotionDialog = new PawnPromotionDialog(piece?.Color ?? PieceColor.White);
            var tcs = new TaskCompletionSource<PieceType>();
            
            // Subscribe to the promotion selected event
            promotionDialog.PromotionSelected += (sender, pieceType) =>
            {
                tcs.SetResult(pieceType);
                // System.Diagnostics.Debug.WriteLine($"Promotion selected: {pieceType}");
            };
            
            // Create a ContentDialog
            var dialog = new ContentDialog
            {
                Title = "Promotion du pion",
                Content = promotionDialog,
                PrimaryButtonText = "Confirmer",
                SecondaryButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            // Show the dialog
            var dialogTask = dialog.ShowAsync().AsTask();
            
            // Wait for either a piece selection or dialog completion
            var completedTask = await Task.WhenAny(dialogTask, tcs.Task);
            
            PieceType selectedPieceType;
            
            if (completedTask == tcs.Task)
            {
                // Piece was selected, close the dialog
                selectedPieceType = await tcs.Task;
                dialog.Hide();
            }
            else
            {
                // Dialog was closed without piece selection
                var result = await dialogTask;
                if (result == ContentDialogResult.Primary)
                {
                    var selectedPiece = promotionDialog.GetSelectedPieceType();
                    selectedPieceType = selectedPiece ?? PieceType.Queen; // Default to Queen if no selection
                }
                else
                {
                    selectedPieceType = PieceType.Queen; // Default to Queen if cancelled
                }
            }
            
            // Make the move with promotion
            // System.Diagnostics.Debug.WriteLine($"Making move with promotion to {selectedPieceType}");
            if (_game.MakeMoveWithPromotion(toPosition, selectedPieceType))
            {
                // System.Diagnostics.Debug.WriteLine("Promotion move successful! Updating interface");
                UpdateSelection();
                MoveMade?.Invoke(this, new ChessMoveEventArgs(fromPosition, toPosition));
            }
            else
            {
                // System.Diagnostics.Debug.WriteLine("Promotion move failed");
            }
        }
    }

    public class ChessMoveEventArgs : EventArgs
    {
        public Position From { get; }
        public Position To { get; }

        public ChessMoveEventArgs(Position from, Position to)
        {
            From = from;
            To = to;
        }
    }
}
