using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Model
{
    public partial class Settings : ObservableObject
    {
        [PrimaryKey]
        [AutoIncrement]
        public int SettingsID { get; set; }
        [ObservableProperty]
        public string distanceUnit;
        [ObservableProperty]
        public string weightUnit;
        [ObservableProperty]
        public string theme;
    }
}
