// =============================================
// ELITE LAN CENTER - BOOL TO RFID CONVERTER
// =============================================

using System.Globalization;
using System.Windows.Data;

namespace EliteLanCenter.Helpers
{
    public class BoolToRFIDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool tieneRfid)
            {
                return tieneRfid ? "✅ RFID" : "❌ Sin RFID";
            }
            return "❌ Sin RFID";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
