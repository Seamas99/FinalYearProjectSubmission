using Microsoft.Maui.Storage;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Database
{
    public class DatabaseMain
    {
        //private static readonly string ConnectionString = $@"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "FinalYearProject.db")}; Pooling=True";

        #region DB Constants
        public const string DatabaseFilename = "FinalYearProject.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        SQLiteAsyncConnection database;
        #endregion

        public AsyncTableQuery<T> Table<T>() where T : new()
        {
            return database.Table<T>();
        }

        public async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DatabasePath, Flags);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, params object[] args)
        {
            //Ensure the database is initialized before running the query
            await Init();

            //ExecuteScalarAsync returns the first column of the first row
            return await database.ExecuteScalarAsync<T>(sql, args);
        }

        public async Task<int> DeleteAllAsync<T>() where T : new()
        {
            //Ensure the database and tables are initialized
            await Init();

            //Database.DeleteAllAsync<T> returns the number of rows removed
            return await database.DeleteAllAsync<T>();
        }

        public Task<int> ExecuteAsync(string sql, params object[] args)
        {
            return database.ExecuteAsync(sql, args);
        }

        public Task<List<T>> QueryAsync<T>(string sql, params object[] args) where T : new()
        {
            return database.QueryAsync<T>(sql, args);
        }

        public Task<List<T>> QueryEqualsAsync<T>(string propertyName, object value) where T : new()
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, propertyName);
            var constant = Expression.Constant(value);
            var body = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);

            return database.Table<T>().Where(lambda).ToListAsync();
        }

        public Task<List<T>> QueryNotEqualsAsync<T>(string propertyName, object value) where T : new()
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, propertyName);
            var constant = Expression.Constant(value);
            var body = Expression.NotEqual(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);

            return database.Table<T>().Where(lambda).ToListAsync();
        }

        public async Task CreateTableAsync<T>() where T : new()
        {
            await database.CreateTableAsync<T>();
        }

        public Task<int> InsertAsync<T>(T item) where T : new()
        {
            return database.InsertAsync(item);
        }

        public Task<int> UpdateAsync<T>(T item) where T : new()
        {
            return database.UpdateAsync(item);
        }

        public Task<int> DeleteAsync<T>(T item) where T : new()
        {
            return database.DeleteAsync(item);
        }

        public Task<List<T>> GetAllAsync<T>() where T : new()
        {
            return database.Table<T>().ToListAsync();

        }

        public Task<T> GetByIdAsync<T>(object pk) where T : new()
        {
            return database.FindAsync<T>(pk);
        }
    }
}
