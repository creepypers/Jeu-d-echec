using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Jeu_D_echec.Models;

namespace Jeu_D_echec.Controls
{
    public sealed partial class ChessSquare : UserControl
    {
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(Position), typeof(ChessSquare),
                new PropertyMetadata(default(Position)));

        public static readonly DependencyProperty PieceProperty =
            DependencyProperty.Register(nameof(Piece), typeof(ChessPiece), typeof(ChessSquare),
                new PropertyMetadata(null, OnPieceChanged));

        public static readonly DependencyProperty SquareColorProperty =
            DependencyProperty.Register(nameof(SquareColor), typeof(Brush), typeof(ChessSquare),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ChessSquare),
                new PropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty IsValidMoveProperty =
            DependencyProperty.Register(nameof(IsValidMove), typeof(bool), typeof(ChessSquare),
                new PropertyMetadata(false, OnIsValidMoveChanged));

        public static readonly DependencyProperty IsInCheckProperty =
            DependencyProperty.Register(nameof(IsInCheck), typeof(bool), typeof(ChessSquare),
                new PropertyMetadata(false, OnIsInCheckChanged));

        public Position Position
        {
            get => (Position)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public ChessPiece? Piece
        {
            get => (ChessPiece?)GetValue(PieceProperty);
            set => SetValue(PieceProperty, value);
        }

        public Brush? SquareColor
        {
            get => (Brush?)GetValue(SquareColorProperty);
            set => SetValue(SquareColorProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsValidMove
        {
            get => (bool)GetValue(IsValidMoveProperty);
            set => SetValue(IsValidMoveProperty, value);
        }

        public bool IsInCheck
        {
            get => (bool)GetValue(IsInCheckProperty);
            set => SetValue(IsInCheckProperty, value);
        }

        public Brush? PieceColor
        {
            get
            {
                if (Piece == null) return null;
                return Piece.Color == Models.PieceColor.White ? 
                    new SolidColorBrush(Microsoft.UI.Colors.White) : 
                    new SolidColorBrush(Microsoft.UI.Colors.Black);
            }
        }

        public event EventHandler<ChessSquareClickedEventArgs>? SquareClicked;

        public ChessSquare()
        {
            InitializeComponent();
        }

        private static void OnPieceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChessSquare square)
            {
                square.OnPieceChanged(e.OldValue as ChessPiece, e.NewValue as ChessPiece);
            }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChessSquare square)
            {
                square.OnIsSelectedChanged((bool)e.OldValue, (bool)e.NewValue);
            }
        }

        private static void OnIsValidMoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChessSquare square)
            {
                square.OnIsValidMoveChanged((bool)e.OldValue, (bool)e.NewValue);
            }
        }

        private static void OnIsInCheckChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChessSquare square)
            {
                square.OnIsInCheckChanged((bool)e.OldValue, (bool)e.NewValue);
            }
        }

        private void OnPieceChanged(ChessPiece? oldPiece, ChessPiece? newPiece)
        {
            System.Diagnostics.Debug.WriteLine($"OnPieceChanged: {oldPiece?.Type} {oldPiece?.Color} -> {newPiece?.Type} {newPiece?.Color}");
            
            // Force the TextBlock to update directly
            PieceText.Text = newPiece?.UnicodeSymbol ?? "";
            
            // Update the color based on piece color
            if (newPiece != null)
            {
                PieceText.Foreground = newPiece.Color == Models.PieceColor.White ? 
                    new SolidColorBrush(Microsoft.UI.Colors.White) : 
                    new SolidColorBrush(Microsoft.UI.Colors.Black);
            }
            else
            {
                PieceText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }

        private void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                SelectionAnimation.Begin();
            }
            else
            {
                SelectionBorder.Opacity = 0.0;
                // Reset transform
                var transform = (CompositeTransform)SelectionBorder.RenderTransform;
                transform.ScaleX = 0.8;
                transform.ScaleY = 0.8;
            }
        }

        private void OnIsValidMoveChanged(bool oldValue, bool newValue)
        {

            if (newValue)
            {
                // Force immediate visibility first
                ValidMoveIndicator.Opacity = 1.0;
                var transform = (CompositeTransform)ValidMoveIndicator.RenderTransform;
                transform.ScaleX = 1.0;
                transform.ScaleY = 1.0;
                
                // Then start animation
                ValidMoveAnimation.Begin();
            }
            else
            {
                ValidMoveIndicator.Opacity = 0.0;
                // Reset transform
                var transform = (CompositeTransform)ValidMoveIndicator.RenderTransform;
                transform.ScaleX = 0.0;
                transform.ScaleY = 0.0;
            }
        }

        private void OnIsInCheckChanged(bool oldValue, bool newValue)
        {
            // Simplified to avoid recursion
            if (newValue)
            {
                CheckBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
            else
            {
                CheckBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SquareClicked?.Invoke(this, new ChessSquareClickedEventArgs(Position));
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!IsSelected)
            {
                HoverBorder.Opacity = 0.1;
            }
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            HoverBorder.Opacity = 0.0;
        }
    }

    public class ChessSquareClickedEventArgs : EventArgs
    {
        public Position Position { get; }

        public ChessSquareClickedEventArgs(Position position)
        {
            Position = position;
        }
    }
}
