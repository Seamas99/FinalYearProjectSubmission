using Firebase.Auth;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Database;
using FinalYearProjectCore.Model;
using FinalYearProjectCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.ViewModels
{
    public partial class ProgressViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _auth;
        private readonly IProfileService _profileService;
        private readonly IMapBoxService _mapBoxService;
        private readonly ICarbonService _carbonService;
        private CancellationTokenSource _searchCts;
        private LeaderboardHelper leaderboardHelper = new();
        private static readonly DateTime currentDate = new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc);

        public ProgressViewModel(FirebaseAuthClient authClient, IProfileService profileService, ICarbonService carbonService)
        {
            _auth = authClient;
            _profileService = profileService;
            _carbonService = carbonService;
            LoadChart();
        }

        public ObservableCollection<CarbonFootprint> displayedFootprints = new();
        public ObservableCollection<Model.Position> positions = new();
        public ObservableCollection<float> carbonMeasurements = new();
        public ObservableCollection<int> xpMeasurements = new();
        public ObservableCollection<int> levelMeasurements = new();
        public ObservableCollection<int> ranks = new();

        [ObservableProperty]
        public float averageCarbon = 0;

        [ObservableProperty]
        public string carbonHigherLower = "";

        [ObservableProperty]
        public float carbonDifferencePercentage = 0;

        [ObservableProperty]
        public int averageXP = 0;

        [ObservableProperty]
        public string xPHigherLower = "";

        [ObservableProperty]
        public float xPDifferencePercentage = 0;

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

        async void LoadChart()
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
                // 1. Fetch missing leagues
                List<League> missingLeagues = await _profileService.GetMissingLeaderboards(profile);

                // 2. Insert leagues in parallel
                await Task.WhenAll(
                    missingLeagues.Select(l => leaderboardHelper.InsertLeagueToDatabase(l))
                );

                // 3. Refresh profile
                profile = await _profileService.ReturnCurrentProfile();

                profileHelper = new ProfileHelper();

                // 4. Insert profile
                await profileHelper.InsertProfileToDatabase(profile);

                // 5. Insert footprints + positions in parallel
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
                        CarbonHigherLower = "Your carbon was higher by";
                    else if (currentCarbon < previousCarbon)
                        CarbonHigherLower = "Your carbon was lower by";
                    else
                        CarbonHigherLower = "Your carbon was the sameas last month";
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

                    if (currentXp > previousXp)
                        XPHigherLower = "higher";
                    else if (currentXp < previousXp)
                        XPHigherLower = "lower";
                    else
                        XPHigherLower = "same";
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

        

    }
}
