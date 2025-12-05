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
        public bool IsSelected { get; set; }
        public bool IsPossibleMove { get; set; }

        // Методы для работы со скинами
        public Color GetPieceColor()
        {
            if (PieceColor == Colors.White)
                return SkinManager.GetPieceColor(PieceColor);
            else if (PieceColor == Colors.Black)
                return SkinManager.GetPieceColor(PieceColor);
            return PieceColor;
        }

        public Color GetPieceBorderColor()
        {
            if (PieceBorderColor == Colors.Gray || PieceBorderColor == Colors.White)
                return SkinManager.GetPieceBorderColor(PieceBorderColor);
            else if (PieceBorderColor == Colors.DarkSlateGray || PieceBorderColor == Colors.Black)
                return SkinManager.GetPieceBorderColor(PieceBorderColor);
            return PieceBorderColor;
        }

        public Color GetKingColor()
        {
            return SkinManager.GetKingColor();
        }

        public string GetKingSymbol()
        {
            return SkinManager.GetKingSymbol();
        }
    }
}