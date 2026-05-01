using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{
    public class Airport
    {
        public string name { get; set; }
        public string country_name { get; set; }
        public string iso_country { get; set; }
        public string region_name { get; set; }
        public string iso_region { get; set; }
        public string iata_code { get; set; }
        public string type { get; set; }
        public double latitude_deg { get; set; }
        public double longitude_deg { get; set; }
    }
}
