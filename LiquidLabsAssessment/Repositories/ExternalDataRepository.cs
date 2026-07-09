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