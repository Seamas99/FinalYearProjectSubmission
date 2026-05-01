using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Model
{
    public class Team
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string OrganisationID { get; set; } = "";
        public string OrganisationName { get; set; } = "";
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public float AverageC02 { get; set; } = 0;
        public float TotalC02 { get; set; } = 0;
        public int XP { get; set; } = 0;
        public int Level { get; set; } = 1;
        public string Email { get; set; } = "";
    }
}
