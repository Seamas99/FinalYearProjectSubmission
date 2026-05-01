using Microcharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace FinalYearProject.Model
{
    public class Profile
    {
        //Basic info
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessDate { get; set; } = DateTime.UtcNow;
        public float TotalC02 { get; set; } = 0;
        public float MonthC02 { get; set; } = 0;
        public int XP { get; set; } = 0;
        public int MonthXP { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int Streak { get; set; } = 0;

        //Organisation
        public bool IsOrganisationMember { get; set; } = false;
        public string? OrganisationID { get; set; } = "";
        public string? OrganisationName { get; set; } = "";
        public string? OrganisationTeamID { get; set; } = "";
        public string? OrganisationTeamName { get; set; } = "";

        //Personal Info
        public DateTime DateOfBirth { get; set; } = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public int Age { get; set; } = 18;
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        //Location
        public string Country { get; set; } = "";
        public string Region { get; set; } = "";
        public string Area { get; set; } = "";
        public string City { get; set; } = "";
        public string Postcode { get; set; } = "";

        //Household Info
        public int HouseholdSize { get; set; } = 1;
        public string HomeType { get; set; } = "Flat";
        public string HeatingType { get; set; } = "Oil";
        public bool ExtraInsulation { get; set; } = false;
        public bool SolarInstalled { get; set; } = false;
        public bool VehicleOwnership { get; set; } = false;

        //Preferences
        public string PreferredCommuteType { get; set; } = "Walk";
        public string DietaryPreference { get; set; } = "All foods";

        [SQLite.Ignore]
        public List<SavedVehicle>? VehicleList { get; set; } = new();
        [SQLite.Ignore]
        public List<CarbonFootprint>? Footprints { get; set; } = new();
        [SQLite.Ignore]
        public List<Position>? Positions { get; set; } = new();
    }

    public class CarbonFootprint
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime MeasureDate { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "";
        public float CarbonMeasurement { get; set; } = 0;
        public int XP { get; set; } = 0;
    }
}
