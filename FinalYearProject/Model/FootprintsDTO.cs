using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Model
{
    public class UpdateFootprintsDTO
    {
        public Profile Profile { get; set; } = new();
        public CarbonFootprint Footprint { get; set; } = new();

    }

    public class UpdateFootprintsResponseDTO
    {
        public Profile Profile { get; set; } = new();
        public League League { get; set; } = new();

    }
}
