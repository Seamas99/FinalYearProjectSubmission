using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Converters
{
    public class WeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double gram)
                return value;

            var unit = parameter as string ?? "grams";

            return unit.ToLower() switch
            {
                "grams" => $"{gram:F1} km",
                "lbs" => $"{gram * 0.002204623:F1} lbs",
                "kilogram" => $"{gram * 0.001:F1} kg",
                "metric tonne" => $"{gram * 0.000001:F1} mt"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
