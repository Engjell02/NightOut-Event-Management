using System.Text.Json.Serialization;

namespace Reservation_Management_App.Service.DTOs
{
    public class ExternalEventsResponseDto
    {
        [JsonPropertyName("events")]
        public List<ExternalEventDto> Events { get; set; } = new();
    }

    public class ExternalEventDto
    {
        [JsonPropertyName("eventCode")]
        public string EventCode { get; set; } = string.Empty;

        [JsonPropertyName("dj")]
        public ExternalDjDto? Dj { get; set; }

        [JsonPropertyName("mainAct")]
        public ExternalMainActDto? MainAct { get; set; }

        [JsonPropertyName("venue")]
        public ExternalVenueDto? Venue { get; set; }
            
        [JsonPropertyName("schedule")]
        public ExternalScheduleDto? Schedule { get; set; }
    }

    public class ExternalDjDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }

    public class ExternalMainActDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }

    public class ExternalVenueDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("baseFee")]
        public decimal BaseFee { get; set; }
    }

    public class ExternalScheduleDto
    {
        [JsonPropertyName("doorsOpen")]
        public string DoorsOpen { get; set; } = string.Empty;

        [JsonPropertyName("djStart")]
        public string DjStart { get; set; } = string.Empty;

        [JsonPropertyName("mainActStart")]
        public string MainActStart { get; set; } = string.Empty;
    }
}
