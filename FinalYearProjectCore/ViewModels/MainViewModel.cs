using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Messages;
using FinalYearProjectCore.Services;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.ViewModels
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

        [ObservableProperty]
        string username;

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
        }

        async Task InitialiseAsync()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                if (!(_authClient.User == null))
                {
                    Profile profile = await _profileService.GetProfileAsync();
                    Username = profile.Username;
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




        [ObservableProperty]
        ObservableCollection<string> items;

        [ObservableProperty]
        string text;

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
