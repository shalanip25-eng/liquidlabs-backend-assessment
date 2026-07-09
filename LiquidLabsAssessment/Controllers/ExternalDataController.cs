using LiquidLabsAssessment.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiquidLabsAssessment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalDataController : ControllerBase
    {
        private readonly IExternalDataService _dataService;
        private readonly ILogger<ExternalDataController> _logger;

        public ExternalDataController(IExternalDataService dataService, ILogger<ExternalDataController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _dataService.GetRecordsAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching external data records.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var record = await _dataService.GetRecordByIdAsync(id);
                if (record == null)
                {
                    return NotFound($"Record with external ID {id} was not found.");
                }
                return Ok(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching record for ID {id}.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}