using FinalYearProjectCore.Database;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Model;
using FinalYearProjectCore.Services;
using FinalYearProjectCore.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonCompassTests
{
    public class LeaderboardViewModelTests
    {
        private readonly Mock<IProfileService> _mockProfileService;
        private readonly Mock<ICarbonService> _mockCarbonService;

        public LeaderboardViewModelTests()
        {
            _mockProfileService = new Mock<IProfileService>();
            _mockCarbonService = new Mock<ICarbonService>();
        }

        private LeaderboardViewModel CreateSut()
        {
            //passing null as FirebaseAuthClient can't be mocked
            //shouldn't crash as long as we dont call loadleaderboard
            return new LeaderboardViewModel(
                null,
                _mockProfileService.Object,
                _mockCarbonService.Object
            );
        }

        [Fact]
        public void Constructor_WhenInstantiated_SetsDefaultSortToRankAscending()
        {
            var sut = CreateSut();

            Assert.Equal("Rank", sut.CurrentSort);
            Assert.False(sut.SortDescending);
            Assert.Contains("▲", sut.RankHeader);
        }

        [Fact]
        public void SortByCommand_WhenSameKeyClicked_TogglesSortDirection()
        {
            var sut = CreateSut();

            sut.SortByCommand.Execute("Rank");

            Assert.Equal("Rank", sut.CurrentSort);
            Assert.True(sut.SortDescending);
            Assert.Contains("▼", sut.RankHeader);
        }

        [Fact]
        public void SortByCommand_WhenNewKeyClicked_SetsNewKeyAndDefaultsToDescendingIfNOTRank()
        {
            var sut = CreateSut();

            sut.SortByCommand.Execute("XP");

            Assert.Equal("XP", sut.CurrentSort);
            Assert.True(sut.SortDescending); //xp defaults to highest first (descending)
            Assert.Contains("▼", sut.XPHeader);
            Assert.DoesNotContain("▼", sut.RankHeader);
        }

        [Fact]
        public void SortByCommand_WhenExecuted_CorrectlySortsDisplayedEntries()
        {
            // Arrange
            var sut = CreateSut();
            sut.CurrentLeague = new League
            {
                LeagueEntries = new List<LeagueEntry>
                {
                    new LeagueEntry { Username = "Alpha", Rank = 3, MonthXP = 100 },
                    new LeagueEntry { Username = "Beta", Rank = 1, MonthXP = 50 },
                    new LeagueEntry { Username = "Charlie", Rank = 2, MonthXP = 500 }
                }
            };

            
            sut.SortByCommand.Execute("Rank");
            sut.SortDescending = false;
            sut.RefreshDisplayedEntries();

            Assert.Equal("Beta", sut.DisplayedEntries[0].Username);
            Assert.Equal("Charlie", sut.DisplayedEntries[1].Username);
            Assert.Equal("Alpha", sut.DisplayedEntries[2].Username);

            sut.SortByCommand.Execute("XP");

            Assert.Equal("Charlie", sut.DisplayedEntries[0].Username);
            Assert.Equal("Alpha", sut.DisplayedEntries[1].Username);
            Assert.Equal("Beta", sut.DisplayedEntries[2].Username);
        }


        [Fact]
        public void OnSelectedMonthYearChanged_WhenMonthExists_SetsCurrentLeagueAndIndex()
        {
            var sut = CreateSut();
            var targetDate = new DateTime(2024, 5, 1);

            sut.Leagues = new List<League>
            {
                new League { ProcessedDate = new DateTime(2024, 4, 1), LeagueName = "April" },
                new League { ProcessedDate = targetDate, LeagueName = "May" },
                new League { ProcessedDate = new DateTime(2024, 6, 1), LeagueName = "June" }
            };

            sut.SelectedMonthYear = targetDate.ToString("MMMM yyyy"); // "May 2024"

            Assert.NotNull(sut.CurrentLeague);
            Assert.Equal("May", sut.CurrentLeague.LeagueName);
        }

        [Fact]
        public void NextAndPreviousLeagueCommands_NavigateCorrectlyThroughList()
        {
            var sut = CreateSut();
            sut.Leagues = new List<League>
            {
                new League { ProcessedDate = new DateTime(2024, 4, 1), LeagueName = "April" },
                new League { ProcessedDate = new DateTime(2024, 5, 1), LeagueName = "May" },
                new League { ProcessedDate = new DateTime(2024, 6, 1), LeagueName = "June" }
            };

            sut.SelectedMonthYear = new DateTime(2024, 5, 1).ToString("MMMM yyyy");

            sut.NextLeagueCommand.Execute(null);

            Assert.Equal("June", sut.CurrentLeague.LeagueName);

            sut.PreviousLeagueCommand.Execute(null);

            Assert.Equal("May", sut.CurrentLeague.LeagueName);
        }
    }
}
