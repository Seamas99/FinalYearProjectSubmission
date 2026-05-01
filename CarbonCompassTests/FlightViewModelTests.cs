using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalYearProjectCore.Services;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Model;
using FinalYearProjectCore.ViewModels;
using FinalYearProjectCore.Database;

namespace CarbonCompassTests
{
    public class FlightViewModelTests
    {
        private readonly Mock<ICarbonService> _mockCarbonService;

        public FlightViewModelTests()
        {
            _mockCarbonService = new Mock<ICarbonService>();

            //avoid null reference
            _mockCarbonService.Setup(s => s.GetAirports())
                .ReturnsAsync(new List<Airport>());

            var defaultFlightResponse = new List<Flight>
            {
                new Flight
                {
                    data = new FlightData
                    {
                        attributes = new FlightAttributes { carbon_g = 0f }
                    }
                }
            };

            _mockCarbonService.Setup(s => s.GetFlights(It.IsAny<FlightRequest>()))
                .ReturnsAsync(defaultFlightResponse);
        }
        //create SystemUnderTest (SUT)
        private FlightViewModel CreateSut()
        {
            return new FlightViewModel(_mockCarbonService.Object);
        }


        [Fact]
        public void Constructor_WhenInstantiated_InitializesCabinClassesProperly()
        {
            var sut = CreateSut();

            Assert.NotNull(sut.CabinClasses);
            Assert.Equal(2, sut.CabinClasses.Count);
            Assert.Contains("Economy", sut.CabinClasses);
            Assert.Contains("Premium", sut.CabinClasses);
        }

        [Fact]
        public void Constructor_WhenInstantiated_InitializesEmptyCollections()
        {
            var sut = CreateSut();

            Assert.NotNull(sut.Legs);
            Assert.Empty(sut.Legs);

            Assert.NotNull(sut.Flights);
            Assert.Empty(sut.Flights);
        }

        //add flight leg command tests

        [Fact]
        public async Task AddFlightLegCommand_WhenFieldsAreMissing_DoesNotAddLeg()
        {
            var sut = CreateSut();

            //test with a missing departure airport but valid arrival and cabin
            sut.DepartureAirport = null;
            sut.ArrivalAirport = new Airport { name = "JFK", iata_code = "JFK" };
            sut.CabinClass = "Economy";

            try { await sut.AddFlightLegCommand.ExecuteAsync(null); } catch { }

            Assert.Empty(sut.Legs);
        }

        [Fact]
        public async Task AddFlightLegCommand_WhenAllFieldsValid_AddsLegAndClearsInputs()
        {
            var sut = CreateSut();
            sut.DepartureAirport = new Airport { name = "Heathrow", iata_code = "LHR" };
            sut.ArrivalAirport = new Airport { name = "JFK", iata_code = "JFK" };
            sut.CabinClass = "Economy";

            try { await sut.AddFlightLegCommand.ExecuteAsync(null); } catch { }

            //check leg was added
            Assert.Single(sut.Legs);
            Assert.Equal("LHR", sut.Legs[0].departure_airport.iata_code);
            Assert.Equal("JFK", sut.Legs[0].destination_airport.iata_code);

            //check fields cleared
            Assert.Null(sut.DepartureAirport);
            Assert.Null(sut.ArrivalAirport);
            Assert.Null(sut.CabinClass);
        }

        //calculate footprint command tests

        [Fact]
        public async Task CalculateFootprintCommand_WhenExecuted_BuildsRequestWithCorrectPassengers()
        {
            var sut = CreateSut();
            sut.Passengers = 3;

            //setup valid leg
            sut.DepartureAirport = new Airport { name = "Heathrow", iata_code = "LHR" };
            sut.ArrivalAirport = new Airport { name = "Kennedy", iata_code = "JFK" };
            sut.CabinClass = "Economy";
            try { await sut.AddFlightLegCommand.ExecuteAsync(null); } catch { }

            FlightRequest capturedRequest = null;
            _mockCarbonService
                .Setup(s => s.GetFlights(It.IsAny<FlightRequest>()))
                .Callback<FlightRequest>(r => capturedRequest = r)
                .ReturnsAsync(new List<Flight> { new Flight { data = new FlightData { attributes = new FlightAttributes { carbon_g = 20000f } } } });

            try { await sut.CalculateFootprintCommand.ExecuteAsync(null); } catch { }

            Assert.NotNull(capturedRequest);
            Assert.Equal(3, capturedRequest.passengers);
        }

        [Fact]
        public async Task CalculateFootprintCommand_WhenExecuted_SendsIataCodesAndCabinLowerCased()
        {
            var sut = CreateSut();

            sut.DepartureAirport = new Airport { name = "Heathrow", iata_code = "LHR" };
            sut.ArrivalAirport = new Airport { name = "Kennedy", iata_code = "JFK" };
            sut.CabinClass = "Premium";
            try { await sut.AddFlightLegCommand.ExecuteAsync(null); } catch { }

            FlightRequest capturedRequest = null;
            _mockCarbonService
                .Setup(s => s.GetFlights(It.IsAny<FlightRequest>()))
                .Callback<FlightRequest>(r => capturedRequest = r)
                .ReturnsAsync(new List<Flight> { new Flight { data = new FlightData { attributes = new FlightAttributes { carbon_g = 10000f } } } });

            try { await sut.CalculateFootprintCommand.ExecuteAsync(null); } catch { }

            Assert.NotNull(capturedRequest);
            Assert.Single(capturedRequest.legs);
            Assert.Equal("lhr", capturedRequest.legs[0].departure_airport);
            Assert.Equal("jfk", capturedRequest.legs[0].destination_airport);
            Assert.Equal("premium", capturedRequest.legs[0].cabin_class); 
        }
    }
}
