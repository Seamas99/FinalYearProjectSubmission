using FinalYearProject.Database;
using FinalYearProject.Interfaces;
using FinalYearProject.Screens;
using FinalYearProject.Screens.ContentViews;
using FinalYearProject.Screens.ContentViews.CalculationEntry;
using FinalYearProject.Screens.ContentViews.CalculationResults;
using FinalYearProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SQLite.SQLite3;

namespace FinalYearProject.ViewModels
{
    public partial class CreateEstimateViewModel : BaseViewModel
    {
        private static readonly DateTime currentDate = DateTime.UtcNow;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public CreateEstimateViewModel(ICarbonService carbonService, ISettingsService settingsService)
        {
            Title = "Vehicle Calculator";
            _carbonService = carbonService;
            _settingsService = settingsService;
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                VehicleVM = new VehicleViewModel(_carbonService, _settingsService);
                ElectricityVM = new ElectricityViewModel(_carbonService);
                FlightVM = new FlightViewModel(_carbonService);
                ShippingVM = new ShippingViewModel(_carbonService, _settingsService);

                VehicleECV = new VehicleEntryContentView(VehicleVM);
                ElectricityECV = new ElectricityEntryContentView(ElectricityVM);
                ShippingECV = new ShippingEntryContentView(ShippingVM);
                FlightECV = new FlightEntryContentView(FlightVM);
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        private readonly ICarbonService _carbonService;
        private readonly ISettingsService _settingsService;
        public VehicleViewModel VehicleVM { get; }
        public ElectricityViewModel ElectricityVM { get; }
        public FlightViewModel FlightVM { get; }
        public ShippingViewModel ShippingVM { get; }

        public VehicleEntryContentView VehicleECV { get; set; }
        public ElectricityEntryContentView ElectricityECV { get; set; }
        public ShippingEntryContentView ShippingECV { get; set; }
        public FlightEntryContentView FlightECV { get; set; }
        public FlightLegEntryContentView FlightLegECV { get; set; }

        public VehicleCalculationResults VehicleResultsECV { get; set; }
        public ElectricityCalculationResults ElectricityResultsECV { get; set; }
        public ShippingCalculationResults ShippingResultsECV { get; set; }
        public FlightCalculationResults FlightResultsECV { get; set; }

        [ObservableProperty]
        string selectedCategory;
        partial void OnSelectedCategoryChanged(string category)
        {
            if (category == "Vehicle")
            {
                SelectedEntryView = VehicleECV;
                SelectedResultsView = VehicleResultsECV;
            }
            else if (category == "Electricity")
            {
                SelectedEntryView = ElectricityECV;
                SelectedResultsView = ElectricityResultsECV;
            }
            else if (category == "Shipping")
            {
                SelectedEntryView = ShippingECV;
                SelectedResultsView = ShippingResultsECV;
            }
            else if (category == "Flight")
            {
                SelectedEntryView = FlightECV;
                SelectedResultsView = FlightResultsECV;
            }
        }

        [ObservableProperty]
        static ContentView selectedEntryView;

        [ObservableProperty]
        static ContentView selectedResultsView;

        [RelayCommand]
        async Task SaveFootprintResult()
        {
            IsBusy = true;
            IsContentVisible = false;

            CarbonFootprint footprint = new CarbonFootprint();
            float carbonMeasurement = 0f;
            if (SelectedCategory == "Vehicle")
            {
                carbonMeasurement = VehicleVM.CarbonGenerated;
            }
            else if (SelectedCategory == "Electricity")
            {
                carbonMeasurement = ElectricityVM.CarbonGenerated;
            }
            else if (SelectedCategory == "Shipping")
            {
                carbonMeasurement = ShippingVM.CarbonGenerated;
            }
            else if (SelectedCategory == "Flight")
            {
                carbonMeasurement = FlightVM.CarbonGenerated;
            }
            footprint.CarbonMeasurement = carbonMeasurement;
            footprint.MeasureDate = currentDate;
            footprint.Type = SelectedCategory;
            footprint.XP = 0;
            footprint.Id = Guid.NewGuid().ToString();
            Model.Profile profile = await ProfileHelper.LoadProfile();
            UpdateFootprintsDTO dto = new();
            dto.Profile = profile;
            dto.Footprint = footprint;
            UpdateFootprintsResponseDTO response = new();
            response = await _carbonService.AddFootprintAsync(dto, currentDate);

            League league = response.League;
            Model.Profile returnedProfile = response.Profile;

            FootprintHelper footprintHelper = new();
            List<CarbonFootprint> cloudFootprints = returnedProfile.Footprints;
            List<CarbonFootprint> localFootprints = await FootprintHelper.LoadFootprints();

            List<CarbonFootprint> cloudOnly = cloudFootprints
                            .Where(cf => !localFootprints.Any(lf => lf.Id == cf.Id))
                            .ToList();

            var insertTasks = cloudOnly
                              .Select(f => footprintHelper.InsertFootprintToDatabase(f));
            await Task.WhenAll(insertTasks);
            
            Position position = new();
            position = returnedProfile.Positions.Where(p => p.EntryDate.Month == footprint.MeasureDate.Month && p.EntryDate.Year == footprint.MeasureDate.Year).FirstOrDefault();
            LeaderboardHelper leaderboardHelper = new();
            //List<League> leagues = await LeaderboardHelper.LoadLeagues();
            //leagues = leagues.Where(e => e.ProcessedDate.Month == currentDate.Month &&
            //                             e.ProcessedDate.Year == currentDate.Year).ToList();

            bool leagueUpdateResult = await leaderboardHelper.UpdateLeagueInDatabase(league);

            bool positionResult = await leaderboardHelper.UpdatePositionInDatabase(position);

            await Shell.Current.GoToAsync($"/{nameof(Screens.CalculationSelectionScreen)}", true);

            IsBusy = false;
            IsContentVisible = true;
        }

        #region SelectionRelayCommands
        [RelayCommand]
        async Task CancelFootprint()
        {
            await Shell.Current.GoToAsync($"/{nameof(Screens.CalculationSelectionScreen)}", true);
        }

        [RelayCommand]
        async Task SelectVehicle()
        {
            VehicleResultsECV = new VehicleCalculationResults(VehicleVM);
            SelectedCategory = "Vehicle";
            await Shell.Current.GoToAsync($"/{nameof(Screens.CreateEstimatePage)}", true);
        }

        [RelayCommand]
        async Task SelectElectricity()
        {
            ElectricityResultsECV = new ElectricityCalculationResults(ElectricityVM);
            SelectedCategory = "Electricity";
            await Shell.Current.GoToAsync($"/{nameof(Screens.CreateEstimatePage)}", true);
        }

        [RelayCommand]
        async Task SelectFlight()
        {
            FlightResultsECV = new FlightCalculationResults(FlightVM);
            //FlightLegECV = new FlightLegEntryContentView(FlightVM);
            SelectedCategory = "Flight";
            await Shell.Current.GoToAsync($"/{nameof(Screens.CreateEstimatePage)}", true);
        }

        [RelayCommand]
        async Task SelectShipping()
        {
            ShippingResultsECV = new ShippingCalculationResults(ShippingVM);
            SelectedCategory = "Shipping";
            await Shell.Current.GoToAsync($"/{nameof(Screens.CreateEstimatePage)}", true);
        }
        #endregion
    }
}
