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
                if (cell.IsSelected)
                    return new SolidColorBrush(Colors.LightBlue);

                if (cell.IsPossibleMove)
                {
                    if (cell.HasPiece)
                        return new SolidColorBrush(Colors.Red);
                    return new SolidColorBrush(Colors.LightGreen);
                }

                if (cell.IsLastMove)
                    return new SolidColorBrush(Color.FromArgb(255, 255, 215, 0));

                return new SolidColorBrush(cell.Color);
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}