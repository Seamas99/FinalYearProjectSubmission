using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinalYearProject.Model
{
    public class Position
    {
        public string LeagueID { get; set; } = "";

        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        public int Rank { get; set; } = 1;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "LeagueID", this.LeagueID },
                { "EntryDate", this.EntryDate },
                { "Rank", this.Rank }
            };
        }

    }

    public class LeagueEntry
    {
        public string UserID { get; set; } = "";

        public string LeagueID { get; set; } = "";

        public string Username { get; set; } = "";

        public string Region { get; set; } = "";

        public bool IsOrganisationMember { get; set; } = false;

        public string? OrganisationID { get; set; }

        public string? OrganisationName { get; set; }

        public string? TeamID { get; set; }

        public string? TeamName { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        public int Rank { get; set; } = 1;

        public float CumulativeCarbon { get; set; } = 0.0f;

        public float MonthCarbon { get; set; } = 0.0f;

        public int CumulativeXP { get; set; } = 0;

        public int MonthXP { get; set; } = 0;

        public int Level { get; set; } = 1;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "UserID", this.UserID },
                { "Username", this.Username },
                { "Region", this.Region },
                { "IsOrganisationMember", this.IsOrganisationMember },
                { "OrganisationID", this.OrganisationID },
                { "OrganisationName", this.OrganisationName },
                { "TeamID", this.TeamID },
                { "TeamName", this.TeamName },
                { "EntryDate", this.EntryDate },
                { "Rank", this.Rank },
                { "CumulativeCarbon", this.CumulativeCarbon },
                { "MonthCarbon", this.MonthCarbon },
                { "CumulativeXP" , this.CumulativeXP },
                { "MonthXP", this.MonthXP },
                { "Level", this.Level }
            };
        }

    }

    public class League
    {
        public string LeagueID { get; set; } = "";

        public string LeagueName { get; set; } = "";

        public string Type { get; set; } = ""; //Region or Organisation

        public string Region { get; set; } = ""; //Regional League only

        public int LeagueNumber { get; set; } = 1;

        public string? OrganisationID { get; set; }

        public string? OrganisationName { get; set; }

        public string? TeamID { get; set; }

        public string? TeamName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;

        public float CumulativeCarbon { get; set; } = 0.0f;

        public float MonthCarbon { get; set; } = 0.0f;

        public int CumulativeXP { get; set; } = 0;

        public int MonthXP { get; set; } = 0;

        [SQLite.Ignore]
        public List<LeagueEntry> LeagueEntries { get; set; } = new();

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "LeagueID", this.LeagueID },
                { "LeagueName", this.LeagueName },
                { "Type", this.Type },
                { "Region", this.Region },
                { "LeagueNumber", this.LeagueNumber },
                { "OrganisationID", this.OrganisationID },
                { "OrganisationName", this.OrganisationName },
                { "TeamID", this.TeamID },
                { "TeamName", this.TeamName },
                { "CreatedAt", this.CreatedAt },
                { "ProcessedDate", this.ProcessedDate },
                { "CumulativeCarbon", this.CumulativeCarbon },
                { "MonthCarbon", this.MonthCarbon },
                { "CumulativeXP", this.CumulativeXP },
                { "MonthXP", this.MonthXP },
                { "LeagueEntries", this.LeagueEntries }
            };
        }

    }
}
