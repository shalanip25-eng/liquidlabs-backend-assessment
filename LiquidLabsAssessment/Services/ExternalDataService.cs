using LiquidLabsAssessment.Models;
using LiquidLabsAssessment.Repositories;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiquidLabsAssessment.Services
{
    public class ExternalDataService : IExternalDataService
    {
        private readonly IExternalDataRepository _repository;
        private readonly HttpClient _httpClient;

        private const string ApiPath = "subjects/programming.json?limit=10";

        public ExternalDataService(IExternalDataRepository repository, HttpClient httpClient)
        {
            _repository = repository;
            _httpClient = httpClient;
        }

        private class OpenLibraryResponseDto
        {
            public List<BookWorkDto> Works { get; set; } = new();
        }

        private class BookWorkDto
        {
            public int Cover_Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public List<AuthorDto> Authors { get; set; } = new();
            public List<string> Subject { get; set; } = new();
        }

        private class AuthorDto
        {
            public string Name { get; set; } = string.Empty;
        }

        public async Task<IEnumerable<ExternalDataRecord>> GetRecordsAsync()
        {
            var localRecords = await _repository.GetAllAsync();
            if (localRecords.Any())
            {
                return localRecords;
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var openLibraryData = await _httpClient.GetFromJsonAsync<OpenLibraryResponseDto>(ApiPath, options);

                if (openLibraryData == null || !openLibraryData.Works.Any())
                    return Enumerable.Empty<ExternalDataRecord>();

                var mappedRecords = openLibraryData.Works.Select((work, index) => new ExternalDataRecord
                {
                    ExternalId = work.Cover_Id > 0 ? work.Cover_Id : (1000 + index),
                    UserId = index + 1,
                    Title = work.Title,
                    Body = work.Subject.Any() ? string.Join(", ", work.Subject) : "Programming, Software Engineering"
                }).ToList();

                foreach (var record in mappedRecords)
                {
                    await _repository.SaveAsync(record);
                }

                return mappedRecords;
            }
            catch (HttpRequestException)
            {
                return Enumerable.Empty<ExternalDataRecord>();
            }
        }

        public async Task<ExternalDataRecord?> GetRecordByIdAsync(int id)
        {
            var localRecord = await _repository.GetByExternalIdAsync(id);
            if (localRecord != null)
            {
                return localRecord;
            }

            var recordsPool = await GetRecordsAsync();
            return recordsPool.FirstOrDefault(r => r.ExternalId == id);
        }
    }
}