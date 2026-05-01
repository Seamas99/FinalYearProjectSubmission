using FinalYearProjectCore.Database;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Model;
using FinalYearProjectCore.Services;
using FinalYearProjectCore.ViewModels;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
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
    public class CarbonServiceTests
    {

        FirebaseAuthClient authClient = new FirebaseAuthClient(new FirebaseAuthConfig()
        {
            //API key in code shouldn't matter as Firebase API keys are only used for identification of application
            ApiKey = "AIzaSyCaBu9XzItQQX_jAZ8FnL_FToKU5xJ1lH0",
            AuthDomain = "final-year-project-484713.firebaseapp.com",
            Providers = new Firebase.Auth.Providers.FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
            UserRepository = new FileUserRepository("CarbonCompass")
        });

        [Fact]
        public async Task GetAirports_WhenUserIsNotLoggedIn_ThrowsException()
        {
            var httpClient = new HttpClient();
            
            var sut = new CarbonService(httpClient, authClient);

            var exception = await Assert.ThrowsAsync<Exception>(() => sut.GetAirports());
            Assert.Equal("User is not logged in.", exception.Message);
        }

        [Fact]
        public async Task GetVehicles_WhenUserIsNotLoggedIn_ThrowsException()
        {
            var httpClient = new HttpClient();
            var sut = new CarbonService(httpClient, authClient);

            var exception = await Assert.ThrowsAsync<Exception>(() => sut.GetVehicles("model-1", "mi", 100));
            Assert.Equal("User is not logged in.", exception.Message);
        }

        [Fact]
        public void FlightRequest_WhenSerialized_FormatsPayloadCorrectly()
        {
            var request = new FlightRequest
            {
                type = "flight",
                passengers = 2,
                legs = new[]
                {
                    new LegRequest
                    {
                        departure_airport = "lhr",
                        destination_airport = "jfk",
                        cabin_class = "economy"
                    }
                }
            };

            var json = JsonSerializer.Serialize(request);

            Assert.Contains("\"type\":\"flight\"", json);
            Assert.Contains("\"passengers\":2", json);
            Assert.Contains("\"lhr\"", json);
            Assert.Contains("\"jfk\"", json);
            Assert.Contains("\"economy\"", json);
        }

        [Fact]
        public void VehicleRequest_WhenSerialized_FormatsPayloadCorrectly()
        {
            var request = new VehicleRequest
            {
                type = "vehicle",
                distance_unit = "mi",
                distance_value = 100,
                vehicle_model_id = "model-id"
            };

            var json = JsonSerializer.Serialize(request);

            Assert.Contains("\"type\":\"vehicle\"", json);
            Assert.Contains("\"distance_unit\":\"mi\"", json);
            Assert.Contains("\"distance_value\":100", json);
            Assert.Contains("\"vehicle_model_id\":\"model-id\"", json);
        }


        [Fact]
        public async Task AddFootprintAsync_OnSuccess_ReturnsDeserializedDto()
        {
            var expectedResponse = new UpdateFootprintsResponseDTO
            {
                Profile = new Profile { Id = "user-123" },
                League = new League { LeagueID = "league-abc" }
            };

            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://test.com")
            };

            var sut = new CarbonService(httpClient, null);
            var dummyDto = new UpdateFootprintsDTO();

            var result = await sut.AddFootprintAsync(dummyDto, DateTime.UtcNow);

            Assert.NotNull(result);
            Assert.NotNull(result.Profile);
            Assert.Equal("user-123", result.Profile.Id);
        }
    }
}
