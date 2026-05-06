using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Database;
using FinalYearProject.Interfaces;
using FinalYearProject.Messages;
using FinalYearProject.Services;
using Firebase.Auth;
using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IProfileService _profileService;
        private readonly FirebaseAuthClient _authClient;

        private static readonly DateTime currentDate = DateTime.UtcNow;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public MainViewModel(ISettingsService settingsService, IProfileService profileService, FirebaseAuthClient authClient)
        {
            _settingsService = settingsService;
            _profileService = profileService;
            _authClient = authClient;

            Items = new ObservableCollection<string>();
            _ = InitialiseAsync();

            WeakReferenceMessenger.Default.Register<UserSignedInMessage>(this, (recipient, message) =>
            {
                _ = InitialiseAsync();
            });

            WeakReferenceMessenger.Default.Register<ChartsUpdatedMessage>(this, (recipient, message) =>
            {
                _ = InitialiseAsync();
            });

            WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this, (recipient, message) =>
            {
                _ = InitialiseAsync();
            });
        }

        async Task InitialiseAsync()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                if(!(_authClient.User == null))
                {
                    Profile profile = await _profileService.GetProfileAsync();
                    Username = profile.Username;
                    await LoadChart();
                }
                else
                {
                    Username = "";
                }
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
                profiles = profiles.Where(p => p.Id == _authClient.User.Uid).ToList();
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

                allPositions = await LeaderboardHelper.LoadPositions();

                allFootprints = await LeaderboardHelper.LoadFootprints();
            }

            // Filter to current user
            string userId = _authClient.User.Uid;
            var userEntries = allEntries
                .Where(e => e.UserID == userId)
                .ToList();

            float metricMultiplier = 1;
            if (_settingsService.WeightUnit == "g")
            {
                metricMultiplier = 1;
            }
            else if (_settingsService.WeightUnit == "kg")
            {
                metricMultiplier = 0.001f;
            }
            else if (_settingsService.WeightUnit == "lb")
            {
                metricMultiplier = 0.002204623f;
            }
            else if (_settingsService.WeightUnit == "mt")
            {
                metricMultiplier = 0.000001f;
            }

            // Group by month and sum MonthCarbon
            var monthlyCarbon = userEntries
                .GroupBy(e => new { e.EntryDate.Year, e.EntryDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalCarbon = g.Sum((x => x.MonthCarbon * metricMultiplier))
                })
                .ToList();

            var monthlyXp = userEntries
                .GroupBy(e => new { e.EntryDate.Year, e.EntryDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalXP = g.Sum(x => x.MonthXP)
                })
                .ToList();

            var monthlyRank = allPositions
                .GroupBy(e => new { e.EntryDate.Year, e.EntryDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Rank = g.Sum(x => x.Rank)
                })
                .ToList();

            var monthlyLevel = userEntries
                .GroupBy(e => new { e.EntryDate.Year, e.EntryDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Level = g.Sum(x => x.Level)
                })
                .ToList();

            var monthlyLeagueNumber = leagues
                .GroupBy(e => new { e.ProcessedDate.Year, e.ProcessedDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    LeagueNumber = g.Sum(x => x.LeagueNumber)
                })
                .ToList();

            var monthlyPercentages = allFootprints
                // First group by month
                .GroupBy(e => new { e.MeasureDate.Year, e.MeasureDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(monthGroup => new
                {
                    Month = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1),

                    // Now group inside the month by Type
                    Types = monthGroup
                        .GroupBy(x => x.Type)
                        .Select(typeGroup => new
                        {
                            Type = typeGroup.Key,
                            Carbon = typeGroup.Sum((x => x.CarbonMeasurement * metricMultiplier))
                        }).ToList()

                }).ToList();

            // Convert to chart entries
            var monthlyCarbonEntries = monthlyCarbon.Select(m => new ChartEntry(m.TotalCarbon)
            {
                Label = m.Month.ToString("MMM"),
                ValueLabel = m.TotalCarbon.ToString(),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            var monthlyXpEntries = monthlyXp.Select(m => new ChartEntry(m.TotalXP)
            {
                Label = m.Month.ToString("MMM"),
                ValueLabel = m.TotalXP.ToString(),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            var monthlyRankEntries = monthlyRank.Select(m => new ChartEntry(m.Rank)
            {
                Label = m.Month.ToString("MMM"),
                ValueLabel = m.Rank.ToString(),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            var monthlyLevelEntries = monthlyLevel.Select(m => new ChartEntry(m.Level)
            {
                Label = m.Month.ToString("MMM"),
                ValueLabel = m.Level.ToString(),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            var monthlyLeagueNumberEntries = monthlyLeagueNumber.Select(m => new ChartEntry(m.LeagueNumber)
            {
                Label = m.Month.ToString("MMM"),
                ValueLabel = m.LeagueNumber.ToString(),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            var selectedMonth = currentDate;

            var monthData = monthlyPercentages
                            .FirstOrDefault(m => m.Month.Year == currentDate.Year && m.Month.Month == currentDate.Month);

            var monthlyPercentageEntries = monthData == null || monthData.Types == null || monthData.Types.Count == 0
                ? new List<ChartEntry>
                {
                    new ChartEntry(0) { Label = "Vehicle", ValueLabel = "0.0", Color = GetColorForType("Vehicle") },
                    new ChartEntry(0) { Label = "Electricity", ValueLabel = "0.0", Color = GetColorForType("Electricity") },
                    new ChartEntry(0) { Label = "Shipping", ValueLabel = "0.0", Color = GetColorForType("Shipping") },
                    new ChartEntry(0) { Label = "Flight", ValueLabel = "0.0", Color = GetColorForType("Flight") }
                }
                : monthData.Types.Select(t => new ChartEntry(t.Carbon)
                {
                    Label = t.Type,
                    ValueLabel = t.Carbon.ToString("0.0"),
                    Color = GetColorForType(t.Type)
                }).ToList();

            ChartEntry largestEntry = monthlyPercentageEntries.MaxBy(e => e.Value);

            if (largestEntry != null)
            {
                BiggestCarbonSource = largestEntry.Label;
            }

            if (MonthlyCarbonSourceDonutChart == null)
            {
                // First time loading - create the chart
                MonthlyCarbonSourceDonutChart = new DonutChart
                {
                    Entries = monthlyPercentageEntries,
                    HoleRadius = 0.6f,
                    LabelTextSize = 32f,
                    BackgroundColor = SKColors.White,
                    IsAnimated = false
                };
            }
            else
            {
                MonthlyCarbonSourceDonutChart.Entries = monthlyPercentageEntries;
            }

            if (monthlyCarbon.Any())
            {
                AverageCarbon = (float)monthlyCarbon.Average(g => g.TotalCarbon);

                if (monthlyCarbon.Count >= 2)
                {
                    float currentCarbon = monthlyCarbon[^1].TotalCarbon;
                    float previousCarbon = monthlyCarbon[^2].TotalCarbon;

                    if (previousCarbon > 0)
                    {
                        CarbonDifferencePercentage = Math.Abs(((currentCarbon - previousCarbon) / previousCarbon) * 100f);
                    }

                    if (currentCarbon > previousCarbon)
                    {
                        CarbonHigherLower = "Your carbon was higher by ";
                        CarbonDifferenceVisible = true;
                    }
                    else if (currentCarbon < previousCarbon)
                    {
                        CarbonHigherLower = "Your carbon was lower by ";
                        CarbonDifferenceVisible = true;

                    }
                    else
                    {
                        CarbonHigherLower = "Your carbon was the same as last month!";
                        CarbonDifferenceVisible = false;
                    }
                }
                else
                {
                    CarbonHigherLower = "No Carbon data for last month!";
                    CarbonDifferenceVisible = false;
                }
            }

        }

        private SKColor GetColorForType(string type)
        {
            return type switch
            {
                "Flight" => SKColor.Parse("#b455b6"),
                "Vehicle" => SKColor.Parse("#2c3e50"),
                "Electricity" => SKColor.Parse("#77d065"),
                "Shipping" => SKColor.Parse("#3498db"),
                _ => SKColor.Parse("#9b59b6")
            };
        }

        [RelayCommand]
        async Task GoToProfilePage()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.Profile)}", true);
        }

        [RelayCommand]
        async Task GoToAlertPage()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.AlertPage)}", true);
        }

        [RelayCommand]
        async Task GoToFootprintSelection()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.CalculationSelectionScreen)}", true);
        }

        [RelayCommand]
        async Task GoToVehicleInformationPage()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.VehicleInformationScreen)}", true);
        }

        public ObservableCollection<float> carbonMeasurements = new();

        [ObservableProperty]
        public string biggestCarbonSource = "";

        [ObservableProperty]
        public float averageCarbon = 0;

        [ObservableProperty]
        public string carbonHigherLower = "Your carbon was the same as last month ";

        [ObservableProperty]
        public float carbonDifferencePercentage = 0;

        [ObservableProperty]
        public bool carbonDifferenceVisible = false;

        [ObservableProperty]
        ObservableCollection<string> items;

        [ObservableProperty]
        string text;

        [ObservableProperty]
        DonutChart monthlyCarbonSourceDonutChart;

        [ObservableProperty]
        string username;

        [ObservableProperty]
        SKColors labelColour;

        [ObservableProperty]
        ChartEntry[] chartEntries = new[]
        {
            new ChartEntry(212)
            {
                Label = "Vehicle",
                ValueLabel = "112",
                Color = SKColor.Parse("#2c3e50")
            },
            new ChartEntry(248)
            {
                Label = "Electricity",
                ValueLabel = "648",
                Color = SKColor.Parse("#77d065")
            },
            new ChartEntry(128)
            {
                Label = "Flights",
                ValueLabel = "428",
                Color = SKColor.Parse("#b455b6")
            },
            new ChartEntry(514)
            {
                Label = "Shipping",
                ValueLabel = "214",
                Color = SKColor.Parse("#3498db")
            }
        };


        [RelayCommand]
        void Add()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return;

            Items.Add(Text);
            Text = string.Empty;
        }

        [RelayCommand]
        void Delete(string s)
        {
            if (Items.Contains(s))
            {
                Items.Remove(s);
            }
        }
    }
}
