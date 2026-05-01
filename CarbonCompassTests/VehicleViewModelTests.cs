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
    public class VehicleViewModelTests
    {
        private readonly Mock<ICarbonService> _mockCarbonService;
        private readonly Mock<ISettingsService> _mockSettingsService;

        public VehicleViewModelTests()
        {
            _mockCarbonService = new Mock<ICarbonService>();
            _mockSettingsService = new Mock<ISettingsService>();

            //setup default returns for the API calls to avoid NullReferenceExceptions
            _mockCarbonService.Setup(s => s.GetVehicleMakes())
                .ReturnsAsync(new List<VehicleMake>());
            _mockCarbonService.Setup(s => s.GetVehicleModels(It.IsAny<string>()))
                .ReturnsAsync(new List<VehicleModel>());
        }

        //create SystemUnderTest (SUT)
        private VehicleViewModel CreateSut()
        {
            return new VehicleViewModel(_mockCarbonService.Object, _mockSettingsService.Object);
        }


        [Fact]
        public void Constructor_WhenInstantiated_SetsTitleToVehicleCalculator()
        {
            var sut = CreateSut();

            Assert.Equal("Vehicle Calculator", sut.Title);
        }

        [Fact]
        public void Constructor_WhenInstantiated_SetsCarbonGeneratedToZero()
        {
            var sut = CreateSut();

            Assert.Equal(0f, sut.CarbonGenerated);
        }

        [Fact]
        public void Constructor_WhenInstantiated_SetsDistanceTravelledToZero()
        {
            var sut = CreateSut();

            Assert.Equal(0, sut.DistanceTravelled);
        }

        [Fact]
        public void Constructor_WhenInstantiated_InitializesCollectionsAsEmpty()
        {
            var sut = CreateSut();

            Assert.NotNull(sut.Vehicles);
            Assert.Empty(sut.Vehicles);

            Assert.NotNull(sut.VehicleMakes);
            Assert.Empty(sut.VehicleMakes);

            Assert.NotNull(sut.SelectedMakeListOfVehicleModels);
            Assert.Empty(sut.SelectedMakeListOfVehicleModels);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(500)]
        public void DistanceTravelled_WhenSet_UpdatesPropertyCorrectly(int distance)
        {
            var sut = CreateSut();

            sut.DistanceTravelled = distance;

            Assert.Equal(distance, sut.DistanceTravelled);
        }

        [Theory]
        [InlineData(15.5f)]
        [InlineData(100.2f)]
        public void CarbonGenerated_WhenSet_UpdatesPropertyCorrectly(float carbon)
        {
            var sut = CreateSut();

            sut.CarbonGenerated = carbon;

            Assert.Equal(carbon, sut.CarbonGenerated);
        }
    }
}
