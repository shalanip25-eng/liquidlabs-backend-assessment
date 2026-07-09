namespace LiquidLabsAssessment.Models
{
    public class ExternalDataRecord
    {
        public int ExternalId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}