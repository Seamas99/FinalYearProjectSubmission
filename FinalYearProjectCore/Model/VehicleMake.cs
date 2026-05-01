using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{
    public class VehicleMake
    {
        public VehicleMakeData data { get; set; }
    }

    public class VehicleMakeData : Data
    {
        public VehicleMakeAttributes attributes { get; set; }
    }

    public class VehicleMakeAttributes
    {
        public string name { get; set; }
        public int number_of_models { get; set; }
    }

    public class FullVehicleMake
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public int number_of_models { get; set; }
    }
}
