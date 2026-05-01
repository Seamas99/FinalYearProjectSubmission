using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Database
{
    public class FootprintHelper
    {
        public static async Task<List<CarbonFootprint>> LoadFootprints()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<CarbonFootprint>();
            List<CarbonFootprint> data = await db.GetAllAsync<CarbonFootprint>();

            return data;
        }

        public async Task<bool> InsertFootprintToDatabase(CarbonFootprint footprint)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<CarbonFootprint>();
                await db.InsertAsync(footprint);


                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> UpdateCarbonFootprint(CarbonFootprint footprint)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                int rowsAffected = await db.UpdateAsync(footprint);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating CarbonFootprint: {ex.Message}");
                return false;
            }
        }
    }
}

