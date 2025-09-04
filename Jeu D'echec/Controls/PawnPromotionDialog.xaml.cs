using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Jeu_D_echec.Models;

namespace Jeu_D_echec.Controls
{
    public sealed partial class PawnPromotionDialog : UserControl
    {
        public event EventHandler<PieceType>? PromotionSelected;

        public PieceColor PawnColor { get; set; }

        public PawnPromotionDialog()
        {
            InitializeComponent();
        }

        public PawnPromotionDialog(PieceColor pawnColor) : this()
        {
            PawnColor = pawnColor;
            UpdatePieceColors();
        }

        private void UpdatePieceColors()
        {
            // Update the piece symbols based on the pawn color
            var queenSymbol = PawnColor == PieceColor.White ? "♕" : "♛";
            var rookSymbol = PawnColor == PieceColor.White ? "♖" : "♜";
            var bishopSymbol = PawnColor == PieceColor.White ? "♗" : "♝";
            var knightSymbol = PawnColor == PieceColor.White ? "♘" : "♞";

            // Update the text blocks inside the buttons
            if (QueenButton.Content is TextBlock queenText)
                queenText.Text = queenSymbol;
            if (RookButton.Content is TextBlock rookText)
                rookText.Text = rookSymbol;
            if (BishopButton.Content is TextBlock bishopText)
                bishopText.Text = bishopSymbol;
            if (KnightButton.Content is TextBlock knightText)
                knightText.Text = knightSymbol;
        }

        private void OnPromotionSelected(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pieceTypeString)
            {
                var pieceType = pieceTypeString switch
                {
                    "Queen" => PieceType.Queen,
                    "Rook" => PieceType.Rook,
                    "Bishop" => PieceType.Bishop,
                    "Knight" => PieceType.Knight,
                    _ => PieceType.Queen
                };

                // Highlight the selected button
                ClearButtonSelection();
                button.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightBlue);
                
                // Store the selection but don't close the dialog yet
                _selectedPieceType = pieceType;
                System.Diagnostics.Debug.WriteLine($"Promotion selected: {pieceType}");
            }
        }

        private PieceType? _selectedPieceType = null;

        private void ClearButtonSelection()
        {
            // Reset to default background color
            var defaultBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            QueenButton.Background = defaultBrush;
            RookButton.Background = defaultBrush;
            BishopButton.Background = defaultBrush;
            KnightButton.Background = defaultBrush;
        }

        public PieceType? GetSelectedPieceType()
        {
            return _selectedPieceType;
        }
    }
}
