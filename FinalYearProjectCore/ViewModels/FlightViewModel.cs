using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalYearProjectCore.Database;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Services;
using FinalYearProjectCore.Interfaces;

namespace FinalYearProjectCore.ViewModels
{
    public partial class FlightViewModel : BaseViewModel
    {
        private readonly ICarbonService _carbonService;

        public FlightViewModel(ICarbonService carbonService)
        {
            _carbonService = carbonService;
            Nav = new Navigator<Leg>(Legs);
            PopulateAiports();
        }

        public ObservableCollection<Leg> Legs { get; } = new();
        public ObservableCollection<Flight> Flights { get; } = new();
        public ObservableCollection<string> CabinClasses { get; } = ["Economy", "Premium"];
        public ObservableCollection<Airport> AirportsList { get; } = new();
        public ObservableCollection<Airport> FilteredAirportsList { get; } = new();

        [ObservableProperty]
        float carbonGenerated;

        [ObservableProperty]
        Flight selectedFlight;

        [ObservableProperty]
        int passengers;

        [ObservableProperty]
        Airport departureAirport;

        [ObservableProperty]
        Airport arrivalAirport;

        [ObservableProperty]
        string cabinClass;


        [ObservableProperty]
        Navigator<Leg> nav;

        [RelayCommand]
        async Task AddFlightLeg()
        {
            if (CheckNullLegValues())
            {
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
                Nav.DeleteCurrent();
            
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

            return flight;
        }

        async Task GetFlightsAsync()
        {
            FlightRequest flight = ReturnFlight();

            if (IsBusy)
                return;

            try
            {
                IsBusy = false;

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
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task CalculateFootprint()
        {
            await GetFlightsAsync();
            try
            {
                CarbonGenerated = Flights.FirstOrDefault().data.attributes.carbon_g;
                foreach (Flight f in Flights)
                {
                    Debug.WriteLine(f.data.attributes.carbon_g);
                }
            }
            catch (Exception ex)
            {
            }
        }

        bool CheckNullLegValues()
        {
            bool check = false;
            if((string.IsNullOrEmpty(DepartureAirport.name) || DepartureAirport == null) || (string.IsNullOrEmpty(ArrivalAirport.name) || ArrivalAirport == null) || string.IsNullOrEmpty(CabinClass))
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
            List<Airport> airportsList = (await AirportHelper.LoadAllAirports().ConfigureAwait(false)).ToList();

            if (airportsList.Count > 9000) 
            {
                if (AirportsList.Count < 9000)
                {
                    foreach (Airport a in airportsList)
                    {
                            AirportsList.Add(a);
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
                            AirportsList.Add(a);
                        await airportHelper.CreateNewAirport(a);
                    }
                }
            }

            Debug.WriteLine($"Airports loaded: {AirportsList.Count}");

        }
    }
}
