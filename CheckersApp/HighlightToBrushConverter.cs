using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CheckersApp
{
    public class HighlightToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is Color baseColor &&
                values[1] is bool isHighlighted && values[2] is bool isLastMove)
            {
                if (isLastMove)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, 215, 0));
                }
                else if (isHighlighted)
                {
                    return new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    return new SolidColorBrush(baseColor);
                }
            }
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}