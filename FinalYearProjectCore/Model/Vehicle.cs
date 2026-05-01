using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{

    public class Vehicle
    {
        public VehicleData data { get; set; }
    }

    public class VehicleData : Data
    {
        public VehicleAttributes attributes { get; set; }
    }

    public class VehicleRequest
    {
        public string type { get; set; }
        public string distance_unit { get; set; }
        public int distance_value { get; set; }
        public string vehicle_model_id { get; set; }
    }

    public class VehicleAttributes : Attributes
    {
        public float distance_value { get; set; }
        public string vehicle_make { get; set; }
        public string vehicle_model { get; set; }
        public int vehicle_year { get; set; }
        public string vehicle_model_id { get; set; }
        public string distance_unit { get; set; }
    }

}
