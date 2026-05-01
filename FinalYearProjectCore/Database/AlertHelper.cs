using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Database
{
    public class AlertHelper
    {
        public async Task<bool> InsertAlertToDatabase(Alert alert)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Alert>();

                int result = await db.InsertAsync(alert);

                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<Alert?> GetAlertById(string id)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Alert>();

                return await db.Table<Alert>()
                               .Where(c => c.alertID == id)
                               .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }


        public async Task<bool> UpdateAlert(Alert alert)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Alert>();

                int result = await db.UpdateAsync(alert);

                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }


        public async Task<List<Alert>> GetAllAlerts()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<Alert>();

            return await db.Table<Alert>().ToListAsync();
        }
    }
}
