using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Database
{
    public class AirportHelper
    {
        public static async Task<IEnumerable<Airport>> LoadAllAirports()
        {
            var db = new DatabaseMain();
            db.Init();
            await db.CreateTableAsync<Airport>();
            IEnumerable<Airport> data = await db.GetAllAsync<Airport>();

            return data;
        }

        public async Task<bool> CreateNewAirport(Airport airport)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.InsertAsync(airport);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }
    }
}
