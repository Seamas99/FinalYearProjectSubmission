using FinalYearProject.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Services
{
    public class MapBoxService : IMapBoxService
    {
        private readonly HttpClient _httpClient = new();
        private const string BaseUrl = "https://carbon-proxy-404544626195.us-central1.run.app";


        public async Task<List<MapBoxSuggestion>> GetCitySuggestionsAsync(string query, string country)
        {
            if (string.IsNullOrWhiteSpace(query)) return new();

            string types = "place, locality";
            string url = BaseUrl + $"/city/{query}/{country}/{types}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<MapBoxResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return data?.Suggestions ?? new();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Request failed", "OK");
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}: {body}");
            }
        }

        public async Task<List<MapBoxSuggestion>> GetRegionSuggestionsAsync(string query, string country)
        {
            if (string.IsNullOrWhiteSpace(query)) return new();

            string types = "region";
            string url = BaseUrl + $"/region/{query}/{country}/{types}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<MapBoxResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return data?.Suggestions ?? new();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Request failed", "OK");
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}: {body}");
            }
        }

        public async Task<List<MapBoxSuggestion>> GetDistrictSuggestionsAsync(string query, string country)
        {
            if (string.IsNullOrWhiteSpace(query)) return new();

            string types = "district";
            string url = BaseUrl + $"/district/{query}/{country}/{types}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<MapBoxResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return data?.Suggestions ?? new();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Request failed", "OK");
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}: {body}");
            }
        }

        public async Task<List<MapBoxSuggestion>> GetPostcodeSuggestionsAsync(string query, string country)
        {
            if (string.IsNullOrWhiteSpace(query)) return new();

            string types = "postcode";
            string url = BaseUrl + $"/postcode/{query}/{country}/{types}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<MapBoxResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return data?.Suggestions ?? new();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error!",
                    $"Request failed", "OK");
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}: {body}");
            }
        }
    }
}
