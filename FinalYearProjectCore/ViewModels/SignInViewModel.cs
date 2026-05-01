using CommunityToolkit.Mvvm.Messaging;
using FinalYearProjectCore.Database;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Messages;
using FinalYearProjectCore.Services;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.ViewModels
{
    public partial class SignInViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _authClient;

        [ObservableProperty]
        public string email;
        [ObservableProperty]
        public string password;
        public string Username => _authClient?.User?.Info?.DisplayName;

        private readonly IProfileService _profileService;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public SignInViewModel(FirebaseAuthClient authClient, IProfileService profileService)
        {
            _authClient = authClient;
            _profileService = profileService;
        }

        [RelayCommand]
        private async Task SignIn()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                await _authClient.SignInWithEmailAndPasswordAsync(Email, Password);
                OnPropertyChanged(nameof(Username));
            }
            catch (FirebaseAuthException ex)
            {
                var fireError = ex.Reason switch
                {
                    AuthErrorReason.WrongPassword => "The password you entered is incorrect!",
                    AuthErrorReason.UnknownEmailAddress => "No account exists with that email address!",
                    AuthErrorReason.AccountExistsWithDifferentCredential => "Account is registered under a different provider!",
                    AuthErrorReason.InvalidEmailAddress => "The email address format is invalid!",
                    AuthErrorReason.MissingPassword => "No Password provided!",
                    AuthErrorReason.MissingEmail => "No email provided!",
                    AuthErrorReason.UserNotFound => "User account was not found!",
                    AuthErrorReason.Undefined => "Please check your connection and try again!",
                    AuthErrorReason.UserDisabled => "This account has been disabled!",
                    AuthErrorReason.Unknown => "Login Failed. If you do not have an account please create one!",
                    AuthErrorReason.TooManyAttemptsTryLater => "Too many failed attempts. Please try again later.",
                    _ => $"Authentication failed: {ex.Reason}"
                };

                Debug.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            if (_authClient?.User != null)
            {
                Model.Profile profile = await _profileService.ReturnCurrentProfile();
                ProfileHelper profileHelper = new();
                LeaderboardHelper leaderboardHelper = new();
                profileHelper.InsertProfileToDatabase(profile);
                foreach (Position position in profile.Positions)
                {
                    bool result = await leaderboardHelper.InsertPositionToDatabase(position);
                }
                foreach (CarbonFootprint footprint in profile.Footprints)
                {
                    await leaderboardHelper.InsertFootprintToDatabase(footprint);
                }
                foreach (Position position in profile.Positions)
                {
                    League league = await _profileService.GetLeaderboard(profile, position.EntryDate);
                    await leaderboardHelper.InsertLeagueToDatabase(league);
                }

                WeakReferenceMessenger.Default.Send(new UserSignedInMessage());


                IsBusy = false;
                IsContentVisible = true;
            }
            else
            {
                IsBusy = false;
                IsContentVisible = true;

                return;
            }
        }

    }
}
