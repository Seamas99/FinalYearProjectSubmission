using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalYearProject.Helper;
using FinalYearProject.Database;

namespace FinalYearProject.ViewModels
{
    public partial class AlertViewModel : BaseViewModel
    {
        private readonly AlertHelper _alertHelper = new();

        public ObservableCollection<Alert> DismissedAlerts { get; } = new();
        public ObservableCollection<Alert> ActiveAlerts { get; } = new();

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public AlertViewModel()
        {
            LoadAlerts();
        }

        private async void LoadAlerts()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                AlertHelper alertHelper = new AlertHelper();
                List<Alert> allAlerts = await alertHelper.GetAllAlerts();

                if (allAlerts.Count == 0 || allAlerts == null)
                {
                    Alert alert = new Alert();
                    alert.severityLevel = Severity.High;
                    alert.title = "Maintain your streak!";
                    alert.description = "You are in danger of losing your streak, input an activity to maintain it!";
                    alert.dismissed = false;

                    Alert alert2 = new Alert();
                    alert2.severityLevel = Severity.Medium;
                    alert2.title = "New league reached";
                    alert2.description = "You have entered a new league";
                    alert2.dismissed = false;

                    Alert alert3 = new Alert();
                    alert3.severityLevel = Severity.Low;
                    alert3.title = "New challenges";
                    alert3.description = "There are new challenges available!";
                    alert3.dismissed = false;

                    allAlerts.Add(alert);
                    allAlerts.Add(alert2);
                    allAlerts.Add(alert3);

                    await alertHelper.InsertAlertToDatabase(alert);
                    await alertHelper.InsertAlertToDatabase(alert2);
                    await alertHelper.InsertAlertToDatabase(alert3);
                }

                DismissedAlerts.Clear();
                ActiveAlerts.Clear();

                foreach (var c in allAlerts)
                {
                    if (c.dismissed)
                        DismissedAlerts.Add(c);
                    else
                        ActiveAlerts.Add(c);
                }
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        [RelayCommand]
        private async Task DismissAlert(Alert alert)
        {
            if (alert.dismissed)
                return;

            alert.dismissed = true;

            await _alertHelper.UpdateAlert(alert);

            //Refresh UI
            LoadAlerts();
        }

        [RelayCommand]
        private async Task ReactivateAlert(Alert alert)
        {
            if (!alert.dismissed)
                return;

            alert.dismissed = false;

            await _alertHelper.UpdateAlert(alert);

            //Refresh UI
            LoadAlerts();
        }
    }
}
