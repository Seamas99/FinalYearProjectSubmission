using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Database;
using FinalYearProjectCore.Services;
using Firebase.Auth;
using Firebase.Auth.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalYearProjectCore.Model;

namespace FinalYearProjectCore.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _auth;
        private readonly IProfileService _profileService;
        private readonly IMapBoxService _mapBoxService;
        private readonly ICarbonService _carbonService;
        private CancellationTokenSource _searchCts;

        [ObservableProperty]
        private Profile profile;

        public ProfileViewModel(FirebaseAuthClient authClient, IProfileService profileService, IMapBoxService mapBoxService, ICarbonService carbonService)
        {
            _auth = authClient;
            _profileService = profileService;
            _mapBoxService = mapBoxService;
            _carbonService = carbonService;
            PopulateVehicleMakes();
            PopulateVehicleModels();
            LoadProfileAsync();
        }

        public ObservableCollection<SavedVehicle> SavedVehiclesList => _profileService.Vehicles;

        public void AddVehicle(SavedVehicle v)
        {
            _profileService.AddVehicle(v);
        }

        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        public ObservableCollection<VehicleMake> VehicleMakes { get; } = new();
        public ObservableCollection<FullVehicleMake> FullVehicleMakes { get; } = new ObservableCollection<FullVehicleMake>();
        public ObservableCollection<VehicleModel> VehicleModel { get; } = new();
        public ObservableCollection<FullVehicleModel> FullVehicleModelsList { get; } = new();

        //List of Vehicle Models belonging to selected Make
        public ObservableCollection<FullVehicleModel> SelectedMakeListOfVehicleModels { get; } = new();

        public ObservableCollection<MapBoxSuggestion> SearchResults { get; } = new();
        public ObservableCollection<Country> Countries { get; set; } = new();
        public ObservableCollection<Subdivision> Subdivisions { get; set; } = new();
        public ObservableCollection<string> Cities { get; set; } = new();
        public ObservableCollection<SavedVehicle> VehicleList { get; set; } = new();


        [ObservableProperty]
        static SavedVehicle selectedVehicle;

        [ObservableProperty]
        static Country selectedCountry;

        [ObservableProperty]
        static Organisation selectedOrganisation;

        [ObservableProperty]
        static Team selectedTeam;

        [ObservableProperty]
        FullVehicleMake selectedMake;
        partial void OnSelectedMakeChanged(FullVehicleMake vehicleMake)
        {
            LoadVehicleModelsFromMake(vehicleMake.name);
        }

        [ObservableProperty]
        FullVehicleModel selectedModel;

        [ObservableProperty]
        string enteredVehicleName;

        public ICommand SearchCommand { get; }

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
        public int age;
        [ObservableProperty]
        public string firstName;
        [ObservableProperty]
        public string lastName;
        [ObservableProperty]
        public string phone;
        //Location
        [ObservableProperty]
        public string country;
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

        public ICommand PerformCitiesSearch => new AsyncRelayCommand<string>(async (query) =>
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchCts.Token);

                var results = await _mapBoxService.GetCitySuggestionsAsync(query, SelectedCountry.alpha2);

                SearchResults.Clear();
                foreach (var res in results) SearchResults.Add(res);
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        public ICommand PerformRegionsSearch => new AsyncRelayCommand<string>(async (query) =>
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchCts.Token);

                var results = await _mapBoxService.GetRegionSuggestionsAsync(query, SelectedCountry.alpha2);

                    SearchResults.Clear();
                    foreach (var res in results) SearchResults.Add(res);
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        public ICommand PerformDistrictsSearch => new AsyncRelayCommand<string>(async (query) =>
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchCts.Token);

                var results = await _mapBoxService.GetDistrictSuggestionsAsync(query, SelectedCountry.alpha2);

                    SearchResults.Clear();
                    foreach (var res in results) SearchResults.Add(res);
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        public ICommand PerformPostcodesSearch => new AsyncRelayCommand<string>(async (query) =>
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchCts.Token);

                var results = await _mapBoxService.GetPostcodeSuggestionsAsync(query, SelectedCountry.alpha2);

                    SearchResults.Clear();
                    foreach (var res in results) SearchResults.Add(res);
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        private async void LoadCountries()
        {
            var countryList = (await CountryHelper.LoadAllCountries().ConfigureAwait(false)).ToList();
            if (countryList.Count > 10)
            {
                foreach (Country c in countryList)
                {
                        Countries.Add(c);
                }
            }
            else
            {
                List<Country> countries = await _profileService.GetCountriesAsync().ConfigureAwait(false);
                CountryHelper countryHelper = new();
                foreach (Country c in countries)
                {
                    //again this if should be unnecessary, but this avoids having double entries
                    if (!Countries.Contains(c))
                    {
                            Countries.Add(c);
                        await countryHelper.CreateNewCountry(c);
                    }
                }
            }
            Debug.WriteLine($"Countries loaded: {Countries.Count}");

        }

        [RelayCommand]
        public async Task DeleteAccountAsync()
        {

            if (string.IsNullOrEmpty(password))
                return;

            try
            {
                await _auth.SignInWithEmailAndPasswordAsync(_auth.User.Info.Email, password);
            }
            catch (FirebaseAuthException ex)
            {
                return;
            }

            await _profileService.DeleteProfileAsync();
            await _auth.User.DeleteAsync();
        }

        [RelayCommand]
        public async Task LogoutAsync()
        {
            _auth.SignOut();
            await _profileService.SignOutAsync();
        }


        [RelayCommand]
        public async Task LoadProfileAsync()
        {
            profile = await _profileService.ReturnCurrentProfile();
        }

        //*************
        //Vehicles Functionality
        //*************

        [RelayCommand]
        public async Task SaveVehicle()
        {
            SavedVehicle savedVehicle = new();
            savedVehicle.carbon_interface_id = SelectedModel.id;
            savedVehicle.name = EnteredVehicleName;
            savedVehicle.model_name = SelectedModel.name;
            savedVehicle.year = SelectedModel.year;
            savedVehicle.vehicle_make = SelectedModel.vehicle_make;
            _profileService.AddVehicle(savedVehicle);
        }


        async Task<string> FindVehicleID(string name)
        {
            string ModelID = "";
            foreach (FullVehicleModel v in SelectedMakeListOfVehicleModels)
            {
                if (v.name == name)
                {
                    ModelID = v.id;
                }
            }
            return ModelID;
        }

        async Task CacheVehicleModels(FullVehicleModel fullVehicleModel)
        {
            VehicleModelHelper vehicleModelHelper = new();
            if (!FullVehicleModelsList.Contains(fullVehicleModel))
            {
                    FullVehicleModelsList.Add(fullVehicleModel);

                await vehicleModelHelper.CreateNewVehicleModel(fullVehicleModel);
            }
        }

        async Task LoadVehicleModelsFromMake(string vehicleMake)
        {
            SelectedMakeListOfVehicleModels.Clear();

            var seenNames = new HashSet<string>();

            foreach (FullVehicleModel v in FullVehicleModelsList)
            {
                if (v.vehicle_make == vehicleMake && !seenNames.Contains(v.name))
                {
                    seenNames.Add(v.name);
                        SelectedMakeListOfVehicleModels.Add(v);
                }
            }
            if (SelectedMakeListOfVehicleModels.Count == 0)
            {
                string vehicleMakeID = "";
                foreach (FullVehicleMake v in FullVehicleMakes)
                {
                    if (v.name == vehicleMake)
                    {
                        vehicleMakeID = v.id;
                    }
                }

                List<VehicleModel> vehicleModelsList = await _carbonService.GetVehicleModels(vehicleMakeID).ConfigureAwait(false);


                foreach (VehicleModel v in vehicleModelsList)
                {
                    FullVehicleModel fullVehicleModel = VehicleModelHelper.ReturnFullVehicleModel(v);
                    CacheVehicleModels(fullVehicleModel);

                    if (!seenNames.Contains(fullVehicleModel.name))
                    {
                        seenNames.Add(fullVehicleModel.name);
                            SelectedMakeListOfVehicleModels.Add(fullVehicleModel);
                    }

                }
            }
        }

        //this will only retrieve models from db already previously requested via api call for specific make
        //e.g. if user has selected ford make, but not volkswagen previously ford will be in db but volkswagen will not
        async Task PopulateVehicleModels()
        {
            List<FullVehicleModel> fullVehicleModels = (await VehicleModelHelper.LoadAllFullVehicleModels().ConfigureAwait(false)).ToList();

            foreach (FullVehicleModel v in fullVehicleModels)
            {
                //This if else should be unnecessary but want to avoid any possibility of double entries
                if (!FullVehicleModelsList.Contains(v))
                {
                        FullVehicleModelsList.Add(v);
                }
            }
        }

        async Task PopulateVehicleMakes()
        {
            List<FullVehicleMake> fullVehicleMakes = (await VehicleMakeHelper.LoadAllFullVehicleMakes().ConfigureAwait(false)).ToList();

            if (fullVehicleMakes.Count == 138) //The number of vehicle makes shouldn't change, this will avoid unnecessary API calls every time wasting credits
            {
                //This if else should be unnecessary, fullVehicleMakes should always be either 0 or 138, but want to avoid any possibility of double entries
                if (FullVehicleMakes.Count == 0)
                {
                    foreach (FullVehicleMake v in fullVehicleMakes)
                    {
                            FullVehicleMakes.Add(v);
                    }
                }
                else
                {
                    foreach (FullVehicleMake v in fullVehicleMakes)
                    {
                            FullVehicleMakes.Add(v);
                    }
                }

            }
            else //else the db hasn't got 138 makes and need to retrieve them via api
            {
                List<VehicleMake> vehicleMakes = await _carbonService.GetVehicleMakes().ConfigureAwait(false);

                VehicleMakeHelper vehicleMakeHelper = new();
                foreach (VehicleMake v in vehicleMakes)
                {
                    FullVehicleMake fullVehicleMake = VehicleMakeHelper.ReturnFullVehicleMake(v);

                    //again this if should be unnecessary, but this avoids having double entries
                    if (!FullVehicleMakes.Contains(fullVehicleMake))
                    {
                            FullVehicleMakes.Add(fullVehicleMake);
                        await vehicleMakeHelper.CreateNewVehicleMake(fullVehicleMake);
                    }
                }
            }

            Debug.WriteLine($"Makes loaded: {VehicleMakes.Count}");

        }

        async Task GetVehicleAsync(string modelID, int distanceValue)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = false;

                List<Vehicle> vehicles = new();
                vehicles = await _carbonService.GetVehicles(modelID, "mi", distanceValue);

                if (Vehicles.Count != 0)
                    Vehicles.Clear();

                foreach (var vehicle in vehicles)
                    Vehicles.Add(vehicle);
                vehicles.Clear(); //for some reason vehicles still had values from previous requests despite leaving scope...
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
