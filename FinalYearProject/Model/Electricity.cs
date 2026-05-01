using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Model
{
    public class Electricity
    {
        public ElectricityData data { get; set; }
    }

    public class ElectricityData : Data
    {
        public ElectricityAttributes attributes { get; set; }
    }
    public class ElectricityAttributes : Attributes
    {
        public string country { get; set; }
        public string state { get; set; }
        public string electricity_unit { get; set; }
        public float electricity_value { get; set; }

    }

    public class ElectricityRequest
    {
        public string type { get; set; }
        public string electricity_unit { get; set; }
        public float electricity_value { get; set; }
        public string country { get; set; }
        public string state { get; set; }
    }
}
