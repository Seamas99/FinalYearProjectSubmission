using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Interfaces
{
    public interface IMapBoxService
    {
        Task<List<MapBoxSuggestion>> GetCitySuggestionsAsync(string query, string country);
        Task<List<MapBoxSuggestion>> GetRegionSuggestionsAsync(string query, string country);
        Task<List<MapBoxSuggestion>> GetDistrictSuggestionsAsync(string query, string country);
        Task<List<MapBoxSuggestion>> GetPostcodeSuggestionsAsync(string query, string country);
    }
}
