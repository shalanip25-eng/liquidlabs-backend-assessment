using Microsoft.Data.SqlClient;
using LiquidLabsAssessment.Models;
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

        public async Task<IEnumerable<ExternalDataRecord>> GetAllAsync()
        {
            var records = new List<ExternalDataRecord>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT ExternalId, UserId, Title, Body FROM ExternalData", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                records.Add(MapToRecord(reader));
            }
            return records;
        }

        public async Task<ExternalDataRecord?> GetByExternalIdAsync(int externalId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT ExternalId, UserId, Title, Body FROM ExternalData WHERE ExternalId = @ExternalId", connection);
            command.Parameters.AddWithValue("@ExternalId", externalId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToRecord(reader);
            }
            return null;
        }

        public async Task SaveAsync(ExternalDataRecord record)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"
                IF NOT EXISTS (SELECT 1 FROM ExternalData WHERE ExternalId = @ExternalId)
                BEGIN
                    INSERT INTO ExternalData (ExternalId, UserId, Title, Body) 
                    VALUES (@ExternalId, @UserId, @Title, @Body)
                END";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ExternalId", record.ExternalId);
            command.Parameters.AddWithValue("@UserId", record.UserId);
            command.Parameters.AddWithValue("@Title", record.Title);
            command.Parameters.AddWithValue("@Body", record.Body);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

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
    }
}