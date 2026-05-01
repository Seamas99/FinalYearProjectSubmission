using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{
    public class Alert
    {
        [SQLite.PrimaryKey]
        public string alertID { get; set; } = Guid.NewGuid().ToString();
        public Severity severityLevel { get; set; } = Severity.Low;
        public string title { get; set; } = "";
        public string description { get; set; } = "";
        public bool dismissed { get; set; } = false;
    }

    public enum Severity
    {
        Low,
        Medium,
        High
    }
}
