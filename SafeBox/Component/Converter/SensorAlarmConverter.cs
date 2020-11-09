using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Utilities;

namespace Component.Converter
{
    [ValueConversion(typeof(int),typeof(String))]
    public class SensorAlarmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int rst = int.Parse(value.ToString());
            switch (rst)
            {
                case UDG_.ALARM_STATUS_CLOSE:
                    return "关";
                case UDG_.ALARM_STATUS_OPEN:
                    return "开";
                case UDG_.ALARM_STATUS_ALARM:
                    return "异常";
                default:
                    return "未知";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
