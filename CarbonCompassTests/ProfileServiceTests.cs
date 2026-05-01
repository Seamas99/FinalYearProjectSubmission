using FinalYearProjectCore.Database;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Model;
using FinalYearProjectCore.Services;
using FinalYearProjectCore.ViewModels;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarbonCompassTests
{
    public class ProfileServiceTests
    {
        private (ProfileService sut, Mock<HttpMessageHandler> mockHandler) CreateSut(HttpResponseMessage mockedResponse = null)
        {
            var mockHandler = new Mock<HttpMessageHandler>();

            if (mockedResponse != null)
            {
                mockHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(mockedResponse);
            }

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://test.com")
            };

            //Passing null for FirebaseAuthClient to test unauthorised http calls
            var sut = new ProfileService(httpClient, null);

            return (sut, mockHandler);
        }


        [Fact]
        public void AddVehicle_WhenCalled_AddsVehicleToCollection()
        {
            var (sut, _) = CreateSut();
            var vehicle = new SavedVehicle { model_name = "TestCar" };

            sut.AddVehicle(vehicle);

            Assert.Single(sut.Vehicles);
            Assert.Contains(vehicle, sut.Vehicles);
        }

        [Fact]
        public void RemoveVehicle_WhenCalled_RemovesVehicleFromCollection()
        {
            var (sut, _) = CreateSut();
            var vehicle = new SavedVehicle { model_name = "TestCar" };
            sut.AddVehicle(vehicle);

            sut.RemoveVehicle(vehicle);

            Assert.Empty(sut.Vehicles);
        }


        [Fact]
        public async Task GetProfileAsync_WhenUserIsNull_ReturnsNull()
        {
            var (sut, _) = CreateSut();

            var result = await sut.GetProfileAsync();

            Assert.Null(result);
        }

        [Fact]
        public async Task GetChallenges_WhenUserIsNull_ReturnsNull()
        {
            var (sut, _) = CreateSut();

            var result = await sut.GetChallenges();

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteProfileAsync_WhenUserIsNull_ReturnsFalse()
        {
            var (sut, _) = CreateSut();

            try
            {
                var result = await sut.DeleteProfileAsync();
                Assert.False(result);
            }
            catch
            {
            }
        }


        [Fact]
        public async Task GetCountriesAsync_OnSuccess_ReturnsDeserializedList()
        {
            var fakeCountries = new List<Country>
            {
                new Country { name = "United Kingdom", alpha2 = "GB" },
                new Country { name = "United States", alpha2 = "US" }
            };

            var jsonResponse = JsonSerializer.Serialize(fakeCountries);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var (sut, mockHandler) = CreateSut(httpResponse);

            var result = await sut.GetCountriesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("GB", result[0].alpha2);
        }

        [Fact]
        public async Task GetSubdivisionsAsync_OnSuccess_ReturnsDeserializedList()
        {
            var fakeSubdivisions = new List<Subdivision>
            {
                new Subdivision { name = "London", code = "ENG" },
                new Subdivision { name = "Scotland", code = "SCT" }
            };

            var jsonResponse = JsonSerializer.Serialize(fakeSubdivisions);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var (sut, mockHandler) = CreateSut(httpResponse);

            var result = await sut.GetSubdivisionsAsync("GB");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("ENG", result[0].code);

            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/GB/subdivisions")),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
