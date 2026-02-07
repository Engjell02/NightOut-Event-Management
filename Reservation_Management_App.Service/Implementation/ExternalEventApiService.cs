using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Reservation_Management_App.Service.DTOs;
using Reservation_Management_App.Service.Interface;

namespace Reservation_Management_App.Service.Implementation
{
    public class ExternalEventApiService : IExternalEventApiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://raw.githubusercontent.com/Engjell02/reservation-app-api/main/events.json";

        public ExternalEventApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ExternalEventDto?> GetByEventCodeAsync(string eventCode)
        {
            if (string.IsNullOrWhiteSpace(eventCode)) return null;

            var response = await _httpClient.GetFromJsonAsync<ExternalEventsResponseDto>(ApiUrl);
            return response?.Events
                .FirstOrDefault(e => e.EventCode.Equals(eventCode, StringComparison.OrdinalIgnoreCase));
        }

        // NEW METHOD
        public async Task<List<ExternalEventDto>> GetAllEventsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ExternalEventsResponseDto>(ApiUrl);
                return response?.Events?.ToList() ?? new List<ExternalEventDto>();
            }
            catch
            {
                return new List<ExternalEventDto>();
            }
        }
    }
}