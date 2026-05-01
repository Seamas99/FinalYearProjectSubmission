using FinalYearProjectCore.Database;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SQLite.SQLite3;

namespace FinalYearProjectCore.ViewModels
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

        [RelayCommand]
        async Task SaveFootprintResult()
        {
            CarbonFootprint footprint = new CarbonFootprint();
            float carbonMeasurement = 0f;
            
            footprint.CarbonMeasurement = carbonMeasurement;
            footprint.MeasureDate = currentDate;
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

        }

    }
}
