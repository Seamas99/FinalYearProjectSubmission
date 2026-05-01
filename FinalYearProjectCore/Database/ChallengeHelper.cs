using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProjectCore.Database
{
    public class ChallengeHelper
    {
        public async Task<bool> InsertChallengeToDatabase(Challenge challenge)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Challenge>();

                int result = await db.InsertAsync(challenge);

                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<Challenge?> GetChallengeById(string id)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Challenge>();

                return await db.Table<Challenge>()
                               .Where(c => c.Id == id)
                               .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }


        public async Task<bool> UpdateChallenge(Challenge challenge)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Challenge>();

                int result = await db.UpdateAsync(challenge);

                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }


        public async Task<List<Challenge>> GetAllChallenges()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<Challenge>();

            return await db.Table<Challenge>().ToListAsync();
        }
    }
}
