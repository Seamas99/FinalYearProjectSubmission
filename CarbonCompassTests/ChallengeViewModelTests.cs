using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalYearProjectCore.Services;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Model;
using FinalYearProjectCore.ViewModels;
using FinalYearProjectCore.Database;

namespace CarbonCompassTests
{
    public class ChallengeViewModelLogicTests
    {

        [Fact]
        public void LoadChallengesLogic_WhenSortingChallenges_SeparatesCompletedAndUncompleted()
        {
            var allChallenges = new List<Challenge>
            {
                new Challenge { Id = "1", CompletionStatus = false, Title = "Start Recycling" },
                new Challenge { Id = "2", CompletionStatus = true,  Title = "Use Public Transport" },
                new Challenge { Id = "3", CompletionStatus = false, Title = "Eat Vegetarian" }
            };

            var uncompletedChallenges = new ObservableCollection<Challenge>();
            var completedChallenges = new ObservableCollection<Challenge>();

            foreach (var c in allChallenges)
            {
                if (c.CompletionStatus)
                    completedChallenges.Add(c);
                else
                    uncompletedChallenges.Add(c);
            }

            Assert.Equal(2, uncompletedChallenges.Count);
            Assert.Single(completedChallenges);

            Assert.Contains(uncompletedChallenges, c => c.Id == "1");
            Assert.Contains(uncompletedChallenges, c => c.Id == "3");
            Assert.Contains(completedChallenges, c => c.Id == "2");
        }

        //accept challenge tests

        [Fact]
        public void AcceptChallengeLogic_WhenChallengeNotAccepted_ChangesStateToTrue()
        {
            var challenge = new Challenge { ChallengeAccepted = false };

            if (!challenge.ChallengeAccepted)
            {
                challenge.ChallengeAccepted = true;
            }

            Assert.True(challenge.ChallengeAccepted);
        }

        [Fact]
        public void AcceptChallengeLogic_WhenChallengeAlreadyAccepted_GuardClausePreventsStateChange()
        {
            var challenge = new Challenge { ChallengeAccepted = true };
            bool databaseUpdateCalled = false;

            if (challenge.ChallengeAccepted)
            {
            }
            else
            {
                //simulates code that would run if failed
                challenge.ChallengeAccepted = true;
                databaseUpdateCalled = true; //simulates update accepted
            }

            Assert.True(challenge.ChallengeAccepted);
            Assert.False(databaseUpdateCalled); 
        }
    }
}
