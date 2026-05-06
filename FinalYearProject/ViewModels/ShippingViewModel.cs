using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Database;
using FinalYearProject.Helper;
using FinalYearProject.Interfaces;
using FinalYearProject.Messages;
using FinalYearProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class ShippingViewModel : BaseViewModel
    {
        private readonly ICarbonService _carbonService;
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public ShippingViewModel(ICarbonService carbonService, ISettingsService settingsService)
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
        }

        public ObservableCollection<Shipping> ShippingList { get; } = new();
        public ObservableCollection<string> TransportMethods { get; } = ["Ship", "Train", "Truck", "Plane"];

        [ObservableProperty]
        string distanceUnit;
        [ObservableProperty]
        string weightUnit;

        [ObservableProperty]
        float carbonGenerated;

        [ObservableProperty]
        float displayedCarbon;

        [ObservableProperty]
        int distance;

        [ObservableProperty]
        int weightValue;

        [ObservableProperty]
        string transportMethod;

        ShippingRequest ReturnShipping()
        {
            ShippingRequest shipping = new ShippingRequest();
            shipping.type = "shipping";

            shipping.weight_unit = WeightUnit.ToLower();
            shipping.weight_value = WeightValue;

            string distanceUnitMsg = "";
            if (DistanceUnit == "km")
            {
                distanceUnitMsg = "km";
            }
            else
            {
                distanceUnitMsg = "mi";
            }
            shipping.distance_unit = distanceUnitMsg;
            shipping.distance_value = Distance;
            shipping.transport_method = TransportMethod.ToLower();

            return shipping;
        }

        async Task GetShippingAsync()
        {
            ShippingRequest shipping = ReturnShipping();

            try
            {
                IsBusy = true;
                IsContentVisible = false;

                List<Shipping> shippingList = new();
                shippingList = await _carbonService.GetShipping(shipping);

                if (ShippingList.Count != 0)
                    ShippingList.Clear();

                foreach (var vehicle in shippingList)
                    ShippingList.Add(vehicle);
                shippingList.Clear(); //for some reason electricity still had values from previous requests despite leaving scope...
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Shipping information", "OK");
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
            await GetShippingAsync();
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                CarbonGenerated = ShippingList.FirstOrDefault().data.attributes.carbon_g;

                if (WeightUnit == "g")
                {
                    DisplayedCarbon = ShippingList.FirstOrDefault().data.attributes.carbon_g;
                }
                else if (WeightUnit == "lb")
                {
                    DisplayedCarbon = ShippingList.FirstOrDefault().data.attributes.carbon_lb;

                }
                else if (WeightUnit == "kg")
                {
                    DisplayedCarbon = ShippingList.FirstOrDefault().data.attributes.carbon_kg;

                }
                else if (WeightUnit == "mt")
                {
                    DisplayedCarbon = ShippingList.FirstOrDefault().data.attributes.carbon_mt;

                }


                foreach (Shipping s in ShippingList)
                {
                    Debug.WriteLine(s.data.attributes.carbon_g);
                }
                await Shell.Current.GoToAsync($"/{nameof(Screens.CarbonCalculationResults)}", true);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Shipping information", "OK");
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }

        }
    }
}
