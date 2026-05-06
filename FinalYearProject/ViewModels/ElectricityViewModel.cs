using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Database;
using FinalYearProject.Helper;
using FinalYearProject.Interfaces;
using FinalYearProject.Messages;
using FinalYearProject.Services;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class ElectricityViewModel : BaseViewModel
    {
        private readonly ICarbonService _carbonService;
        private readonly ISettingsService _settingsService;


        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public ElectricityViewModel(ICarbonService carbonService, ISettingsService settingsService)
        {
            _carbonService = carbonService;
            _settingsService = settingsService;

            WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this, (recipient, message) =>
            {
                DistanceUnit = _settingsService.DistanceUnit;
                WeightUnit = _settingsService.WeightUnit;
            });

            DistanceUnit = _settingsService.DistanceUnit;
            WeightUnit = _settingsService.WeightUnit;
            _settingsService = settingsService;
        }

        public ObservableCollection<Electricity> ElectricityList { get; } = new();

        [ObservableProperty]
        string distanceUnit;

        [ObservableProperty]
        string weightUnit;

        [ObservableProperty]
        float carbonGenerated;

        [ObservableProperty]
        float displayedCarbon;

        [ObservableProperty]
        int electricityUsed;

        ElectricityRequest ReturnElectricity()
        {
            ElectricityRequest electricity = new ElectricityRequest();
            electricity.type = "electricity";
            electricity.electricity_value = ElectricityUsed;
            electricity.country = "gb";

            return electricity;
        }

        async Task GetElectricityAsync()
        {

            ElectricityRequest electricity = ReturnElectricity();

            try
            {
                IsBusy = true;
                IsContentVisible = false;

                List<Electricity> electricityList = new();
                electricityList = await _carbonService.GetElectricity(electricity);

                if (ElectricityList.Count != 0)
                    ElectricityList.Clear();

                foreach (var vehicle in electricityList)
                    ElectricityList.Add(vehicle);
                electricityList.Clear(); //for some reason electricity still had values from previous requests despite leaving scope...
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Electricity information", "OK");
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        [RelayCommand]
        async Task CalculateFootprint()
        {
            await GetElectricityAsync();
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                CarbonGenerated = ElectricityList.FirstOrDefault().data.attributes.carbon_g;


                if (WeightUnit == "g")
                {
                    DisplayedCarbon = ElectricityList.FirstOrDefault().data.attributes.carbon_g;
                }
                else if (WeightUnit == "lb")
                {
                    DisplayedCarbon = ElectricityList.FirstOrDefault().data.attributes.carbon_lb;

                }
                else if (WeightUnit == "kg")
                {
                    DisplayedCarbon = ElectricityList.FirstOrDefault().data.attributes.carbon_kg;

                }
                else if (WeightUnit == "mt")
                {
                    DisplayedCarbon = ElectricityList.FirstOrDefault().data.attributes.carbon_mt;

                }

                foreach (Electricity e in ElectricityList)
                {
                    Debug.WriteLine(e.data.attributes.carbon_g);
                }
                await Shell.Current.GoToAsync($"/{nameof(Screens.CarbonCalculationResults)}", true);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Electricity information", "OK");
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }

        }

    }
}
