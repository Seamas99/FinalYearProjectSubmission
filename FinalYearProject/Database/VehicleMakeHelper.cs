using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Database
{
    public class VehicleMakeHelper
    {
        private static readonly string SelectVehicleMakes = "SELECT vehicle_make_id " +
            "make_name" +
            "number_of_models" +
            "FROM vehicle_make_attributes";
        private static readonly string InsertVehicleMake = "INSERT INTO vehicle_makes(id) VALUES (@MakeID)";
        private static readonly string InsertVehicleMakeAttributes = "INSERT INTO vehicle_make_attributes(vehicle_make_id, make_name, number_of_models) VALUES (@MakeID, @MakeName, @NumberOfModels)";
        
        public static async Task<IEnumerable<FullVehicleMake>> LoadAllFullVehicleMakes()
        {
            var db = new DatabaseMain();
            db.Init();
            await db.CreateTableAsync<FullVehicleMake>();
            IEnumerable<FullVehicleMake> data = await db.GetAllAsync<FullVehicleMake>();

            return data;
        }

        public async Task<bool> CreateNewVehicleMake(FullVehicleMake Make)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.InsertAsync(Make);

                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> ClearVehicleMake(FullVehicleMake Make)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.DeleteAsync<FullVehicleMake>(Make);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public static FullVehicleMake ReturnFullVehicleMake(VehicleMake Make)
        {
            FullVehicleMake fullVehicleMake = new FullVehicleMake();
            fullVehicleMake.id = Make.data.id;
            fullVehicleMake.type = Make.data.type;
            fullVehicleMake.name = Make.data.attributes.name;
            fullVehicleMake.number_of_models = Make.data.attributes.number_of_models;

            return fullVehicleMake;
        }
    }
}
