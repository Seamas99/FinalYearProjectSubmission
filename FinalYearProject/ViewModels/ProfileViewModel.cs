using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Database;
using FinalYearProject.Helper;
using FinalYearProject.Interfaces;
using FinalYearProject.Messages;
using FinalYearProject.Services;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Microcharts;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalYearProject.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _auth;
        private readonly IProfileService _profileService;
        private readonly IMapBoxService _mapBoxService;
        private readonly ICarbonService _carbonService;
        private readonly ISettingsService _settingsService;
        private CancellationTokenSource _searchCts;

        private static readonly DateTime currentDate = DateTime.UtcNow;

        [ObservableProperty]
        string distanceUnit;

        [ObservableProperty]
        string weightUnit;

        [ObservableProperty]
        private Profile currentProfile;

        public ObservableCollection<float> carbonMeasurements = new();

        [ObservableProperty]
        LineChart monthlyCarbonLineChart;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        [ObservableProperty]
        private Color iconColour = Colors.White;

        public ProfileViewModel(FirebaseAuthClient authClient, IProfileService profileService, IMapBoxService mapBoxService, ICarbonService carbonService, ISettingsService settingsService)
        {
            _auth = authClient;
            _profileService = profileService;
            _mapBoxService = mapBoxService;
            _carbonService = carbonService;
            _settingsService = settingsService;
            WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this, (recipient, message) =>
            {
                DistanceUnit = _settingsService.DistanceUnit;
                WeightUnit = _settingsService.WeightUnit;
            });

            WeakReferenceMessenger.Default.Register<ChartsUpdatedMessage>(this, (recipient, message) =>
            {
                _ = ChartsUpdateAsync();
            });

            _ = InitialiseAsync();

            string savedImagePath = Preferences.Default.Get("SavedProfilePic", string.Empty);

            if (!string.IsNullOrEmpty(savedImagePath) && File.Exists(savedImagePath))
            {
                ProfileImageSource = ImageSource.FromFile(savedImagePath);
            }
        }

        async Task ChartsUpdateAsync()
        {
            await LoadProfileAsync();
            await ReturnLevels();
            await LoadChart();
            await CalculateProgress();
        }


        async Task InitialiseAsync()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                DistanceUnit = _settingsService.DistanceUnit;
                WeightUnit = _settingsService.WeightUnit;
                if (_settingsService.Theme == "Dark")
                {
                    IconColour = Colors.White;
                }
                else
                {
                    IconColour = Colors.Black;
                }
                await PopulateVehicleMakes();
                await PopulateVehicleModels();
                await LoadProfileAsync();
                await ReturnLevels();
                await LoadChart();
                await CalculateProgress();
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        async Task LoadChart()
        {
            LeaderboardHelper leaderboardHelper = new LeaderboardHelper();
            // Load all league entries
            List<LeagueEntry> allEntries = await LeaderboardHelper.LoadLeagueEntries();

            List<Model.Position> allPositions = await LeaderboardHelper.LoadPositions();

            List<Model.CarbonFootprint> allFootprints = await LeaderboardHelper.LoadFootprints();

            List<League> leagues = await LeaderboardHelper.LoadLeagues();

            Profile profile = await ProfileHelper.LoadProfile();
            ProfileHelper profileHelper = new();

            if (leagues.Count == 0)
            {
                List<Profile> profiles = await ProfileHelper.LoadAllProfiles();
                profiles = profiles.Where(p => p.Id == _auth.User.Uid).ToList();
                profile = profiles.LastOrDefault();

                if (profile == null)
                {
                    profile = await _profileService.ReturnCurrentProfile();
                    bool profileResult = await profileHelper.InsertProfileToDatabase(profile);

                    var footprintTasks = profile.Footprints
                    .Select(f => leaderboardHelper.UpdateFootprintInDatabase(f));
                    var footprintResults = await Task.WhenAll(footprintTasks);

                    var positionsTask = profile.Positions
                    .Select(p => leaderboardHelper.UpdatePositionInDatabase(p));
                    var positionsResult = await Task.WhenAll(positionsTask);
                }
                // Fetch all leagues in parallel
                if (profile.Positions.Count == 0 || profile.Positions == null)
                {
                    profile = await _profileService.GetProfileAsync();
                    var positionsTask = profile.Positions
                    .Select(p => leaderboardHelper.UpdatePositionInDatabase(p));
                    var positionsResult = await Task.WhenAll(positionsTask);
                }
                var tasks = profile.Positions
                    .Select(p => _profileService.GetLeaderboard(profile, p.EntryDate));
                var results = await Task.WhenAll(tasks);

                var insertTasks = results
                    .Where(l => l != null)
                    .Select(l => leaderboardHelper.UpdateLeagueInDatabase(l));
                await Task.WhenAll(insertTasks);

                leagues = await LeaderboardHelper.LoadLeagues();

                allEntries = await LeaderboardHelper.LoadLeagueEntries();

                allPositions = await LeaderboardHelper.LoadPositions();

                allFootprints = await LeaderboardHelper.LoadFootprints();
            }

            leagues = leagues.OrderBy(l => l.ProcessedDate).ToList();
            League latest = leagues.LastOrDefault();
            if (latest.ProcessedDate.Month < currentDate.Month || latest.ProcessedDate.Year < currentDate.Year)
            {
                profile = await ProfileHelper.LoadProfile();
                profile.Positions = await LeaderboardHelper.LoadPositions();
                
                List<League> missingLeagues = await _profileService.GetMissingLeaderboards(profile);

                
                await Task.WhenAll(
                    missingLeagues.Select(l => leaderboardHelper.InsertLeagueToDatabase(l))
                );

                
                profile = await _profileService.ReturnCurrentProfile();

                profileHelper = new ProfileHelper();

                
                await profileHelper.InsertProfileToDatabase(profile);

                
                var footprintTasks = profile.Footprints
                    .Select(f => leaderboardHelper.InsertFootprintToDatabase(f));

                var positionTasks = profile.Positions
                    .Select(p => leaderboardHelper.InsertPositionToDatabase(p));

                await Task.WhenAll(footprintTasks.Concat(positionTasks));

                leagues = await LeaderboardHelper.LoadLeagues();

                allEntries = await LeaderboardHelper.LoadLeagueEntries();
            }

            // Filter to current user
            string userId = _auth.User.Uid;
            var userEntries = allEntries
                .Where(e => e.UserID == userId)
                .ToList();

            // Group by month and sum MonthCarbon
            var monthlyCarbon = userEntries
                .GroupBy(e => new { e.EntryDate.Year, e.EntryDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalCarbon = g.Sum(x => x.MonthCarbon)
                })
                .ToList();

            // Convert to chart entries
            var monthlyCarbonEntries = monthlyCarbon.Select(m => new ChartEntry(m.TotalCarbon)
            {
                Label = m.Month.ToString("MMM"),
                ValueLabel = m.TotalCarbon.ToString(),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            var selectedMonth = currentDate;

            // Build chart
            if (MonthlyCarbonLineChart == null)
            {
                MonthlyCarbonLineChart = new LineChart
                {
                    Entries = monthlyCarbonEntries,
                    LabelTextSize = 30f,
                    BackgroundColor = SKColors.White,
                    LineMode = LineMode.Straight,
                    LineSize = 6,
                    PointSize = 12,
                    IsAnimated = true
                };
            }
            else
            {
                MonthlyCarbonLineChart.Entries = monthlyCarbonEntries;
            }

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
        static ContentView selectedSignUpView;

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

        private List<LevelLookup>? _cachedLevels;

        [ObservableProperty]
        public string displayName;

        [ObservableProperty]
        public DateTime joinDate;

        [ObservableProperty]
        public int level;

        [ObservableProperty]
        public float monthCarbon;

        [ObservableProperty]
        public int totalXP;

        [ObservableProperty]
        public double progressRatio;

        [ObservableProperty]
        public string nextLevelText;

        public async Task<List<LevelLookup>> ReturnLevels()
        {
            if (_cachedLevels is not null && _cachedLevels.Count >= 250)
                return _cachedLevels;

            ProfileHelper profileHelper = new ProfileHelper();
            _cachedLevels = await ProfileHelper.LoadLevels();

            if(_cachedLevels.Count < 250)
            {
                for(int i = 0; i <250; i++)
                {
                    LevelLookup level = new LevelLookup();
                    level.Level = i;
                    level.XP = i * i * 1000;
                    _cachedLevels.Add(level);
                    await profileHelper.InsertLevelToDatabase(level);
                }
            }

            return _cachedLevels;
        }

        public async Task CalculateProgress()
        {
            var levels = await ReturnLevels();

            var currentLevelConfig = levels.FirstOrDefault(l => l.Level == Level);
            var nextLevelConfig = levels.FirstOrDefault(l => l.Level == Level + 1);

            if (currentLevelConfig != null && nextLevelConfig != null)
            {
                //Caclulate current progress to next level
                float xpGainedThisLevel = MonthCarbon - currentLevelConfig.XP;
                float totalXpNeededForNextLevel = nextLevelConfig.XP - currentLevelConfig.XP;

                //calc ration 0.0 to 1.0
                double rawProgress = xpGainedThisLevel / totalXpNeededForNextLevel;

                //ensure doesn't break/exceed bounds
                ProgressRatio = Math.Clamp(rawProgress, 0, 1);

                // Calculate remaining xp to next level for the label
                int remainingXP = nextLevelConfig.XP - TotalXP;

                NextLevelText = $"{TotalXP}xp/{totalXpNeededForNextLevel}xp to reach Level {nextLevelConfig.Level}";
            }
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

                var results = await _mapBoxService.GetDistrictSuggestionsAsync(query, SelectedCountry.alpha2);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SearchResults.Clear();
                    foreach (var res in results) SearchResults.Add(res);
                });
            }
            catch (OperationCanceledException) { /* Typing continued, ignore this task */ }
        });

        [ObservableProperty]
        private ImageSource profileImageSource = "profilepic.png";

        [RelayCommand]
        private async Task UploadProfilePictureAsync()
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Please select a profile picture"
                });

                if (result != null)
                {
                    string localFilePath = Path.Combine(FileSystem.AppDataDirectory, result.FileName);

                    using Stream sourceStream = await result.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);
                    await sourceStream.CopyToAsync(localFileStream);

                    Preferences.Default.Set("SavedProfilePic", localFilePath);

                    ProfileImageSource = ImageSource.FromFile(localFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image picking failed: {ex.Message}");
            }
        }

        private async void LoadCountries()
        {
            var countryList = (await CountryHelper.LoadAllCountries().ConfigureAwait(false)).ToList();
            if (countryList.Count > 10)
            {
                foreach (Country c in countryList)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Countries.Add(c);
                    });
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

        [RelayCommand]
        public async Task DeleteAccountAsync()
        {
            string password = await Shell.Current.DisplayPromptAsync("Delete Account?", "To delete account please enter your password:", "Delete", "Cancel");

            if (string.IsNullOrEmpty(password))
                return;

            try
            {
                IsBusy = true;
                IsContentVisible = false;
                await _auth.SignInWithEmailAndPasswordAsync(_auth.User.Info.Email, password);
            }
            catch (FirebaseAuthException ex)
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to verify login", "OK");
                return;
            }

            await _profileService.DeleteProfileAsync();
            await _auth.User.DeleteAsync();
            await Shell.Current.GoToAsync($"/{nameof(Screens.SignInView)}", true);
            IsBusy = false;
            IsContentVisible = true;
        }

        [RelayCommand]
        public async Task LogoutAsync()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;
                _auth.SignOut();
                await _profileService.SignOutAsync();
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
                await Shell.Current.GoToAsync($"/{nameof(Screens.SignInView)}", true);
            }
        }

        [RelayCommand]
        async Task GoToSettingsPage()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.SettingsScreen)}", true);
        }

        [RelayCommand]
        public async Task LoadProfileAsync()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;
                CurrentProfile = await _profileService.ReturnCurrentProfile();
                JoinDate = CurrentProfile.JoinDate;
                DisplayName = CurrentProfile.Username;
                Level = CurrentProfile.Level;
                MonthCarbon = CurrentProfile.MonthC02;
                TotalXP = CurrentProfile.XP;
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        //Vehicles Functionality

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
            await Shell.Current.GoToAsync($"/{nameof(Screens.SignUpView)}", true);
        }

        [RelayCommand]
        public async Task NavigateSignUp()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.SignUpView)}", true);
        }

        [RelayCommand]
        private async Task NavigateSignIn()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.SignInView)}", true);
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
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    FullVehicleModelsList.Add(fullVehicleModel);
                });

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
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        SelectedMakeListOfVehicleModels.Add(v);
                    });
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
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            SelectedMakeListOfVehicleModels.Add(fullVehicleModel);
                        });
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
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        FullVehicleModelsList.Add(v);
                    });
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
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            FullVehicleMakes.Add(v);
                        });
                    }
                }
                else
                {
                    foreach (FullVehicleMake v in fullVehicleMakes)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            FullVehicleMakes.Add(v);
                        });
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
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            FullVehicleMakes.Add(fullVehicleMake);
                        });
                        await vehicleMakeHelper.CreateNewVehicleMake(fullVehicleMake);
                    }
                }
            }

            Debug.WriteLine($"Makes loaded: {VehicleMakes.Count}");

        }

        async Task GetVehicleAsync(string modelID, int distanceValue)
        {

            try
            {
                IsBusy = true;
                IsContentVisible = false;

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
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Vehicle information", "OK");
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

    }
}
