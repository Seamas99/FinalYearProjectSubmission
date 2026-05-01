using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Database
{
    public class VehicleModelHelper
    {
        private static readonly string InsertVehicleMake = "INSERT INTO vehicle_model(id, vehicle_make_id) VALUES (@ModelID, @VehicleMakeID)";
        private static readonly string InsertVehicleMakeAttributes = "INSERT INTO vehicle_model_attributes(vehicle_model_id, vehicle_name, year, vehicle_make) VALUES (@ModelID, @ModelName, @Year, @VehicleMake)";

        public static async Task<IEnumerable<FullVehicleModel>> LoadAllFullVehicleModels()
        {
            var db = new DatabaseMain();
            db.Init();
            await db.CreateTableAsync<FullVehicleModel>();
            IEnumerable<FullVehicleModel> data = await db.GetAllAsync<FullVehicleModel>();

            return data;
        }

        public async Task<bool> CreateNewVehicleModel(FullVehicleModel Model)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.InsertAsync(Model);


                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public static FullVehicleModel ReturnFullVehicleModel(VehicleModel Model)
        {
            FullVehicleModel fullVehicleModel = new FullVehicleModel();
            fullVehicleModel.id = Model.data.id;
            fullVehicleModel.type = Model.data.type;
            fullVehicleModel.name = Model.data.attributes.name;
            fullVehicleModel.year = Model.data.attributes.year;
            fullVehicleModel.vehicle_make = Model.data.attributes.vehicle_make;

            return fullVehicleModel;
        }
    }
}
