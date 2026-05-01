using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Converters
{
    public class SeverityToColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Severity severity)
            {
                return severity switch
                {
                    Severity.High => Colors.Red,
                    Severity.Medium => Colors.Orange,
                    Severity.Low => Colors.DimGray,
                    _ => Colors.DimGray
                };
            }

            return Colors.DimGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
