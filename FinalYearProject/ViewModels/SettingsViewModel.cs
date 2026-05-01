using FinalYearProject.Interfaces;
using FinalYearProject.Model;
using FinalYearProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            if (ProposedSettings == null)
            {
                ProposedSettings = new();
                _ = LoadSettings();
            }
        }

        #region Properties
        [ObservableProperty]
        string selectedWeightUnit;
        partial void OnSelectedWeightUnitChanged(string weightUnit)
        {
            ProposedSettings.WeightUnit = weightUnit;
        }

        [ObservableProperty]
        string selectedDistanceUnit;
        partial void OnSelectedDistanceUnitChanged(string distanceUnit)
        {
            ProposedSettings.DistanceUnit = distanceUnit;
        }

        [ObservableProperty]
        string selectedAppTheme;
        partial void OnSelectedAppThemeChanged(string appTheme)
        {
            if (appTheme == "Dark")
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
            else if (appTheme == "Light")
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }
            if (appTheme == "System")
            {
                Application.Current.UserAppTheme = AppTheme.Unspecified;

            }
            ProposedSettings.Theme = appTheme;
        }

        [ObservableProperty]
        Settings proposedSettings;
        partial void OnProposedSettingsChanged(Settings oldValue, Settings newValue)
        {
            if (oldValue != null)
                oldValue.PropertyChanged -= SettingsChanged;

            if (newValue != null)
                newValue.PropertyChanged += SettingsChanged;
        }
        #endregion

        #region DropDown Lists
        public List<string> WeightUnits { get; } = new()
        {
            "grams", "lbs", "kilogram", "metric tonne"
        };

        public List<string> DistanceUnits { get; } = new()
        {
            "kilometre", "mile"
        };

        public List<string> AppThemes { get; } = new()
        {
            "System", "Dark", "Light"
        }; 
        #endregion

        public bool HasUnsavedChanges;

        public double testVal { get; } = 100.0d;

        private async void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            Settings sysSettings = await _settingsService.GetSettings();
            if (!AreSettingsEqual(ProposedSettings, sysSettings))
                HasUnsavedChanges = true;
        }

        private bool AreSettingsEqual(Settings first, Settings second)
        {
            var properties = typeof(Settings)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != nameof(Settings.SettingsID));

            foreach (var prop in properties)
            {
                var firstValue = prop.GetValue(first);
                var secondValue = prop.GetValue(second);

                if (!Equals(firstValue, secondValue))
                    return false;
            }

            return true;
        }

        public override async Task<bool> CanNavigateAwayAsync()
        {
            if (!HasUnsavedChanges)
                return true;

            bool response = await Shell.Current.DisplayAlert(
                "Unsaved Changes",
                "Are you sure you want to leave without saving?",
                "Leave",
                "Stay"
            );
            if (response)
            {
                ProposedSettings = await _settingsService.GetSettings();
                SelectedDistanceUnit = ProposedSettings.DistanceUnit;
                SelectedWeightUnit = ProposedSettings.WeightUnit;
                SelectedAppTheme = ProposedSettings.Theme;
                Application.Current.UserAppTheme = Helper.HelperFunctions.AppThemeConverter(ProposedSettings.Theme);
                HasUnsavedChanges = false;
            }
            return response;
        }

        private async Task LoadSettings()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                ProposedSettings = await _settingsService.LoadSettings();
                SelectedDistanceUnit = ProposedSettings.DistanceUnit;
                SelectedWeightUnit = ProposedSettings.WeightUnit;
                SelectedAppTheme = ProposedSettings.Theme;
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }

        }

        [RelayCommand]
        async Task SaveSettings()
        {
            _settingsService.WeightUnit = ProposedSettings.WeightUnit;
            _settingsService.DistanceUnit = ProposedSettings.DistanceUnit;
            _settingsService.Theme = ProposedSettings.Theme;
            Application.Current.UserAppTheme = Helper.HelperFunctions.AppThemeConverter(ProposedSettings.Theme);

            _settingsService.SaveSettings();
            Debug.WriteLine("Settings Saved");
            HasUnsavedChanges = false;
        }
    }
}
