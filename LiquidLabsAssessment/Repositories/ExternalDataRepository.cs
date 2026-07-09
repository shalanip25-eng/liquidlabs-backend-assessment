using LiquidLabsAssessment.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LiquidLabsAssessment.Repositories
{
    public class ExternalDataRepository : IExternalDataRepository
    {
        private readonly string _connectionString;

        public ExternalDataRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        #region Common Execution Helper

        /// <summary>
        /// A helper method that manages the connection and command, 
        /// applies parameters safely, and opens/closes the database connection.
        /// </summary>
        private async Task<T> ExecuteCommandAsync<T>(
            string sql,
            Func<SqlCommand, Task<T>> executeAndMapFunc,
            Action<SqlParameterCollection>? addParams = null)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            addParams?.Invoke(command.Parameters);
            await connection.OpenAsync();
            return await executeAndMapFunc(command);
        }

        #endregion

        #region Repository Methods

        public async Task<IEnumerable<ExternalDataRecord>> GetAllAsync()
        {
            const string query = "SELECT ExternalId, UserId, Title, Body FROM ExternalData";

            return await ExecuteCommandAsync(query, async (command) =>
            {
                var records = new List<ExternalDataRecord>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    records.Add(MapToRecord(reader));
                }
                return records;
            });
        }

        public async Task<ExternalDataRecord?> GetByExternalIdAsync(int externalId)
        {
            const string query = "SELECT ExternalId, UserId, Title, Body FROM ExternalData WHERE ExternalId = @ExternalId";

            return await ExecuteCommandAsync(
                query,
                async (command) =>
                {
                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return MapToRecord(reader);
                    }
                    return null;
                },
                parameters => parameters.AddWithValue("@ExternalId", externalId)
            );
        }

        public async Task SaveAsync(ExternalDataRecord record)
        {
            const string query = @"
                IF NOT EXISTS (SELECT 1 FROM ExternalData WHERE ExternalId = @ExternalId)
                BEGIN
                    INSERT INTO ExternalData (ExternalId, UserId, Title, Body) 
                    VALUES (@ExternalId, @UserId, @Title, @Body)
                END";
            await ExecuteCommandAsync<bool>(
                query,
                async (command) =>
                {
                    await command.ExecuteNonQueryAsync();
                    return true;
                },
                parameters =>
                {
                    parameters.AddWithValue("@ExternalId", record.ExternalId);
                    parameters.AddWithValue("@UserId", record.UserId);
                    parameters.AddWithValue("@Title", record.Title);
                    parameters.AddWithValue("@Body", record.Body);
                }
            );
        }

        #endregion

        #region Mapping Helper

        private static ExternalDataRecord MapToRecord(SqlDataReader reader)
        {
            return new ExternalDataRecord
            {
                ExternalId = reader.GetInt32("ExternalId"),
                UserId = reader.GetInt32("UserId"),
                Title = reader.GetString("Title"),
                Body = reader.GetString("Body")
            };
        }

        #endregion
    }
}