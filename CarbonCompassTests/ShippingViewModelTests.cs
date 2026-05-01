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
    public class ShippingViewModelTests
    {
        private readonly Mock<ICarbonService> _mockCarbonService;
        private readonly Mock<ISettingsService> _mockSettingsService;

        public ShippingViewModelTests()
        {
            _mockCarbonService = new Mock<ICarbonService>();
            _mockSettingsService = new Mock<ISettingsService>();

            //avoid null reference
            var defaultResponse = new List<Shipping>
            {
                new Shipping
                {
                    data = new ShippingData
                    {
                        attributes = new ShippingAttributes { carbon_g = 0f }
                    }
                }
            };

            _mockCarbonService
                .Setup(s => s.GetShipping(It.IsAny<ShippingRequest>()))
                .ReturnsAsync(defaultResponse);
        }

        //create SystemUnderTest (SUT)
        private ShippingViewModel CreateSut()
        {
            return new ShippingViewModel(_mockCarbonService.Object, _mockSettingsService.Object);
        }

        [Fact]
        public void Constructor_WhenInstantiated_InitializesTransportMethodsProperly()
        {
            var sut = CreateSut();

            Assert.NotNull(sut.TransportMethods);
            Assert.Equal(4, sut.TransportMethods.Count);
            Assert.Contains("Ship", sut.TransportMethods);
            Assert.Contains("Train", sut.TransportMethods);
            Assert.Contains("Truck", sut.TransportMethods);
            Assert.Contains("Plane", sut.TransportMethods);
        }

        [Fact]
        public void Constructor_WhenInstantiated_SetsDefaultNumericValuesToZero()
        {
            var sut = CreateSut();

            Assert.Equal(0f, sut.CarbonGenerated);
            Assert.Equal(0, sut.Distance);
            Assert.Equal(0, sut.WeightValue);
        }

        [Fact]
        public void Constructor_WhenInstantiated_InitializesEmptyShippingList()
        {
            var sut = CreateSut();

            Assert.NotNull(sut.ShippingList);
            Assert.Empty(sut.ShippingList);
        }

        //calc footprnt command tests
        [Fact]
        public async Task CalculateFootprintCommand_WhenExecuted_MapsPropertiesToShippingRequestCorrectly()
        {
            var sut = CreateSut();
            sut.Distance = 300;
            sut.WeightValue = 25;
            sut.TransportMethod = "Train";

            ShippingRequest capturedRequest = null;
            _mockCarbonService
                .Setup(s => s.GetShipping(It.IsAny<ShippingRequest>()))
                .Callback<ShippingRequest>(r => capturedRequest = r)
                .ReturnsAsync(new List<Shipping> { new Shipping { data = new ShippingData { attributes = new ShippingAttributes() } } });

            
            try { await sut.CalculateFootprintCommand.ExecuteAsync(null); } catch { }

            Assert.NotNull(capturedRequest);
            Assert.Equal("shipping", capturedRequest.type);
            Assert.Equal(300, capturedRequest.distance_value);
            Assert.Equal("km", capturedRequest.distance_unit);
            Assert.Equal(25, capturedRequest.weight_value);
            Assert.Equal("kg", capturedRequest.weight_unit);
        }

        [Theory]
        [InlineData("Ship", "ship")]
        [InlineData("Train", "train")]
        [InlineData("Truck", "truck")]
        [InlineData("Plane", "plane")]
        [InlineData("MIXEDcase", "mixedcase")] //mixed case to test tolower works correctly
        public async Task CalculateFootprintCommand_WhenExecuted_SendsTransportMethodLowerCased(string inputMethod, string expectedMethod)
        {
            var sut = CreateSut();
            sut.Distance = 100;
            sut.WeightValue = 10;
            sut.TransportMethod = inputMethod;

            ShippingRequest capturedRequest = null;
            _mockCarbonService
                .Setup(s => s.GetShipping(It.IsAny<ShippingRequest>()))
                .Callback<ShippingRequest>(r => capturedRequest = r)
                .ReturnsAsync(new List<Shipping> { new Shipping { data = new ShippingData { attributes = new ShippingAttributes() } } });

            try { await sut.CalculateFootprintCommand.ExecuteAsync(null); } catch { }

            Assert.NotNull(capturedRequest);
            Assert.Equal(expectedMethod, capturedRequest.transport_method);
        }

        [Fact]
        public async Task CalculateFootprintCommand_WhenServiceReturnsData_SetsCarbonGenerated()
        {
            var expectedCarbon = 4321f;
            var sut = CreateSut();
            sut.Distance = 100;
            sut.WeightValue = 10;
            sut.TransportMethod = "Train";

            var mockResponse = new List<Shipping>
            {
                new Shipping
                {
                    data = new ShippingData
                    {
                        attributes = new ShippingAttributes { carbon_g = expectedCarbon }
                    }
                }
            };

            _mockCarbonService
                .Setup(s => s.GetShipping(It.IsAny<ShippingRequest>()))
                .ReturnsAsync(mockResponse);

            try { await sut.CalculateFootprintCommand.ExecuteAsync(null); } catch { }

            Assert.Equal(expectedCarbon, sut.CarbonGenerated);
            Assert.Single(sut.ShippingList); //checks item was added to selection
        }
    }
}
