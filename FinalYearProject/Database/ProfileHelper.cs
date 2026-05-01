using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Database
{
    public class ProfileHelper
    {
        public static async Task<Profile> LoadProfile()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<Profile>();
            List<Profile> data = await db.GetAllAsync<Profile>();
            List<Profile> profiles = data.ToList();

            Profile profile = profiles.LastOrDefault();
            return profile;
        }

        public static async Task<List<Profile>> LoadAllProfiles()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<Profile>();
            List<Profile> data = await db.GetAllAsync<Profile>();
            List<Profile> profiles = data.ToList();
            return profiles;
        }

        public async Task<bool> InsertProfileToDatabase(Profile profile)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Profile>();
                int intResult = await db.InsertAsync(profile);
                bool result = false;
                if (intResult > 0)
                {
                    result = true;
                }

                if (profile.VehicleList.Count > 0)
                {
                    foreach (SavedVehicle vehicle in profile.VehicleList)
                    {
                        result = await InsertSavedVehicleToDatabase(vehicle);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> UpdateProfile(Profile profile)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<Profile>();
                string sql = @"
                            INSERT OR REPLACE INTO Profile (
                                Id, Username, Email, JoinDate, LastAccessDate, 
                                TotalC02, MonthC02, XP, MonthXP, Level, Streak, 
                                IsOrganisationMember, OrganisationID, OrganisationName, 
                                OrganisationTeamID, OrganisationTeamName, 
                                DateOfBirth, Age, FirstName, LastName, 
                                PhoneNumber, Country, Region, Area, 
                                City, Postcode, HouseholdSize, HomeType, 
                                HeatingType, ExtraInsulation, SolarInstalled, 
                                VehicleOwnership, PreferredCommuteType, DietaryPreference
                            ) VALUES (
                                ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 
                                ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?
                            ) WHERE Id = ?";

                int result = await db.ExecuteAsync(sql,
                    profile.Id, profile.Username, profile.Email, profile.JoinDate, profile.LastAccessDate,
                    profile.TotalC02, profile.MonthC02, profile.XP, profile.MonthXP,
                    profile.Level, profile.Streak,
                    profile.IsOrganisationMember, profile.OrganisationID, profile.OrganisationName,
                    profile.OrganisationTeamID, profile.OrganisationTeamName,
                    profile.DateOfBirth, profile.Age, profile.FirstName, profile.LastName,
                    profile.PhoneNumber, profile.Country, profile.Region, profile.Area,
                    profile.City, profile.Postcode, profile.HouseholdSize, profile.HomeType,
                    profile.HeatingType, profile.ExtraInsulation, profile.SolarInstalled,
                    profile.VehicleOwnership, profile.PreferredCommuteType, profile.DietaryPreference, profile.Id
                );

                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public static async Task<List<SavedVehicle>> LoadSavedVehicles()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<SavedVehicle>();
            List<SavedVehicle> data = await db.GetAllAsync<SavedVehicle>();
            List<SavedVehicle> savedVehicles = data.ToList();

            return savedVehicles;
        }

        public async Task<bool> InsertSavedVehicleToDatabase(SavedVehicle savedVehicle)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<SavedVehicle>();
                int intResult = await db.InsertAsync(savedVehicle);
                bool result = false;
                if (intResult > 0)
                {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> DeleteAllSavedVehicles()
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.DeleteAllAsync<SavedVehicle>();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting all SavedVehicles: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAllProfiles()
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.DeleteAllAsync<Profile>();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting all Profiles: {ex.Message}");
                return false;
            }
        }

        public static async Task<List<LevelLookup>> LoadLevels()
        {
            var db = new DatabaseMain();
            await db.Init();
            await db.CreateTableAsync<LevelLookup>();
            List<LevelLookup> levels = await db.GetAllAsync<LevelLookup>();
            return levels;
        }

        public async Task<bool> InsertLevelToDatabase(LevelLookup levelLookup)
        {
            try
            {
                var db = new DatabaseMain();
                await db.Init();
                await db.CreateTableAsync<LevelLookup>();
                int intResult = await db.InsertAsync(levelLookup);
                bool result = false;
                if (intResult > 0)
                {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }
    }
}
