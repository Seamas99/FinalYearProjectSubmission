using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Database;
using FinalYearProject.Helper;
using FinalYearProject.Interfaces;
using FinalYearProject.Messages;
using FinalYearProject.Model;
using FinalYearProject.Services;
using Firebase.Auth;
using Microcharts;
using Microcharts.Maui;
using SkiaSharp;
using Syncfusion.Maui.Toolkit;
using Syncfusion.Maui.Toolkit.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class ProgressViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _auth;
        private readonly IProfileService _profileService;
        private readonly IMapBoxService _mapBoxService;
        private readonly ICarbonService _carbonService;
        private CancellationTokenSource _searchCts;
        private LeaderboardHelper leaderboardHelper = new();
        private static readonly DateTime currentDate = DateTime.UtcNow;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public ProgressViewModel(FirebaseAuthClient authClient, IProfileService profileService, ICarbonService carbonService)
        {
            _auth = authClient;
            _profileService = profileService;
            _carbonService = carbonService;

            try
            {
                IsBusy = true;
                IsContentVisible = false;

                _ = InitialiseAsync();
            }
            catch (Exception)
            {
                IsBusy = false;
                IsContentVisible = true;
            }

            WeakReferenceMessenger.Default.Register<ChartsUpdatedMessage>(this, (recipient, message) =>
            {
                _ = InitialiseAsync();
            });
        }

        private async Task InitialiseAsync()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                await LoadChart();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chart data: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        public ObservableCollection<CarbonFootprint> displayedFootprints = new();
        public ObservableCollection<Model.Position> positions = new();
        public ObservableCollection<float> carbonMeasurements = new();
        public ObservableCollection<int> xpMeasurements = new();
        public ObservableCollection<int> levelMeasurements = new();
        public ObservableCollection<int> ranks = new();

        [ObservableProperty]
        PieChart footprintPieChart;

        [ObservableProperty]
        SeriesChart footprintSeriesChart;

        [ObservableProperty]
        PointChart footprintPointChart;

        [ObservableProperty]
        SimpleChart footprintSimpleChart;

        [ObservableProperty]
        LineChart monthlyCarbonLineChart;

        [ObservableProperty]
        LineChart monthlyXPLineChart;

        [ObservableProperty]
        LineChart monthlyLevelLineChart;

        [ObservableProperty]
        LineChart monthlyRankLineChart;

        [ObservableProperty]
        LineChart monthlyLeagueNumberLineChart;

        [ObservableProperty]
        DonutChart monthlyCarbonSourceDonutChart;

        [ObservableProperty]
        public float averageCarbon = 0;

        [ObservableProperty]
        public string carbonHigherLower = "Your carbon was the same as last month ";

        [ObservableProperty]
        public float carbonDifferencePercentage = 0;

        [ObservableProperty]
        public bool carbonDifferenceVisible = false;

        [ObservableProperty]
        public int averageXP = 0;

        [ObservableProperty]
        public string xPHigherLower = "Your xp was the same as last month ";

        [ObservableProperty]
        public float xPDifferencePercentage = 0;

        [ObservableProperty]
        public bool xpDifferenceVisible = false;

        [ObservableProperty]
        public int levelGain = 0;

        [ObservableProperty]
        public int averageRank = 0;

        [ObservableProperty]
        public int highestRank = 0;

        [ObservableProperty]
        public int lowestRank = 0;

        [ObservableProperty]
        public int currentLeague = 0;

        [ObservableProperty]
        public int highestLeague = 0;

        [ObservableProperty]
        public int lowestLeague = 0;

        async Task LoadChart()
        {
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

                allPositions = await LeaderboardHelper.LoadPositions();

                allFootprints = await LeaderboardHelper.LoadFootprints();
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
                            Carbon = typeGroup.Sum(x => x.CarbonMeasurement)
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

            // Build or Update the charts
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

            if (MonthlyXPLineChart == null)
            {
                MonthlyXPLineChart = new LineChart
                {
                    Entries = monthlyXpEntries,
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
                MonthlyXPLineChart.Entries = monthlyXpEntries;
            }

            if (MonthlyLevelLineChart == null)
            {
                MonthlyLevelLineChart = new LineChart
                {
                    Entries = monthlyLevelEntries,
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
                MonthlyLevelLineChart.Entries = monthlyLevelEntries;
            }

            if (MonthlyRankLineChart == null)
            {
                MonthlyRankLineChart = new LineChart
                {
                    Entries = monthlyRankEntries,
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
                MonthlyRankLineChart.Entries = monthlyRankEntries;
            }

            if (MonthlyLeagueNumberLineChart == null)
            {
                MonthlyLeagueNumberLineChart = new LineChart
                {
                    Entries = monthlyLeagueNumberEntries,
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
                MonthlyLeagueNumberLineChart.Entries = monthlyLeagueNumberEntries;
            }

            if (MonthlyCarbonSourceDonutChart == null)
            {
                MonthlyCarbonSourceDonutChart = new DonutChart
                {
                    Entries = monthlyPercentageEntries,
                    HoleRadius = 0.6f,
                    LabelTextSize = 32f,
                    BackgroundColor = SKColors.White,
                    IsAnimated = true
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
                    else
                    {
                        CarbonDifferenceVisible = false;
                    }

                    if (currentCarbon > previousCarbon && previousCarbon > 0)
                    {
                        CarbonHigherLower = "Your carbon was higher by ";
                        CarbonDifferenceVisible = true;
                    }
                    else if (currentCarbon < previousCarbon && currentCarbon > 0)
                    {
                        CarbonHigherLower = "Your carbon was lower by ";
                        CarbonDifferenceVisible = true;

                    }
                    else if (currentCarbon == previousCarbon)
                    {
                        CarbonHigherLower = "Your carbon was the same as last month!";
                        CarbonDifferenceVisible = false;
                    }
                    else
                    {
                        CarbonHigherLower = "";
                        CarbonDifferenceVisible = false;
                    }
                }
                else
                {
                    CarbonHigherLower = "No Carbon data for last month!";
                    CarbonDifferenceVisible = false;
                }
            }

            if (monthlyXp.Any())
            {
                AverageXP = (int)monthlyXp.Average(g => g.TotalXP);

                if (monthlyXp.Count >= 2)
                {
                    float currentXp = monthlyXp[^1].TotalXP;
                    float previousXp = monthlyXp[^2].TotalXP;

                    if (previousXp > 0)
                    {
                        XPDifferencePercentage = Math.Abs(((currentXp - previousXp) / previousXp) * 100f);
                    }
                    else
                    {
                        xpDifferenceVisible = false;
                    }

                    if (currentXp > previousXp && previousXp > 0)
                    {
                        XPHigherLower = "Your xp gain was higher by ";
                        XpDifferenceVisible = true;
                    }
                    else if (currentXp < previousXp && currentXp > 0)
                    {
                        XPHigherLower = "Your xp gain was lower by ";
                        XpDifferenceVisible = true;
                    }
                    else if (previousXp == currentXp)
                    {
                        XPHigherLower = "Your xp gain was the same as last month!";
                        XpDifferenceVisible = false;
                    }
                    else
                    {
                        XPHigherLower = "";
                        XpDifferenceVisible = false;
                    }
                }
                else
                {
                    CarbonHigherLower = "No xp data for last month!";
                    XpDifferenceVisible = false;
                }
            }

            if (monthlyLevel.Any())
            {
                if (monthlyLevel.Count >= 2)
                {
                    LevelGain = monthlyLevel[^1].Level - monthlyLevel[^2].Level;
                }
                else
                {
                    LevelGain = 0;
                }
            }

            if (monthlyRank.Any())
            {
                AverageRank = (int)monthlyRank.Average(g => g.Rank);

                HighestRank = monthlyRank.Min(g => g.Rank);
                LowestRank = monthlyRank.Max(g => g.Rank);
            }

            if (monthlyLeagueNumber.Any())
            {
                CurrentLeague = monthlyLeagueNumber[^1].LeagueNumber;

                HighestLeague = monthlyLeagueNumber.Min(g => g.LeagueNumber);
                LowestLeague = monthlyLeagueNumber.Max(g => g.LeagueNumber);
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

    }
}
