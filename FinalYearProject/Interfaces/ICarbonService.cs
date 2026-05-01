using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Interfaces
{
    public interface ICarbonService
    {
        Task<List<VehicleMake>> GetVehicleMakes();
        Task<List<VehicleModel>> GetVehicleModels(string vehicleMakeID);
        Task<List<Vehicle>> GetVehicles(string modelID, string distanceUnit, int distanceValue);
        Task<List<Airport>> GetAirports();
        Task<List<Flight>> GetFlights(FlightRequest flightRequest);
        Task<List<Electricity>> GetElectricity(ElectricityRequest electricityRequest);
        Task<List<Shipping>> GetShipping(ShippingRequest shippingRequest);
        Task<UpdateFootprintsResponseDTO> AddFootprintAsync(UpdateFootprintsDTO updateFootprintsDTO, DateTime enteredDate);
        Task<UpdateFootprintsResponseDTO> RemoveFootprint(UpdateFootprintsDTO dto, DateTime enteredDate);
    }
}
