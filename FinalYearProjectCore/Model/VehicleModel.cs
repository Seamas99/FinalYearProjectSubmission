using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{
    public class VehicleModel
    {
        public VehicleModelData data { get; set; }
    }

    public class VehicleModelData : Data
    {
        public VehicleModelAttributes attributes { get; set; }
    }

    public class VehicleModelAttributes
    {
        public string name { get; set; }
        public int year { get; set; }
        public string vehicle_make { get; set; }
    }
    public class FullVehicleModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public int year { get; set; }
        public string vehicle_make { get; set; }
    }

    public class SavedVehicle
    {
        public string id { get; set; } = "";

        public string carbon_interface_id { get; set; } = "";

        public string name { get; set; } = "";

        public string model_name { get; set; } = "";

        public int year { get; set; } = 2000;

        public string vehicle_make { get; set; } = "";
    }
}
