using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalYearProject.Model;
using FinalYearProject.ViewModels;
using FinalYearProject.Services;

namespace FinalYearProject.Converters
{
    public class DistanceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is not double kms)
                return values[0];

            if (values[1] is not string unit)
                unit = "km";


            return unit.ToLower() switch
            {
                "km" => $"{kms:F1} km",
                "mile" => $"{kms * 0.6213712:F1} mi",
                _ => $"{kms:F1} km"
            };
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
