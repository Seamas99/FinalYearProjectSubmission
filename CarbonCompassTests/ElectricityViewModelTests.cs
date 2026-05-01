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
    public class ElectricityViewModelTests
    {
        private readonly Mock<ICarbonService> _mockCarbonService;

        public ElectricityViewModelTests()
        {
            _mockCarbonService = new Mock<ICarbonService>();

            //avoid null reference
            var defaultResponse = new List<Electricity>
            {
                new Electricity
                {
                    data = new ElectricityData
                    {
                        attributes = new ElectricityAttributes { carbon_g = 0f }
                    }
                }
            };

            _mockCarbonService
                .Setup(s => s.GetElectricity(It.IsAny<ElectricityRequest>()))
                .ReturnsAsync(defaultResponse);
        }

        //create SystemUnderTest (SUT)
        private ElectricityViewModel CreateSut()
        {
            return new ElectricityViewModel(_mockCarbonService.Object);
        }

        [Fact]
        public void Constructor_WhenInstantiated_SetsDefaultNumericValuesToZero()
        {
            var sut = CreateSut();

            Assert.Equal(0f, sut.CarbonGenerated);
            Assert.Equal(0, sut.ElectricityUsed);
        }

        [Fact]
        public void Constructor_WhenInstantiated_InitializesEmptyElectricityList()
        {
            var sut = CreateSut();

            Assert.NotNull(sut.ElectricityList);
            Assert.Empty(sut.ElectricityList);
        }

        //property tests

        [Theory]
        [InlineData(100)]
        [InlineData(5000)]
        public void ElectricityUsed_WhenSet_UpdatesPropertyCorrectly(int usage)
        {
            var sut = CreateSut();

            sut.ElectricityUsed = usage;

            Assert.Equal(usage, sut.ElectricityUsed);
        }

        //calculate footprint tests
        [Fact]
        public async Task CalculateFootprintCommand_WhenExecuted_MapsPropertiesToElectricityRequestCorrectly()
        {
            var sut = CreateSut();
            sut.ElectricityUsed = 350;

            ElectricityRequest capturedRequest = null;
            _mockCarbonService
                .Setup(s => s.GetElectricity(It.IsAny<ElectricityRequest>()))
                .Callback<ElectricityRequest>(r => capturedRequest = r)
                .ReturnsAsync(new List<Electricity> { new Electricity { data = new ElectricityData { attributes = new ElectricityAttributes() } } });

            
            try { await sut.CalculateFootprintCommand.ExecuteAsync(null); } catch { }

            Assert.NotNull(capturedRequest);
            Assert.Equal("electricity", capturedRequest.type);
            Assert.Equal(350, capturedRequest.electricity_value);
            Assert.Equal("gb", capturedRequest.country);
        }

        [Fact]
        public async Task CalculateFootprintCommand_WhenServiceReturnsData_UpdatesPropertiesAndCollection()
        {
            var expectedCarbon = 7777f;
            var sut = CreateSut();
            sut.ElectricityUsed = 100;

            var mockResponse = new List<Electricity>
            {
                new Electricity
                {
                    data = new ElectricityData
                    {
                        attributes = new ElectricityAttributes { carbon_g = expectedCarbon }
                    }
                }
            };

            _mockCarbonService
                .Setup(s => s.GetElectricity(It.IsAny<ElectricityRequest>()))
                .ReturnsAsync(mockResponse);

            try { await sut.CalculateFootprintCommand.ExecuteAsync(null); } catch { }

            Assert.Equal(expectedCarbon, sut.CarbonGenerated);
            Assert.Single(sut.ElectricityList); //checks item was added
        }
    }
}