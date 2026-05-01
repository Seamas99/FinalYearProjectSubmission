using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Database
{
    public class CountryHelper
    {
        public static async Task<IEnumerable<Country>> LoadAllCountries()
        {
            var db = new DatabaseMain();
            db.Init();
            await db.CreateTableAsync<Country>();
            IEnumerable<Country> data = await db.GetAllAsync<Country>();

            return data;
        }

        public static async Task<IEnumerable<Subdivision>> LoadCountrySubdivisions(string alpha2)
        {
            var db = new DatabaseMain();
            db.Init();
            await db.CreateTableAsync<Subdivision>();
            IEnumerable<Subdivision> data = await db.QueryEqualsAsync<Subdivision>("country", alpha2);

            return data;
        }

        public async Task<bool> CreateNewCountry(Country country)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.InsertAsync(country);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> CreateNewSubdivision(Subdivision subdivision)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.InsertAsync(subdivision);

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
