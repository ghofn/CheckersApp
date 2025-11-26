using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CheckersApp
{
    public class CellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BoardCell cell)
            {
                if (IsSelectedChecker(cell))
                {
                    return new SolidColorBrush(Colors.LightBlue);
                }

                if (IsPossibleMove(cell))
                {
                    return new SolidColorBrush(cell.Color == Colors.Orange ? Colors.Orange : Colors.LightGreen);
                }

                if (cell.IsLastMove && !cell.HasPiece)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)); 
                }

                return new SolidColorBrush(cell.Color);
            }

            return Brushes.Transparent;
        }

        private bool IsSelectedChecker(BoardCell cell)
        {
            return cell.Color == Colors.LightBlue;
        }

        private bool IsPossibleMove(BoardCell cell)
        {
            return cell.Color == Colors.Orange || cell.Color == Colors.LightGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}