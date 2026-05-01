using FinalYearProject.Database;
using FinalYearProject.Interfaces;
using Firebase.Auth;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace FinalYearProject.Services
{
    public class ProfileService : IProfileService
    {
        private readonly FirebaseAuthClient _authClient;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonCaseSensitivityOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private const string BaseUrl = "https://carbon-proxy-404544626195.us-central1.run.app";
        private const string ProxyCountriesUrl = "https://carbon-proxy-404544626195.us-central1.run.app/countries";

        public ProfileService(HttpClient httpClient, FirebaseAuthClient authclient)
        {
            _httpClient = httpClient;
            _authClient = authclient;
        }

        List<Country> countryList = new();
        List<Subdivision> subdivisionList = new();

        public async Task LoadProfile()
        {
            CurrentProfile = new();
            CurrentProfile.Footprints = new();
            CurrentProfile.VehicleList = new();
            CurrentProfile.Positions = new();
            CurrentProfile = await GetProfileAsync();
        }

        public static Profile CurrentProfile { get; set; } = new();

        private ObservableCollection<SavedVehicle> _vehicles = new();
        public ObservableCollection<SavedVehicle> Vehicles
        {
            get => _vehicles;
            private set
            {
                _vehicles = value;
                OnPropertyChanged(nameof(Vehicles));
            }
        }

        public async Task<Profile> ReturnCurrentProfile()
        {
            await LoadProfile();
            return CurrentProfile;
        }

        public void AddVehicle(SavedVehicle vehicle)
        {
            Vehicles.Add(vehicle);
        }

        public void RemoveVehicle(SavedVehicle vehicle)
        {
            Vehicles.Remove(vehicle);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        public async Task<List<Country>> GetCountriesAsync()
        {
            if (countryList?.Count > 10) //Temporary: Find a better way
                return countryList;

            var request = new HttpRequestMessage(HttpMethod.Get, ProxyCountriesUrl);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                countryList = await response.Content.ReadFromJsonAsync<List<Country>>();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Request failed", "OK");
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}: {body}");
            }

            return countryList;
        }

        public async Task<List<Subdivision>> GetSubdivisionsAsync(string alpha2)
        {
            subdivisionList.Clear();
            if (subdivisionList?.Count > 10) //Temporary: Find a better way
                return subdivisionList;

            string subdivisionLink = ProxyCountriesUrl + $"/{alpha2}/subdivisions";

            var request = new HttpRequestMessage(HttpMethod.Get, subdivisionLink);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                subdivisionList = await response.Content.ReadFromJsonAsync<List<Subdivision>>(_jsonCaseSensitivityOption);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Request failed", "OK");
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}: {body}");
            }

            return subdivisionList;
        }

        public async Task<List<string>> GetCitiesAsync(string query)
        {
            List<string> list = new List<string>();

            return list;
        }

        public async Task<Profile?> GetProfileAsync()
        {
            try
            {
                if (_authClient?.User == null)
                    return null;
                string link = $"{BaseUrl}/GetProfile/{_authClient?.User.Uid}";
                var response = await _httpClient.GetAsync(link);
                string bla = await response.Content.ReadAsStringAsync();
                return await response.Content.ReadFromJsonAsync<Profile>(_jsonCaseSensitivityOption);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return null;
            }
        }

        public async Task<List<Challenge>> GetChallenges()
        {
            try
            {
                if (_authClient?.User == null)
                    return null;
                string link = $"{BaseUrl}/ReturnChallenges";
                var response = await _httpClient.GetAsync(link);
                string bla = await response.Content.ReadAsStringAsync();
                return await response.Content.ReadFromJsonAsync<List<Challenge>>(_jsonCaseSensitivityOption);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return null;
            }
        }

        public async Task<List<League>> GetMissingLeaderboards(Profile profile)
        {
            string link = $"{BaseUrl}/GetNewerLeaderboards";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(link, profile);
                var rawBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(rawBody);

                 response.EnsureSuccessStatusCode();

                List<League> leagues = await response.Content.ReadFromJsonAsync<List<League>>(_jsonCaseSensitivityOption);

                return leagues;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return null;
            }
        }

        public async Task<League> GetLeaderboard(Profile profile, DateTime enteredDate)
        {
            string link = $"{BaseUrl}/GetLeaderboard?enteredDate={enteredDate:yyyy-MM-ddTHH:mm:ss}";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(link, profile);
                var rawBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(rawBody);

                response.EnsureSuccessStatusCode();

                League league = await response.Content.ReadFromJsonAsync<League>(_jsonCaseSensitivityOption);

                return league;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return null;
            }
        }

        public async Task<bool> SaveProfileAsync(Profile profile, DateTime enteredDate)
        {
            string link = $"{BaseUrl}/CreateProfile?enteredDate={enteredDate:yyyy-MM-ddTHH:mm:ss}";

            try
            {
                var response = await _httpClient.PostAsJsonAsync(link, profile);
                var rawBody = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Status: {response.StatusCode}");
                Debug.WriteLine($"Response body: {rawBody}");

                if (!response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Error!", $"Request failed: {response.StatusCode}", "OK");
                    return false;
                }

                League league = await response.Content.ReadFromJsonAsync<League>(_jsonCaseSensitivityOption);

                bool result = false;
                if (profile.IsOrganisationMember)
                {
                    if (league.LeagueID == league.TeamID)
                    {
                        result = true;
                    }
                }
                else
                {
                    if (league.Region == profile.Region)
                    {
                        result = true;
                    }
                }

                if (result)
                {
                    ProfileHelper profileHelper = new();
                    LeaderboardHelper leaderboardHelper = new();
                    bool insertProfileResult = await profileHelper.InsertProfileToDatabase(profile);
                    bool insertLeagueResult = await leaderboardHelper.InsertLeagueToDatabase(league);
                    Position position = new();
                    LeagueEntry entry = new();
                    entry = league.LeagueEntries.Where(e => e.UserID == profile.Id).FirstOrDefault();
                    position.LeagueID = entry.LeagueID;
                    position.EntryDate = entry.EntryDate;
                    position.Rank = entry.Rank;
                    bool insertedPos = await leaderboardHelper.InsertPositionToDatabase(position);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return false;
            }
        }

        public async Task<bool> SignOutAsync()
        {
            try
            {
                LeaderboardHelper leaderboardHelper = new();
                ProfileHelper profileHelper = new();

                var tasks = new[]
                {
                    leaderboardHelper.DeleteAllLeagueEntries(),
                    leaderboardHelper.DeleteAllLeagues(),
                    leaderboardHelper.DeleteAllPositions(),
                    profileHelper.DeleteAllProfiles(),
                    profileHelper.DeleteAllSavedVehicles(),
                    leaderboardHelper.DeleteAllFootprints()
                };

                await Task.WhenAll(tasks);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProfileAsync()
        {
            string link = BaseUrl + $"/DeleteProfile/{_authClient?.User.Uid.ToString()}";
            try
            {
                var user = _authClient?.User;
                if (user == null)
                {
                    await Shell.Current.DisplayAlert("Error!",
                        $"Not logged in", "OK");
                    throw new Exception("User is not logged in.");
                }

                var token = await user.GetIdTokenAsync(forceRefresh: false);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var request = new HttpRequestMessage(HttpMethod.Post, link);

                var response = await _httpClient.SendAsync(request);
                var rawBody = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Status: {response.StatusCode}");
                Debug.WriteLine($"Response body: {rawBody}");

                if (!response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Error!", $"Request failed: {response.StatusCode}", "OK");
                    return false;
                }

                LeaderboardHelper leaderboardHelper = new();
                ProfileHelper profileHelper = new();
                await leaderboardHelper.DeleteAllLeagueEntries();
                await leaderboardHelper.DeleteAllLeagues();
                await leaderboardHelper.DeleteAllPositions();
                await profileHelper.DeleteAllProfiles();
                await profileHelper.DeleteAllSavedVehicles();
                await leaderboardHelper.DeleteAllFootprints();
                var result = JsonSerializer.Deserialize<Response>(rawBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });



                return result?.message == "Profile deleted";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return false;
            }
        }

        public async Task<List<Organisation>> GetOrganisationsAsync()
        {
            try
            {
                string link = $"{BaseUrl}/GetOrganisations";
                var response = await _httpClient.GetAsync(link);
                string bla = await response.Content.ReadAsStringAsync();
                return await response.Content.ReadFromJsonAsync<List<Organisation>>(_jsonCaseSensitivityOption);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return null;
            }
        }

        public async Task<List<Team>> GetOrganisationTeamsAsync(string organisationID)
        {
            try
            {
                string link = $"{BaseUrl}/GetOrganisationTeams/{organisationID}";
                var response = await _httpClient.GetAsync(link);
                string bla = await response.Content.ReadAsStringAsync();
                return await response.Content.ReadFromJsonAsync<List<Team>>(_jsonCaseSensitivityOption);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return null;
            }
        }

        public class Response
        {
            public string message { get; set; }
            public string id { get; set; }
        }

    }
}
