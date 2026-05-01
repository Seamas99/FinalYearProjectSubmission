using Firebase.Auth;
using FinalYearProject.Interfaces;
using FinalYearProject.Helper;
using FinalYearProject.Database;
using FinalYearProject.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class ChallengeViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _auth;
        private readonly IProfileService _profileService;
        private readonly IMapBoxService _mapBoxService;
        private readonly ICarbonService _carbonService;
        private CancellationTokenSource _searchCts;
        private LeaderboardHelper leaderboardHelper = new();
        private static readonly DateTime currentDate = DateTime.UtcNow;
        private readonly ChallengeHelper _challengeHelper = new();

        public ObservableCollection<Challenge> UncompletedChallenges { get; } = new();
        public ObservableCollection<Challenge> CompletedChallenges { get; } = new();

        [ObservableProperty]
        public string reward = "";

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public ChallengeViewModel(FirebaseAuthClient authClient, IProfileService profileService, ICarbonService carbonService)
        {
            _auth = authClient;
            _profileService = profileService;
            _carbonService = carbonService;
            LoadChallenges();
        }

        private async void LoadChallenges()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                ChallengeHelper challengeHelper = new ChallengeHelper();
                List<Challenge> allChallenges = await challengeHelper.GetAllChallenges();

                if (allChallenges.Count == 0 || allChallenges == null)
                {
                    allChallenges = await _profileService.GetChallenges();
                    foreach (Challenge challenge in allChallenges)
                    {
                        bool result = await challengeHelper.InsertChallengeToDatabase(challenge);
                    }
                }

                UncompletedChallenges.Clear();
                CompletedChallenges.Clear();

                foreach (var c in allChallenges)
                {
                    if (c.CompletionStatus)
                        CompletedChallenges.Add(c);
                    else
                        UncompletedChallenges.Add(c);
                }
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        [RelayCommand]
        private async Task AcceptChallenge(Challenge challenge)
        {
            if (challenge.ChallengeAccepted)
                return; // cannot unaccept

            challenge.ChallengeAccepted = true;

            await _challengeHelper.UpdateChallenge(challenge);

            //Refresh UI
            LoadChallenges();
        }
    }
}
