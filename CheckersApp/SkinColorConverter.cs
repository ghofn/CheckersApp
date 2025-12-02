using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CheckersApp
{
    public class SkinColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BoardCell cell && parameter is string param)
            {
                switch (param)
                {
                    case "PieceColor":
                        return new SolidColorBrush(cell.GetPieceColor());
                    case "PieceBorderColor":
                        return new SolidColorBrush(cell.GetPieceBorderColor());
                    case "KingColor":
                        return new SolidColorBrush(cell.GetKingColor());
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}