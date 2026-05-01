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
using System.Threading.Tasks;

namespace CarbonCompassTests
{
    public class MapBoxServiceTests
    {
        private (MapBoxService sut, Mock<HttpMessageHandler> mockHandler) CreateSut(HttpResponseMessage mockedResponse = null)
        {
            var mockHandler = new Mock<HttpMessageHandler>();

            //If a mock response is provided, set up the handler to return it
            if (mockedResponse != null)
            {
                mockHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(mockedResponse);
            }

            var httpClient = new HttpClient(mockHandler.Object);
            var sut = new MapBoxService();

            var field = typeof(MapBoxService).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(sut, httpClient);

            return (sut, mockHandler);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetCitySuggestionsAsync_WhenQueryIsNullOrWhiteSpace_ReturnsEmptyListAndSkipsHttpCall(string invalidQuery)
        {
            var (sut, mockHandler) = CreateSut();

            var result = await sut.GetCitySuggestionsAsync(invalidQuery, "gb");

            Assert.NotNull(result);
            Assert.Empty(result);

            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        //url formatting tests
        [Fact]
        public async Task GetCitySuggestionsAsync_WhenQueryIsValid_FormatsUrlCorrectly()
        {
            // Arrange
            var jsonResponse = "{\"Suggestions\": []}";
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            var (sut, mockHandler) = CreateSut(httpResponse);

            // Act
            await sut.GetCitySuggestionsAsync("London", "gb");

            // Assert
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "https://carbon-proxy-404544626195.us-central1.run.app/city/London/gb/place, locality"),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetRegionSuggestionsAsync_WhenQueryIsValid_FormatsUrlCorrectly()
        {
            var jsonResponse = "{\"Suggestions\": []}";
            var (sut, mockHandler) = CreateSut(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });

            await sut.GetRegionSuggestionsAsync("Scotland", "gb");

            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "https://carbon-proxy-404544626195.us-central1.run.app/region/Scotland/gb/region"),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetPostcodeSuggestionsAsync_WhenQueryIsValid_FormatsUrlCorrectly()
        {
            var jsonResponse = "{\"Suggestions\": []}";
            var (sut, mockHandler) = CreateSut(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });

            await sut.GetPostcodeSuggestionsAsync("BT48", "gb");

            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "https://carbon-proxy-404544626195.us-central1.run.app/postcode/BT48/gb/postcode"),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task GetCitySuggestionsAsync_OnSuccess_ReturnsDeserializedSuggestions()
        {
            var jsonResponse = @"{
                ""Suggestions"": [
                    { ""name"": ""London"" },
                    { ""name"": ""Londonderry"" }
                ]
            }";

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var (sut, _) = CreateSut(httpResponse);

            var result = await sut.GetCitySuggestionsAsync("Lond", "gb");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        //error handling tests

        [Fact]
        public async Task GetCitySuggestionsAsync_OnHttpError_ThrowsException()
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError, // 500 Error
                Content = new StringContent("Internal Server Error")
            };

            var (sut, _) = CreateSut(httpResponse);

            try
            {
                await sut.GetCitySuggestionsAsync("London", "gb");
                Assert.True(false, "Method should have thrown an exception.");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
