using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Database;
using FinalYearProject.Helper;
using FinalYearProject.Interfaces;
using FinalYearProject.Messages;
using FinalYearProject.Screens.ContentViews.SignUpContentViews;
using FinalYearProject.Services;
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalYearProject.ViewModels
{
    public partial class SignUpViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _authClient;
        private readonly IProfileService _profileService;
        private readonly IMapBoxService _mapBoxService;
        private CancellationTokenSource _searchCts;
        private static readonly DateTime currentDate = DateTime.UtcNow;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public SignUpViewModel(FirebaseAuthClient authClient, IProfileService profileService, IMapBoxService mapBoxService)
        {
            _authClient = authClient;
            _profileService = profileService;
            _mapBoxService = mapBoxService;

            BasicInfoCV = new BasicInfo(this);
            HouseholdCV = new Household(this);
            OrganisationCV = new OrganisationContentView(this);
            PersonalCV = new Personal(this);
            LocationCV = new SignUpLocation(this);
            PreferencesCV = new SignUpPreferences(this);

            SelectedSignUpView = BasicInfoCV;

            try
            {
                IsBusy = true;
                IsContentVisible = false;

                PopulateObservableCollections();
                LoadCountries();
                LoadOrganisations();
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        public ObservableCollection<MapBoxSuggestion> SearchResults { get; } = new();
        public ObservableCollection<Organisation> Organisations { get; set; } = new();
        public ObservableCollection<Team> Teams { get; set; } = new();
        public ObservableCollection<Country> Countries { get; set; } = new();
        public ObservableCollection<Subdivision> Subdivisions { get; set; } = new();
        public ObservableCollection<string> Cities { get; set; } = new();
        public ObservableCollection<SavedVehicle> VehicleList => _profileService.Vehicles;

        public ObservableCollection<string> HomeTypes { get; set; } = new();
        public ObservableCollection<string> HeatingTypes { get; set; } = new();
        public ObservableCollection<string> CommuteTypes { get; set; } = new();
        public ObservableCollection<string> DietaryPreferences { get; set; } = new();
        public ObservableCollection<string> AccountTypes { get; set; } = new();

        [ObservableProperty]
        static ContentView selectedSignUpView;

        [ObservableProperty]
        static SavedVehicle selectedVehicle;

        [ObservableProperty]
        static Organisation selectedOrganisation;
        partial void OnSelectedOrganisationChanged(Organisation organisation)
        {
            organisationID = organisation.Id;
            organisationName = organisation.Name;
            LoadOrganisationTeams(organisation.Id);
        }

        [ObservableProperty]
        static Team selectedTeam;
        partial void OnSelectedTeamChanged(Team team)
        {
            organisationTeamID = team.Id;
            organisationTeam = team.Name;
        }

        public ICommand SearchCommand { get; }

        public BasicInfo BasicInfoCV { get; set; }
        public Household HouseholdCV { get; set; }
        public OrganisationContentView OrganisationCV { get; set; }
        public Personal PersonalCV { get; set; }
        public SignUpLocation LocationCV { get; set; }
        public SignUpPreferences PreferencesCV { get; set; }

        //Basic info
        [ObservableProperty]
        public string email;
        [ObservableProperty]
        public string password;
        [ObservableProperty]
        public string username;
        [ObservableProperty]
        public string userID;
        //Organisation
        [ObservableProperty]
        public bool isOrganisationMember;
        [ObservableProperty]
        public string organisationID;
        [ObservableProperty]
        public string organisationName;
        [ObservableProperty]
        public string organisationTeamID;
        [ObservableProperty]
        public string organisationTeam;
        //Personal info
        [ObservableProperty]
        public DateTime dateOfBirth = DateTime.UtcNow;
        [ObservableProperty]
        public string firstName;
        [ObservableProperty]
        public string lastName;
        [ObservableProperty]
        public string phone;
        //Location
        [ObservableProperty]
        static Country selectedCountry;
        [ObservableProperty]
        public string city;
        partial void OnCityChanged(string City)
        {
            SearchResults.Clear();
        }
        [ObservableProperty]
        public string region;
        partial void OnRegionChanged(string region)
        {
            SearchResults.Clear();
        }
        [ObservableProperty]
        public string area;
        partial void OnAreaChanged(string area)
        {
            SearchResults.Clear();
        }
        [ObservableProperty]
        public string postcode;
        partial void OnPostcodeChanged(string area)
        {
            SearchResults.Clear();
        }
        //Household info
        [ObservableProperty]
        public int householdSize;
        [ObservableProperty]
        public string homeType;
        [ObservableProperty]
        public string heatingType;
        [ObservableProperty]
        public bool extraInsulation;
        [ObservableProperty]
        public bool solarInstalled;
        [ObservableProperty]
        public bool vehicleOwnership;
        //Preferences
        [ObservableProperty]
        public string preferredCommuteType;
        [ObservableProperty]
        public string dietaryPreference;


        void PopulateObservableCollections()
        {
            HomeTypes.Add("Bungalow");
            HomeTypes.Add("Cottage");
            HomeTypes.Add("Flat");
            HomeTypes.Add("Detached");
            HomeTypes.Add("Semi-Detached");
            HomeTypes.Add("Terrace");
            HeatingTypes.Add("Gas");
            HeatingTypes.Add("Electric");
            HeatingTypes.Add("Oil");
            HeatingTypes.Add("Renewable");
            CommuteTypes.Add("Carpool");
            CommuteTypes.Add("Walk");
            CommuteTypes.Add("Bus");
            CommuteTypes.Add("Train");
            CommuteTypes.Add("Cycle");
            CommuteTypes.Add("Drive");
            DietaryPreferences.Add("Vegetarian");
            DietaryPreferences.Add("Vegan");
            DietaryPreferences.Add("Pescatarian");
            DietaryPreferences.Add("White meat only");
            DietaryPreferences.Add("Keto");
            DietaryPreferences.Add("All foods");
            AccountTypes.Add("Personal");
            AccountTypes.Add("Organisation");
        }

        public ICommand PerformCitiesSearch => new Command<string>(async (query) =>
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchCts.Token);

                var results = await _mapBoxService.GetCitySuggestionsAsync(query, SelectedCountry.alpha2);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SearchResults.Clear();
                    foreach (var res in results) SearchResults.Add(res);
                });
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        public ICommand PerformRegionsSearch => new Command<string>(async (query) =>
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchCts.Token);

                var results = await _mapBoxService.GetRegionSuggestionsAsync(query, SelectedCountry.alpha2);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SearchResults.Clear();
                    foreach (var res in results) SearchResults.Add(res);
                });
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        public ICommand PerformDistrictsSearch => new Command<string>(async (query) =>
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchCts.Token);

                var results = await _mapBoxService.GetDistrictSuggestionsAsync(query, SelectedCountry.alpha2);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SearchResults.Clear();
                    foreach (var res in results) SearchResults.Add(res);
                });
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        public ICommand PerformPostcodesSearch => new Command<string>(async (query) =>
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchCts.Token);

                var results = await _mapBoxService.GetPostcodeSuggestionsAsync(query, SelectedCountry.alpha2);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SearchResults.Clear();
                    foreach (var res in results) SearchResults.Add(res);
                });
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;

            int age = today.Year - dateOfBirth.Year;

            //If birthday hasn't occurred yet this year, subtract one.
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        private async void LoadOrganisations()
        {
            List<Organisation> orgs = await _profileService.GetOrganisationsAsync();
            foreach(Organisation org in orgs)
            {
                Organisations.Add(org);
            }
        }

        private async void LoadOrganisationTeams(string organisationID)
        {
            Teams.Clear();
            SelectedTeam = new Team();
            organisationTeam = "";
            organisationTeamID = "";

            List<Team> teams = await _profileService.GetOrganisationTeamsAsync(organisationID);
            foreach (Team team in teams)
            {
                Teams.Add(team);
            }
        }

        private async void LoadCountries()
        {
            // Load local countries
            var countryList = (await CountryHelper.LoadAllCountries()
                .ConfigureAwait(false))
                .OrderBy(c => c.name)
                .ToList();

            if (countryList.Count > 10)
            {
                // Add sorted countries in one UI batch
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var c in countryList)
                        Countries.Add(c);
                });
            }
            else
            {
                // Load from API
                var countriesFromApi = (await _profileService.GetCountriesAsync()
                    .ConfigureAwait(false))
                    .OrderBy(c => c.name)
                    .ToList();

                CountryHelper countryHelper = new();

                foreach (var c in countriesFromApi)
                {
                    if (!Countries.Contains(c))
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Countries.Add(c);
                        });

                        await countryHelper.CreateNewCountry(c);
                    }
                }
            }

            Debug.WriteLine($"Countries loaded: {Countries.Count}");
        }

        private async Task<Profile> ReturnProfile()
        {
            Profile profile = new();
            profile.Username = Username;
            profile.Email = Email;
            profile.JoinDate = currentDate;
            profile.IsOrganisationMember = IsOrganisationMember;

            if (isOrganisationMember)
            {
                profile.OrganisationID = SelectedOrganisation.Id;
                profile.OrganisationName = SelectedOrganisation.Name;
                profile.OrganisationTeamID = SelectedTeam.Id;
                profile.OrganisationTeamName = SelectedTeam.Name;
            }

            profile.DateOfBirth = DateOfBirth.ToUniversalTime();
            profile.Age = CalculateAge(profile.DateOfBirth);
            profile.FirstName = FirstName;
            profile.LastName = LastName;
            profile.PhoneNumber = Phone;

            profile.Country = SelectedCountry.name;
            profile.Region = Region;
            profile.Area = Area;
            profile.City = City;
            profile.Postcode = Postcode;

            profile.HouseholdSize = HouseholdSize;
            profile.HomeType = HomeType;
            profile.HeatingType = HeatingType;
            profile.ExtraInsulation = ExtraInsulation;
            profile.SolarInstalled = SolarInstalled;
            profile.VehicleOwnership = VehicleOwnership;

            profile.PreferredCommuteType = PreferredCommuteType;
            profile.DietaryPreference = DietaryPreference;
            if (VehicleOwnership)
            {
                profile.VehicleList = VehicleList.ToList();
            }

            return profile;
        }

        private async Task<Profile> ReturnTestProfile()
        {
            Profile profile = new();
            profile.Username = "Tester123";
            profile.Email = "test@gmail.com";
            profile.JoinDate = currentDate;
            profile.IsOrganisationMember = false;

            if (isOrganisationMember)
            {
                profile.OrganisationID = SelectedOrganisation.Id;
                profile.OrganisationName = SelectedOrganisation.Name;
                profile.OrganisationTeamID = SelectedTeam.Id;
                profile.OrganisationTeamName = SelectedTeam.Name;
            }

            profile.DateOfBirth = DateTime.UtcNow;
            profile.Age = 18;
            profile.FirstName = "John";
            profile.LastName = "Smith";
            profile.PhoneNumber = "07912345678";

            profile.Country = "United Kingdom";
            profile.Region = "Northern Ireland";
            profile.Area = "Derry City and Strabane District";
            profile.City = "Londonderry";
            profile.Postcode = "BT48 9XX";

            profile.HouseholdSize = 3;
            profile.HomeType = "Semi-Detached";
            profile.HeatingType = "Gas";
            profile.ExtraInsulation = true;
            profile.SolarInstalled = false;
            profile.VehicleOwnership = true;

            profile.PreferredCommuteType = "Walk";
            profile.DietaryPreference = "All foods";
            SavedVehicle savedVehicle = new SavedVehicle();
            savedVehicle.id = new Guid().ToString();
            savedVehicle.name = "Tester's Car";
            savedVehicle.vehicle_make = "Toyota";
            savedVehicle.model_name = "Corrola";
            savedVehicle.carbon_interface_id = "6108d711-be04-4dc4-93f9-43d969fd5273";
            if (VehicleOwnership)
            {
                profile.VehicleList = VehicleList.ToList();
            }

            return profile;
        }

        [RelayCommand]
        private async Task TestSignUp()
        {
            Profile profile = await ReturnTestProfile();
            bool nullProfileFields = HelperFunctions.HasAnyNullProperty(profile, "OrganisationID", "OrganisationName", "OrganisationTeamID", "OrganisationTeamName");

            if (nullProfileFields)
            {
                Debug.WriteLine("Null Profile Field");
                await Shell.Current.DisplayAlert("Please check all values are filled in!",
                    "Null Field Error!", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                IsContentVisible = false;

                var result = await _authClient.CreateUserWithEmailAndPasswordAsync(profile.Email, "Password1", profile.Username);

                profile.Id = result.User.Uid;

                bool saveResult = await _profileService.SaveProfileAsync(profile, currentDate);
                if (!saveResult)
                {
                    await _authClient?.User.DeleteAsync();
                    await Shell.Current.DisplayAlert("Sign up failed!",
                    "Signup Error!", "OK");
                }

            }
            catch (FirebaseAuthException ex)
            {
                var fireError = ex.Reason switch
                {
                    AuthErrorReason.AlreadyLinked => "This email is already linked to an account!",
                    AuthErrorReason.AccountExistsWithDifferentCredential => "Account is already registered with a different provider!",
                    AuthErrorReason.InvalidEmailAddress => "The email address format is invalid!",
                    AuthErrorReason.MissingPassword => "No Password provided!",
                    AuthErrorReason.MissingEmail => "No email provided!",
                    AuthErrorReason.WeakPassword => "Password must be stronger than 6 characters!",
                    AuthErrorReason.Undefined => "Please check your connection and try again!",
                    _ => $"Authentication failed: {ex.Reason}"
                };

                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Signup Error!",
                    $"{fireError}", "OK");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Unexpected Error",
                    "Something went wrong. Please try again.", "OK");
                return;
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;

                WeakReferenceMessenger.Default.Send(new UserSignedInMessage());

                await Shell.Current.GoToAsync($"///{nameof(MainPage)}", true);
            }
        }

        [RelayCommand]
        public async Task DeleteTestAccountAsync()
        {
            string password = "Password1";

            if (string.IsNullOrEmpty(password))
                return;

            try
            {
                IsBusy = true;
                IsContentVisible = false;
                await _authClient.SignInWithEmailAndPasswordAsync(_authClient?.User.Info.Email, password);
            }
            catch (FirebaseAuthException ex)
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to verify login", "OK");
                return;
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }

            await _authClient?.User.DeleteAsync();
            await Shell.Current.GoToAsync($"/{nameof(Screens.SignInView)}", true);
        }

        [RelayCommand]
        private async Task SignUp()
        {
            Profile profile = await ReturnProfile();
            bool nullProfileFields = HelperFunctions.HasAnyNullProperty(profile, "OrganisationID", "OrganisationName", "OrganisationTeamID", "OrganisationTeamName");

            if (nullProfileFields)
            {
                Debug.WriteLine("Null Profile Field");
                await Shell.Current.DisplayAlert("Please check all values are filled in!",
                    "Null Field Error!", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                IsContentVisible = false;
                var result = await _authClient.CreateUserWithEmailAndPasswordAsync(Email, Password, Username);

                profile.Id = result.User.Uid;

                bool saveResult = await _profileService.SaveProfileAsync(profile, currentDate);
                if (!saveResult)
                {
                    await _authClient?.User.DeleteAsync();
                    await Shell.Current.DisplayAlert("Sign up failed!",
                    "Signup Error!", "OK");
                }

            }
            catch (FirebaseAuthException ex)
            {
                var fireError = ex.Reason switch
                {
                    AuthErrorReason.AlreadyLinked => "This email is already linked to an account!",
                    AuthErrorReason.AccountExistsWithDifferentCredential => "Account is already registered with a different provider!",
                    AuthErrorReason.InvalidEmailAddress => "The email address format is invalid!",
                    AuthErrorReason.MissingPassword => "No Password provided!",
                    AuthErrorReason.MissingEmail => "No email provided!",
                    AuthErrorReason.WeakPassword => "Password must be stronger than 6 characters!",
                    AuthErrorReason.Undefined => "Please check your connection and try again!",
                    _ => $"Authentication failed: {ex.Reason}"
                };

                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Signup Error!",
                    $"{fireError}", "OK");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Unexpected Error",
                    "Something went wrong. Please try again.", "OK");
                return;
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;

                WeakReferenceMessenger.Default.Send(new UserSignedInMessage());

                await Shell.Current.GoToAsync($"///{nameof(MainPage)}", true);
            }
        }

        [RelayCommand]
        private async Task AddVehicle()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.CreateSavedVehicle)}", true);
        }

        [RelayCommand]
        private async Task DeleteVehicle()
        {
            _profileService.RemoveVehicle(SelectedVehicle);
            VehicleList.Remove(SelectedVehicle);
        }

        [RelayCommand]
        private async Task NavigateSignIn()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.SignInView)}", true);
        }

        [RelayCommand]
        private async Task NavigateBasic()
        {
            SelectedSignUpView = BasicInfoCV;
        }

        [RelayCommand]
        private async Task NavigateOrganisation()
        {
            SelectedSignUpView = OrganisationCV;
        }

        [RelayCommand]
        private async Task NavigatePersonal()
        {
            SelectedSignUpView = PersonalCV;
        }

        [RelayCommand]
        private async Task NavigateLocation()
        {
            SelectedSignUpView = LocationCV;
        }

        [RelayCommand]
        private async Task NavigateHousehold()
        {
            SelectedSignUpView = HouseholdCV;
        }

        [RelayCommand]
        private async Task NavigatePreferences()
        {
            SelectedSignUpView = PreferencesCV;
        }
    }
}
