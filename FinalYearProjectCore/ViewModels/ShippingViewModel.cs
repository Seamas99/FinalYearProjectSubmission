using CommunityToolkit.Mvvm.Messaging;
using FinalYearProjectCore.Database;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Messages;
using FinalYearProjectCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.ViewModels
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
        int distance;
        [ObservableProperty]
        int weightValue;
        [ObservableProperty]
        string transportMethod;

        ShippingRequest ReturnShipping()
        {
            ShippingRequest shipping = new ShippingRequest();
            shipping.type = "shipping";
            shipping.weight_unit = "kg";
            shipping.weight_value = WeightValue;
            shipping.distance_unit = "km";
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
                foreach (Shipping s in ShippingList)
                {
                    Debug.WriteLine(s.data.attributes.carbon_g);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }

        }
    }
}
