using System;
using System.Globalization;
using System.Windows.Data;

namespace CheckersApp
{
    public class SkinKingSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BoardCell cell)
            {
                return cell.GetKingSymbol();
            }
            return "♕";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}