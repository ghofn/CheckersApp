using System.Windows.Media;

namespace CheckersApp
{
    public class BoardCell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public Color Color { get; set; }
        public bool HasPiece { get; set; }
        public Color PieceColor { get; set; }
        public Color PieceBorderColor { get; set; }
        public bool IsKing { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsLastMove { get; set; }
    }
}