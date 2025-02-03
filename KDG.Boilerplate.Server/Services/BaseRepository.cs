using Npgsql;
using System.Data;
using Microsoft.Data.SqlClient;

namespace KDG.Boilerplate.Server.Services
{
    public class BaseRepository : IBaseRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public BaseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            var tempConnectionString = _configuration["ConnectionStrings:DefaultConnection"];

            if (string.IsNullOrEmpty(tempConnectionString))
                throw new ArgumentNullException("Database connection string is null or empty.");

            _connectionString = tempConnectionString;
        }

        // use for buffered queries that return a type
        public async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                await using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception (not a timeout)", GetType().FullName), ex);
            }
        }

        public async Task<T> WithTransaction<T>(Func<IDbTransaction, Task<T>> getData)
        {
            try
            {
                await using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var result = await getData(transaction);
                            transaction.Commit();
                            return result;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception(String.Format("{0}.WithTransaction() experienced a SQL exception (not a timeout)", GetType().FullName), ex);
                        }
                    }
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithTransaction() experienced a SQL timeout", GetType().FullName), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithTransaction() experienced a SQL exception (not a timeout)", GetType().FullName), ex);
            }
        }

        // use for buffered queries that do not return a type
        public async Task WithConnection(Func<IDbConnection, Task> getData)
        {
            try
            {
                await using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception (not a timeout)", GetType().FullName), ex);
            }
        }

        public async Task WithTransaction(Func<IDbTransaction, Task> getData)
        {
            try
            {
                await using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            await getData(transaction);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception(String.Format("{0}.WithTransaction() experienced a SQL exception (not a timeout)", GetType().FullName), ex);
                        }
                    }
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception (not a timeout)", GetType().FullName), ex);
            }
        }

        //use for non-buffered queries that return a type
        public async Task<TResult> WithConnection<TRead, TResult>(Func<IDbConnection, Task<TRead>> getData, Func<TRead, Task<TResult>> process)
        {
            try
            {
                await using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var data = await getData(connection);
                    return await process(data);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception (not a timeout)", GetType().FullName), ex);
            }
        }
    }
}
