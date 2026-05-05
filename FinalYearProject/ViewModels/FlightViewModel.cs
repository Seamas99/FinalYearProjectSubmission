using CommunityToolkit.Mvvm.Messaging;
using FinalYearProject.Database;
using FinalYearProject.Helper;
using FinalYearProject.Interfaces;
using FinalYearProject.Messages;
using FinalYearProject.Screens.ContentViews.CalculationEntry;
using FinalYearProject.Services;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.ViewModels
{
    public partial class FlightViewModel : BaseViewModel
    {
        private readonly ICarbonService _carbonService;
        private readonly ISettingsService _settingsService;


        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isContentVisible = true;

        public FlightViewModel(ICarbonService carbonService, ISettingsService settingsService)
        {
            _carbonService = carbonService;
            _settingsService = settingsService;
            FlightLeg = new FlightLegEntryContentView(this);
            Nav = new Navigator<Leg>(Legs);

            WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this, (recipient, message) =>
            {
                DistanceUnit = _settingsService.DistanceUnit;
                WeightUnit = _settingsService.WeightUnit;
            });

            DistanceUnit = _settingsService.DistanceUnit;
            WeightUnit = _settingsService.WeightUnit;
            _ = PopulateAiports();
        }

        public ObservableCollection<Leg> Legs { get; } = new();
        public ObservableCollection<Flight> Flights { get; } = new();
        public ObservableCollection<string> CabinClasses { get; } = ["Economy", "Premium"];
        public ObservableCollection<Airport> AirportsList { get; } = new();
        public ObservableCollection<Airport> FilteredAirportsList { get; } = new();

        [ObservableProperty]
        string distanceUnit;

        [ObservableProperty]
        string weightUnit;

        [ObservableProperty]
        float carbonGenerated;

        [ObservableProperty]
        Flight selectedFlight;

        [ObservableProperty]
        int passengers;

        [ObservableProperty]
        int flightLegs;

        [ObservableProperty]
        Airport departureAirport;

        [ObservableProperty]
        Airport arrivalAirport;

        [ObservableProperty]
        string cabinClass;

        [ObservableProperty]
        static ContentView flightLeg;

        [ObservableProperty]
        Navigator<Leg> nav;

        [RelayCommand]
        async Task AddFlightLeg()
        {
            if (CheckNullLegValues())
            {
                Shell.Current.DisplayAlert("Empty fields", "You have left fields blank, please enter a value", "OK");
                return;
            }
            else
            {
                Legs.Add(AddLeg());
                ClearLegValues();
                Nav.MoveNext();
            }
        }

        [RelayCommand]
        async Task EditFlightLeg()
        {
            Nav.Current.departure_airport = DepartureAirport;
            Nav.Current.destination_airport = ArrivalAirport;
            Nav.Current.cabin_class = CabinClass;
        }

        [RelayCommand]
        async Task DeleteFlightLeg()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Nav.DeleteCurrent();
            });
            
            DepartureAirport = Nav.Current.departure_airport;
            ArrivalAirport = Nav.Current.destination_airport;
            CabinClass = Nav.Current.cabin_class;
        }

        [RelayCommand]
        async Task GoToPreviousLeg()
        {
            if (Nav.Index < Legs.Count - 1)
            {
                Nav.MovePrevious();
                DepartureAirport = Nav.Current.departure_airport;
                ArrivalAirport = Nav.Current.destination_airport;
                CabinClass = Nav.Current.cabin_class;
            }
            else
            {
                DepartureAirport = Nav.Current.departure_airport;
                ArrivalAirport = Nav.Current.destination_airport;
                CabinClass = Nav.Current.cabin_class;
                Nav.MovePrevious();
            }
        }

        [RelayCommand]
        async Task GoToNextLeg()
        {
            if(Nav.Index < Legs.Count-1)
            {
                Nav.MoveNext();
                DepartureAirport = Nav.Current.departure_airport;
                ArrivalAirport = Nav.Current.destination_airport;
                CabinClass = Nav.Current.cabin_class;
            }
            else
            {
                DepartureAirport = null;
                ArrivalAirport = null;
                CabinClass = null;
            }
            
        }

        FlightRequest ReturnFlight()
        {
            FlightRequest flight = new FlightRequest();
            flight.type = "flight";
            flight.passengers = Passengers;
            List<LegRequest> legRequests = new List<LegRequest>();
            foreach (Leg leg in Legs)
            {
                LegRequest legRequest = new LegRequest();
                legRequest.departure_airport = leg.departure_airport.iata_code.ToLower();
                legRequest.destination_airport = leg.destination_airport.iata_code.ToLower();
                legRequest.cabin_class = leg.cabin_class.ToLower();
                legRequests.Add(legRequest);
            }
            flight.legs = legRequests.ToArray();

            FlightLegs = flight.legs.Count();
            return flight;
        }

        async Task GetFlightsAsync()
        {
            FlightRequest flight = ReturnFlight();

            try
            {
                IsBusy = true;
                IsContentVisible = false;

                List<Flight> flights = new();
                flights = await _carbonService.GetFlights(flight);

                if (Flights.Count != 0)
                    Flights.Clear();

                foreach (var vehicle in flights)
                    Flights.Add(vehicle);
                flights.Clear(); //for some reason flights still had values from previous requests despite leaving scope...
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Flight information {ex.Message}", "OK");
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
            await GetFlightsAsync();
            try
            {
                IsBusy = true;
                IsContentVisible = false;

                if (WeightUnit == "g")
                {
                    CarbonGenerated = Flights.FirstOrDefault().data.attributes.carbon_g;
                }
                else if (WeightUnit == "lb")
                {
                    CarbonGenerated = Flights.FirstOrDefault().data.attributes.carbon_lb;

                }
                else if (WeightUnit == "kg")
                {
                    CarbonGenerated = Flights.FirstOrDefault().data.attributes.carbon_kg;

                }
                else if (WeightUnit == "mt")
                {
                    CarbonGenerated = Flights.FirstOrDefault().data.attributes.carbon_mt;

                }

                foreach (Flight f in Flights)
                {
                    Debug.WriteLine(f.data.attributes.carbon_g);
                }
                await Shell.Current.GoToAsync($"/{nameof(Screens.CarbonCalculationResults)}", true);
            }
            catch (Exception ex)
            {

                await Shell.Current.DisplayAlert("Error!",
                    $"Unable to get Flight information {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }
        }

        bool CheckNullLegValues()
        {
            bool check = false;

            if (string.IsNullOrEmpty(DepartureAirport?.name) || string.IsNullOrEmpty(ArrivalAirport?.name) || string.IsNullOrEmpty(CabinClass))
            {
                check = true;
            }

            return check;
        }

        Leg AddLeg()
        {
            Leg leg = new Leg();
            leg.departure_airport = DepartureAirport;
            leg.destination_airport = ArrivalAirport;
            leg.cabin_class = CabinClass;
            return leg;
        }

        void ClearLegValues()
        {
            DepartureAirport = null;
            ArrivalAirport = null;
            CabinClass = null;
        }

        void ClearFlightValues()
        {
            ClearLegValues();
            Nav = new Navigator<Leg>(Legs);
            Legs.Clear();
        }

        async Task PopulateAiports()
        {
            try
            {
                IsBusy = true;
                IsContentVisible = false;
                List<Airport> airportsList = (await AirportHelper.LoadAllAirports().ConfigureAwait(false)).ToList();

                if (airportsList.Count > 9000)
                {
                    if (AirportsList.Count < 9000)
                    {
                        foreach (Airport a in airportsList)
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                AirportsList.Add(a);
                            });
                        }
                    }
                }
                else //else the db hasn't got > 9000 and need to retrieve them via api
                {
                    List<Airport> airportList = await _carbonService.GetAirports().ConfigureAwait(false);

                    AirportHelper airportHelper = new();
                    foreach (Airport a in airportList)
                    {
                        //again this if should be unnecessary, but this avoids having double entries
                        if (!AirportsList.Contains(a))
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                AirportsList.Add(a);
                            });
                            await airportHelper.CreateNewAirport(a);
                        }
                    }
                }
            }
            finally
            {
                IsBusy = false;
                IsContentVisible = true;
            }

            Debug.WriteLine($"Airports loaded: {AirportsList.Count}");

        }
    }
}
