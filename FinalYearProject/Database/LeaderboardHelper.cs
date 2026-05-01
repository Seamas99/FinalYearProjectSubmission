using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Database
{
    public class LeaderboardHelper
    {
        public static async Task<List<League>> LoadLeagues()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<League>();
            List<League> data = await db.GetAllAsync<League>();
            foreach(League league in data)
            {
                List<LeagueEntry> entries = await db.GetAllAsync<LeagueEntry>();
                             league.LeagueEntries = entries.Where(e => e.LeagueID == league.LeagueID)
                             .Where(e => e.EntryDate.Month == league.ProcessedDate.Month &&
                                         e.EntryDate.Year == league.ProcessedDate.Year).ToList();
            }

            return data;
        }

        public static async Task<List<LeagueEntry>> LoadLeagueEntries()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<LeagueEntry>();
            List<LeagueEntry> data = await db.GetAllAsync<LeagueEntry>();

            return data;
        }

        public static async Task<List<Position>> LoadPositions()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<Position>();
            List<Position> data = await db.GetAllAsync<Position>();

            return data;
        }

        public static async Task<List<CarbonFootprint>> LoadFootprints()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<CarbonFootprint>();
            List<CarbonFootprint> data = await db.GetAllAsync<CarbonFootprint>();

            return data;
        }

        public async Task<bool> InsertLeagueToDatabase(League league)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<League>();

                await db.InsertAsync(league);

                var entryInsertTasks = league.LeagueEntries.Select(entry => InsertLeagueEntryToDatabase(entry));
                await Task.WhenAll(entryInsertTasks);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> InsertLeagueEntryToDatabase(LeagueEntry entry)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<LeagueEntry>();
                await db.InsertAsync(entry);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        private async Task EnsureLeagueExistsAsync(League league)
        {
            var db = new DatabaseMain();
            await db.Init();

            // Pull leagues with matching ID
            var existingLeagues = await db.Table<League>()
                                          .Where(l => l.LeagueID == league.LeagueID)
                                          .ToListAsync();

            // Check if any match the month and year
            bool exists = existingLeagues.Any(l =>
                l.ProcessedDate.Month == league.ProcessedDate.Month &&
                l.ProcessedDate.Year == league.ProcessedDate.Year);

            if (!exists)
            {
                await InsertLeagueToDatabase(league);
            }
        }

        private async Task EnsureLeagueEntryExistsAsync(LeagueEntry entry)
        {
            var db = new DatabaseMain();
            await db.Init();

            string checkSql = @"SELECT COUNT(*) FROM LeagueEntry 
                        WHERE UserID = ?
                        AND LeagueID = ?
                        AND strftime('%m', EntryDate) = ? 
                        AND strftime('%Y', EntryDate) = ?";

            var count = await db.ExecuteScalarAsync<int>(checkSql,
                            entry.UserID,
                            entry.LeagueID,
                            entry.EntryDate.Month.ToString("D2"),
                            entry.EntryDate.Year.ToString());

            if (count == 0)
            {
                await InsertLeagueEntryToDatabase(entry);
            }
        }

        public async Task<bool> UpdateLeagueInDatabase(League league)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                DateTime date = new DateTime(league.ProcessedDate.Year, league.ProcessedDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime nextMonth = date.AddMonths(1);
                string deleteSql = "DELETE FROM League WHERE LeagueID = ? AND ProcessedDate >= ? AND ProcessedDate < ?";
                await db.ExecuteAsync(deleteSql, league.LeagueID, date, nextMonth);

                league.ProcessedDate = date;
                await db.InsertAsync(league);

                if (league.LeagueEntries != null)
                {
                    foreach (var entry in league.LeagueEntries)
                    {
                        await UpdateLeagueEntryInDatabase(entry);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"League Sync Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateLeagueEntryInDatabase(LeagueEntry entry)
        {
            var db = new DatabaseMain();
            await db.Init();

            DateTime date = new DateTime(entry.EntryDate.Year, entry.EntryDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime nextMonth = date.AddMonths(1);

            // Delete existing entry for this User in this League for this Month
            string deleteSql = "DELETE FROM LeagueEntry WHERE UserID = ? AND LeagueID = ? AND EntryDate >= ? AND EntryDate < ?";
            await db.ExecuteAsync(deleteSql, entry.UserID, entry.LeagueID, date, nextMonth);

            entry.EntryDate = date;
            await db.InsertAsync(entry);
            return true;
        }

        public async Task<bool> UpdateFootprintInDatabase(CarbonFootprint carbonFootprint)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<CarbonFootprint>();

                string sql = @"
                            INSERT OR REPLACE INTO CarbonFootprint (
                                Id, MeasureDate, Type, CarbonMeasurement, XP
                            ) VALUES (
                                ?, ?, ?, ?, ?
                            )";

                int result = await db.ExecuteAsync(sql,
                    carbonFootprint.MeasureDate, carbonFootprint.Type, carbonFootprint.CarbonMeasurement, carbonFootprint.XP,
                    carbonFootprint.Id // The WHERE clause
                );

                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Footprint Sync Error: {ex.Message}");
                return false;
            }
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

        public async Task<bool> InsertPositionToDatabase(Position position)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Position>();
                await db.InsertAsync(position);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> UpdatePositionInDatabase(Position position)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Position>();

                DateTime date = new DateTime(position.EntryDate.Year, position.EntryDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime nextMonth = date.AddMonths(1);

                // Clear existing rank for this month before inserting new rank
                string deleteSql = "DELETE FROM Position WHERE LeagueID = ? AND EntryDate >= ? AND EntryDate < ?";
                await db.ExecuteAsync(deleteSql, position.LeagueID, date, nextMonth);

                position.EntryDate = date;
                int result = await db.InsertAsync(position);
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Position Sync Error: {ex.Message}");
                return false;
            }
        }

        public async Task<Position> GetPositionForMonth(string leagueId, DateTime date)
        {
            var db = new DatabaseMain();
            await db.Init();

            string sql = @"SELECT * FROM Position 
                    WHERE LeagueID = ? 
                    AND strftime('%m', EntryDate) = ? 
                    AND strftime('%Y', EntryDate) = ? 
                    LIMIT 1";

            var results = await db.QueryAsync<Position>(sql,
                leagueId,
                date.Month.ToString("D2"),
                date.Year.ToString());

            return results.FirstOrDefault();
        }

        public async Task<bool> CheckPositionExists(string leagueId, DateTime date)
        {
            var db = new DatabaseMain();
            await db.Init();

            string sql = @"SELECT * FROM Position 
                    WHERE LeagueID = ? 
                    AND strftime('%m', EntryDate) = ? 
                    AND strftime('%Y', EntryDate) = ? 
                    LIMIT 1";

            var results = await db.QueryAsync<Position>(sql,
                leagueId,
                date.Month.ToString("D2"),
                date.Year.ToString());

            if (results.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteAllLeagueEntries()
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();

                await db.DeleteAllAsync<LeagueEntry>();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting all entries: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAllLeagues()
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.DeleteAllAsync<League>();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting all leagues: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAllPositions()
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.DeleteAllAsync<Position>();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting all positions: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAllFootprints()
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.DeleteAllAsync<CarbonFootprint>();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting all footprints: {ex.Message}");
                return false;
            }
        }
    }
}
