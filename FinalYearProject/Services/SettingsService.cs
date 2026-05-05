using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FinalYearProject.Model;
using FinalYearProject.Helper;
using FinalYearProject.Services;
using FinalYearProject.Database;
using FinalYearProject.Interfaces;
using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Messages;

namespace FinalYearProject.Services
{
    public partial class SettingsService : ISettingsService
    {
        public string DistanceUnit
        {
            get => Preferences.Default.Get(nameof(DistanceUnit), "miles");
            set => Preferences.Default.Set(nameof(DistanceUnit), value);
        }

        public string WeightUnit
        {
            get => Preferences.Default.Get(nameof(WeightUnit), "g");
            set => Preferences.Default.Set(nameof(WeightUnit), value);
        }

        public string Theme
        {
            get => Preferences.Default.Get(nameof(Theme), "System");
            set => Preferences.Default.Set(nameof(Theme), value);
        }

        public Task CreateSettings(Settings settings)
        {
            SettingsHelper settingsHelper = new SettingsHelper();
            return settingsHelper.CreateSettings(settings);
        }
        public async void SaveSettings()
        {
            Settings settings = new Settings();
            settings = await GetSettings();

            settings.DistanceUnit = DistanceUnit;
            settings.WeightUnit = WeightUnit;
            settings.Theme = Theme;

            SettingsHelper settingsHelper = new SettingsHelper();
            settingsHelper.UpdateSettings(settings);

            WeakReferenceMessenger.Default.Send(new SettingsChangedMessage());
        }

        public async Task<Settings> CreateDefaultSettings()
        {
            Settings settings = new Settings();
            settings.DistanceUnit = "mile";
            settings.WeightUnit = "g";
            settings.Theme = "System";
            CreateSettings(settings);
            return settings;
        }

        public async Task<Settings> GetSettings()
        {
            Settings settings = new Settings();

            settings = (await SettingsHelper.LoadSettings().ConfigureAwait(false)).FirstOrDefault();
            return settings;
        }

        public async Task<Settings> LoadSettings()
        {
            Settings settings = new Settings();

            try
            {
                settings = (await SettingsHelper.LoadSettings().ConfigureAwait(false)).FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.WeightUnit) || string.IsNullOrEmpty(settings.DistanceUnit) || string.IsNullOrEmpty(settings.Theme))
                {
                    settings = await CreateDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                if (settings == null || string.IsNullOrEmpty(settings.WeightUnit) || string.IsNullOrEmpty(settings.DistanceUnit) || string.IsNullOrEmpty(settings.Theme))
                {
                    settings = await CreateDefaultSettings();
                }
            }

            DistanceUnit = settings.DistanceUnit;
            WeightUnit = settings.WeightUnit;
            Theme = settings.Theme;
            Application.Current.UserAppTheme = Helper.HelperFunctions.AppThemeConverter(settings.Theme);


            return settings;
        }
    }
}
