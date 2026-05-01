using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Helper
{
    public static class HelperFunctions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }

        public static bool HasAnyNullProperty<T>(T obj, params string[] ignoreProperties)
        {
            if (obj == null)
                return true;

            var properties = typeof(T).GetProperties(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance
            );

            foreach (var prop in properties)
            {
                // Skip indexers
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                // Skip ignored properties
                if (ignoreProperties.Contains(prop.Name))
                    continue;

                var value = prop.GetValue(obj);

                if (value == null)
                    return true;
            }

            return false;
        }

    }
}
