using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{
    public class MapBoxSuggestion
    {
        public string Name { get; set; }
        public string FullAddress { get; set; }
        public string MapboxId { get; set; }
    }

    public class MapBoxResponse
    {
        public List<MapBoxSuggestion> Suggestions { get; set; }
    }
}
