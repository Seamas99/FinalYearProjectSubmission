using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{
    public class Country
    {
        public int id { get; set; }
        public string alpha2 { get; set; }
        public string alpha3 { get; set; }
        public string name { get; set; }
        public float ghgPerCapita { get; set; }

    }
}
