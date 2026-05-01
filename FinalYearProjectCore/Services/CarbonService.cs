using FinalYearProjectCore.Interfaces;
using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Services
{
    public class CarbonService : ICarbonService
    {
        HttpClient _httpClient;
        private readonly FirebaseAuthClient _authClient;
        private const string BaseUrl = "https://carbon-proxy-404544626195.us-central1.run.app";
        private const string ProxyBaseUrl = "https://carbon-proxy-404544626195.us-central1.run.app/proxy";
        private const string ProxyAirportsUrl = "https://carbon-proxy-404544626195.us-central1.run.app/airports";

        private readonly JsonSerializerOptions _jsonIgnoreNullOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private readonly JsonSerializerOptions _jsonCaseSensitivityOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public CarbonService(HttpClient httpClient, FirebaseAuthClient authClient)
        {
            _httpClient = httpClient;
            _authClient = authClient;
        }

        List<Vehicle> vehicleList = new();
        List<Flight> flightList = new();
        List<Electricity> electricityList = new();
        List<Shipping> shippingList = new();
        List<VehicleMake> vehicleMakesList = new();
        List<VehicleModel> vehicleModelsList = new();
        List<Airport> airportList = new();

        public async Task<List<Airport>> GetAirports()
        {
            if (airportList?.Count > 9000) //Temporary: Find a better way
                return airportList;

            var user = _authClient?.User;
            if (user == null)
            {
                throw new Exception("User is not logged in.");
            }

            var token = await user.GetIdTokenAsync(forceRefresh: false);
            var request = new HttpRequestMessage(HttpMethod.Get, ProxyAirportsUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                airportList = await response.Content.ReadFromJsonAsync<List<Airport>>();
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}: {body}");
            }

            return airportList;
        }

        public async Task<List<VehicleMake>> GetVehicleMakes()
        {
            if (vehicleMakesList?.Count == 138) //Temporary: Find a better way
                return vehicleMakesList;

            var user = _authClient?.User;
            if (user == null)
            {
                throw new Exception("User is not logged in.");
            }

            var token = await user.GetIdTokenAsync(forceRefresh: false);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ProxyBaseUrl}/vehicle_makes");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                vehicleMakesList = await response.Content.ReadFromJsonAsync<List<VehicleMake>>();
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Cloud Run rejected the request: {response.StatusCode}: {body}");
            }


            return vehicleMakesList;
        }

        public async Task<List<VehicleModel>> GetVehicleModels(string vehicleMakeID)
        {
            var vehicleModelsUrl = $"{ProxyBaseUrl}/vehicle_makes/{vehicleMakeID}/vehicle_models";

            var user = _authClient?.User;
            if (user == null)
            {
                throw new Exception("User is not logged in.");
            }

            var token = await user.GetIdTokenAsync(forceRefresh: false);
            var request = new HttpRequestMessage(HttpMethod.Get, vehicleModelsUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    vehicleModelsList = await response.Content.ReadFromJsonAsync<List<VehicleModel>>();
                }
                catch (JsonException jex)
                {
                    Debug.WriteLine($"JSON Error: {jex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"General Error: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"General Error: {response.StatusCode}");
            }

            return vehicleModelsList;
        }

        public async Task<List<Vehicle>> GetVehicles(string modelID, string distanceUnit, int distanceValue)
        {
            var estimateUrl = $"{ProxyBaseUrl}/estimates";

            var user = _authClient?.User;
            if (user == null)
            {
                throw new Exception("User is not logged in.");
            }

            var token = await user.GetIdTokenAsync(forceRefresh: false);

            VehicleRequest vehicleRequest = new VehicleRequest();
            vehicleRequest.type = "vehicle";
            vehicleRequest.distance_unit = distanceUnit;
            vehicleRequest.distance_value = distanceValue;
            vehicleRequest.vehicle_model_id = modelID;

            using var request = new HttpRequestMessage(HttpMethod.Post, estimateUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(vehicleRequest);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Vehicle vehicle = await response.Content.ReadFromJsonAsync<Vehicle>();
                vehicleList.Add(vehicle);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
            }

            return vehicleList;
        }

        public async Task<List<Flight>> GetFlights(FlightRequest flightRequest)
        {
            var estimateUrl = $"{ProxyBaseUrl}/estimates";

            var user = _authClient?.User;
            if (user == null)
            {
                throw new Exception("User is not logged in.");
            }

            var token = await user.GetIdTokenAsync(forceRefresh: false);

            using var request = new HttpRequestMessage(HttpMethod.Post, estimateUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(flightRequest);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Flight flight = await response.Content.ReadFromJsonAsync<Flight>();
                flightList.Add(flight);
            }

            return flightList;
        }

        public async Task<List<Electricity>> GetElectricity(ElectricityRequest electricityRequest)
        {
            var estimateUrl = $"{ProxyBaseUrl}/estimates";

            var user = _authClient?.User;
            if (user == null)
            {
                throw new Exception("User is not logged in.");
            }

            var token = await user.GetIdTokenAsync(forceRefresh: false);

            using var request = new HttpRequestMessage(HttpMethod.Post, estimateUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(electricityRequest, inputType: typeof(ElectricityRequest), options: _jsonIgnoreNullOptions);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Electricity electricity = await response.Content.ReadFromJsonAsync<Electricity>();
                electricityList.Add(electricity);
            }

            return electricityList;
        }

        public async Task<List<Shipping>> GetShipping(ShippingRequest shippingRequest)
        {
            var estimateUrl = $"{ProxyBaseUrl}/estimates";

            var user = _authClient?.User;
            if (user == null)
            {
                throw new Exception("User is not logged in.");
            }

            var token = await user.GetIdTokenAsync(forceRefresh: false);

            using var request = new HttpRequestMessage(HttpMethod.Post, estimateUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(shippingRequest, inputType: typeof(ShippingRequest), options: _jsonIgnoreNullOptions);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Shipping shipping = await response.Content.ReadFromJsonAsync<Shipping>();
                shippingList.Add(shipping);
            }

            return shippingList;
        }

        public async Task<UpdateFootprintsResponseDTO> AddFootprintAsync(UpdateFootprintsDTO dto, DateTime enteredDate)
        {
            string link = $"{BaseUrl}/AddFootprint";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(link, dto);
                var rawBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(rawBody);

                response.EnsureSuccessStatusCode();

                UpdateFootprintsResponseDTO returnedValue = await response.Content.ReadFromJsonAsync<UpdateFootprintsResponseDTO>(_jsonCaseSensitivityOption);


                return returnedValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return null;
            }
        }

        public async Task<UpdateFootprintsResponseDTO> RemoveFootprint(UpdateFootprintsDTO dto, DateTime enteredDate)
        {
            string link = $"{BaseUrl}/RemoveFootprint";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(link, dto);
                var rawBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(rawBody);

                response.EnsureSuccessStatusCode();

                UpdateFootprintsResponseDTO returnedValue = await response.Content.ReadFromJsonAsync<UpdateFootprintsResponseDTO>(_jsonCaseSensitivityOption);

                return returnedValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex}");
                return null;
            }
        }
    }
}
