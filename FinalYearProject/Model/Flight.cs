using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalYearProject.Model;

namespace FinalYearProject.Model
{
    public class Flight
    {
        public FlightData data { get; set; }
    }

    public class FlightData : Data
    {
        public FlightAttributes attributes { get; set; }
    }

    public class  FlightAttributes : Attributes
    {
        public int passengers { get; set; }
        public List<LegRequest> legs { get; set; }
        public string distance_unit { get; set; }
    }

    public class Leg
    {
        public Airport departure_airport { get; set; }
        public Airport destination_airport { get; set; }
        public string cabin_class { get; set; }
    }

    public class LegRequest
    {
        public string departure_airport { get; set; }
        public string destination_airport { get; set; }
        public string cabin_class { get; set; }
    }

    public class FlightRequest
    {
        public string type { get; set; }
        public int passengers { get; set;}
        public LegRequest[] legs { get; set;}
    }
}
