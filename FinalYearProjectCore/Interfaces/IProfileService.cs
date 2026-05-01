using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Interfaces
{
    public interface IProfileService
    {
        Task<Profile> ReturnCurrentProfile();
        Task<Profile> GetProfileAsync();
        Task LoadProfile();
        Task<bool> SignOutAsync();
        Task<bool> DeleteProfileAsync();
        Task<bool> SaveProfileAsync(Profile profile, DateTime enteredDate);
        Task<List<Organisation>> GetOrganisationsAsync();
        Task<List<Team>> GetOrganisationTeamsAsync(string organisationID);
        Task<List<Country>> GetCountriesAsync();
        Task<List<Subdivision>> GetSubdivisionsAsync(string alpha2);
        Task<List<string>> GetCitiesAsync(string query);
        Task<List<Challenge>> GetChallenges();
        Task<League> GetLeaderboard(Profile profile, DateTime enteredDate);
        Task<List<League>> GetMissingLeaderboards(Profile profile);
        void AddVehicle(SavedVehicle vehicle);
        void RemoveVehicle(SavedVehicle vehicle);
        ObservableCollection<SavedVehicle> Vehicles { get; }
    }
}
