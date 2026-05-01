using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Database
{
    public class SettingsHelper
    {
        public static async Task<IEnumerable<Settings>> LoadSettings()
        {
            var db = new DatabaseMain();
            db.Init();
            try
            {
                await db.CreateTableAsync<Settings>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            IEnumerable<Settings> data = await db.GetAllAsync<Settings>();
            return data;
        }

        public async Task<bool> CreateSettings(Settings settings)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.InsertAsync(settings);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> UpdateSettings(Settings settings)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.UpdateAsync(settings);

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
