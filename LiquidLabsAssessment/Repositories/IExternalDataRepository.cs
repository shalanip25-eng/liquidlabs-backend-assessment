using LiquidLabsAssessment.Models;

namespace LiquidLabsAssessment.Repositories
{
    public interface IExternalDataRepository
    {
        Task<IEnumerable<ExternalDataRecord>> GetAllAsync();
        Task<ExternalDataRecord?> GetByExternalIdAsync(int externalId);
        Task SaveAsync(ExternalDataRecord record);
    }
}