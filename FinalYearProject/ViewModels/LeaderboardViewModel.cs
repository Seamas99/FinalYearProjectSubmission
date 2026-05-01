using Firebase.Auth;
using FinalYearProject.Services;
using FinalYearProject.Interfaces;
using FinalYearProject.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalYearProject.Database;

namespace FinalYearProject.ViewModels
{
    public partial class LeaderboardViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _auth;
        private readonly IProfileService _profileService;
        private readonly IMapBoxService _mapBoxService;
        private readonly ICarbonService _carbonService;
        private CancellationTokenSource _searchCts;
        private LeaderboardHelper leaderboardHelper = new();
        private static readonly DateTime currentDate = DateTime.UtcNow;

        public LeaderboardViewModel(FirebaseAuthClient authClient, IProfileService profileService, ICarbonService carbonService)
        {
            _auth = authClient;
            _profileService = profileService;
            _carbonService = carbonService;
        }

        private int _currentIndex = 0;

        [ObservableProperty]
        public ObservableCollection<LeagueEntry> displayedEntries = new();
        public void RefreshDisplayedEntries()
        {
            DisplayedEntries = new ObservableCollection<LeagueEntry>(
                ApplySort(CurrentLeague?.LeagueEntries)
            );
        }

        public List<League> Leagues { get; set; } = new();

        [ObservableProperty]
        private string selectedMonthYear;
        partial void OnSelectedMonthYearChanged(string value)
        {
            var league = Leagues.FirstOrDefault(
                l => l.ProcessedDate.ToString("MMMM yyyy") == value
            );

            if (league != null)
            {
                _currentIndex = Leagues.IndexOf(league);
                CurrentLeague = league;
            }
        }

        public List<string> MonthYearOptions { get; set; }

        [ObservableProperty]
        public League currentLeague;
        partial void OnCurrentLeagueChanged(League value)
        {
            RefreshDisplayedEntries();
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RankHeader), nameof(UserHeader), nameof(XPHeader), nameof(CarbonHeader), nameof(LevelHeader))]
        private string currentSort = "Rank";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RankHeader), nameof(UserHeader), nameof(XPHeader), nameof(CarbonHeader), nameof(LevelHeader))]
        private bool sortDescending = false;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        // Computed header text with sort indicator
        public string RankHeader => "# " + (CurrentSort == "Rank" ? (SortDescending ? "▼" : "▲") : "");
        public string UserHeader => "User " + (CurrentSort == "User" ? (SortDescending ? "▼" : "▲") : "");
        public string XPHeader => "XP " + (CurrentSort == "XP" ? (SortDescending ? "▼" : "▲") : "");
        public string CarbonHeader => "Carbon " + (CurrentSort == "Carbon" ? (SortDescending ? "▼" : "▲") : "");
        public string LevelHeader => "Level " + (CurrentSort == "Level" ? (SortDescending ? "▼" : "▲") : "");

        [RelayCommand]
        private void SortBy(string sortKey)
        {
            if (CurrentSort == sortKey)
                SortDescending = !SortDescending;
            else
            {
                //Default: Rank ascending, XP/Carbon descending (highest first)
                SortDescending = sortKey != "Rank";
                CurrentSort = sortKey;
            }
            RefreshDisplayedEntries();
        }


        private IEnumerable<LeagueEntry> ApplySort(IEnumerable<LeagueEntry> entries)
        {
            if (entries == null) return Enumerable.Empty<LeagueEntry>();

            return (CurrentSort, SortDescending) switch
            {
                ("XP", true) => entries.OrderByDescending(e => e.MonthXP),
                ("XP", false) => entries.OrderBy(e => e.MonthXP),
                ("User", true) => entries.OrderByDescending(e => e.Username),
                ("User", false) => entries.OrderBy(e => e.Username),
                ("Carbon", true) => entries.OrderByDescending(e => e.MonthCarbon),
                ("Carbon", false) => entries.OrderBy(e => e.MonthCarbon),
                ("Level", true) => entries.OrderByDescending(e => e.Level),
                ("Level", false) => entries.OrderBy(e => e.Level),
                (_, true) => entries.OrderByDescending(e => e.Rank),
                _ => entries.OrderBy(e => e.Rank),
            };
        }

        public async Task LoadLeaderboards()
        {
            IsBusy = true;
            try
            {
                Leagues = await LeaderboardHelper.LoadLeagues();
                Profile profile = await ProfileHelper.LoadProfile();
                ProfileHelper profileHelper = new();

                if (Leagues.Count == 0)
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

                    Leagues = await LeaderboardHelper.LoadLeagues();
                }

                Leagues = Leagues.OrderBy(l => l.ProcessedDate).ToList();

                if (Leagues.Count > 0)
                {
                    League latest = Leagues.LastOrDefault();
                    if (latest.ProcessedDate.Month < currentDate.Month || latest.ProcessedDate.Year < currentDate.Year)
                    {
                        profile.Positions = await LeaderboardHelper.LoadPositions();
                        
                        List<League> missingLeagues = await _profileService.GetMissingLeaderboards(profile);

                        
                        await Task.WhenAll(
                            missingLeagues.Select(l => leaderboardHelper.UpdateLeagueInDatabase(l))
                        );

                        
                        profile = await _profileService.ReturnCurrentProfile();

                        profileHelper = new ProfileHelper();

                        
                        await profileHelper.UpdateProfile(profile);

                        
                        var footprintTasks = profile.Footprints
                            .Select(f => leaderboardHelper.UpdateFootprintInDatabase(f));

                        var positionTasks = profile.Positions
                            .Select(p => leaderboardHelper.UpdatePositionInDatabase(p));

                        await Task.WhenAll(footprintTasks.Concat(positionTasks));
                        Leagues.Clear();
                        Leagues = await LeaderboardHelper.LoadLeagues();
                    }
                    MonthYearOptions = Leagues
                        .Select(l => l.ProcessedDate.ToString("MMMM yyyy"))
                        .ToList();

                    _currentIndex = Leagues.Count - 1;
                    CurrentLeague = Leagues[_currentIndex];
                    SelectedMonthYear = CurrentLeague.ProcessedDate.ToString("MMMM yyyy");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error!", "Couldn't load Leagues", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }


        [RelayCommand]
        private void NextLeague()
        {
            if (_currentIndex < Leagues.Count - 1)
            {
                _currentIndex++;
                League league = new();
                league.LeagueEntries = new();
                league = Leagues[_currentIndex];
                league.LeagueEntries = league.LeagueEntries.OrderBy(e => e.Rank).ToList();
                CurrentLeague = league;
            }
        }

        [RelayCommand]
        private void PreviousLeague()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                League league = new();
                league.LeagueEntries = new();
                league = Leagues[_currentIndex];
                league.LeagueEntries = league.LeagueEntries.OrderBy(e => e.Rank).ToList();
                CurrentLeague = league;
            }
        }
    }
}
