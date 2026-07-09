using LiquidLabsAssessment.Models;

namespace LiquidLabsAssessment.Services
{
    public interface IExternalDataService
    {
        Task<IEnumerable<ExternalDataRecord>> GetRecordsAsync();
        Task<ExternalDataRecord?> GetRecordByIdAsync(int id);
    }
}