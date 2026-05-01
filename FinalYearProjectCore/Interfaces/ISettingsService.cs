using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Interfaces
{
    public interface ISettingsService
    {
        string DistanceUnit { get; set; }
        string WeightUnit { get; set; }
        string Theme { get; set; }
        void SaveSettings();
        Task<Settings> GetSettings();
        Task<Settings> LoadSettings();
    }
}
