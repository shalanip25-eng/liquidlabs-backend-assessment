using LiquidLabsAssessment.Models;
using LiquidLabsAssessment.Repositories;
using System.Text.Json;

namespace LiquidLabsAssessment.Services
{
    public class ExternalDataService : IExternalDataService
    {
        private readonly IExternalDataRepository _repository;
        private readonly HttpClient _httpClient;

        private const string ApiUrl = "https://jsonplaceholder.typicode.com/posts";

        public ExternalDataService(IExternalDataRepository repository, HttpClient httpClient)
        {
            _repository = repository;
            _httpClient = httpClient;
        }

        private class JsonPlaceholderPostDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
        }

        public async Task<IEnumerable<ExternalDataRecord>> GetRecordsAsync()
        {
            var localRecords = await _repository.GetAllAsync();
            if (localRecords.Any())
            {
                return localRecords;
            }

            var response = await _httpClient.GetAsync(ApiUrl);
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<ExternalDataRecord>();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var externalData = JsonSerializer.Deserialize<List<JsonPlaceholderPostDto>>(content, options) ?? new List<JsonPlaceholderPostDto>();

            var mappedRecords = externalData.Select(dto => new ExternalDataRecord
            {
                ExternalId = dto.Id,
                UserId = dto.UserId,
                Title = dto.Title,
                Body = dto.Body
            }).ToList();

            foreach (var record in mappedRecords)
            {
                await _repository.SaveAsync(record);
            }

            return mappedRecords;
        }

        public async Task<ExternalDataRecord?> GetRecordByIdAsync(int id)
        {
            var localRecord = await _repository.GetByExternalIdAsync(id);
            if (localRecord != null)
            {
                return localRecord;
            }

            var response = await _httpClient.GetAsync($"{ApiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<JsonPlaceholderPostDto>(content, options);

            if (dto == null) return null;

            var record = new ExternalDataRecord
            {
                ExternalId = dto.Id,
                UserId = dto.UserId,
                Title = dto.Title,
                Body = dto.Body
            };

            await _repository.SaveAsync(record);
            return record;
        }
    }
}