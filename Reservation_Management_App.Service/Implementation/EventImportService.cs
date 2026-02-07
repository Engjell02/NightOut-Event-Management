using System;
using System.Linq;
using System.Threading.Tasks;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Service.Interface;

namespace Reservation_Management_App.Service.Implementation
{
    public class EventImportService : IEventImportService
    {
        private readonly IExternalEventApiService _apiService;
        private readonly IEventService _eventService;
        private readonly ILocationService _locationService;
        private readonly IPerformerService _performerService;

        public EventImportService(
            IExternalEventApiService apiService,
            IEventService eventService,
            ILocationService locationService,
            IPerformerService performerService)
        {
            _apiService = apiService;
            _eventService = eventService;
            _locationService = locationService;
            _performerService = performerService;
        }

        public async Task<int> ImportEventsFromApiAsync()
        {
            try
            {
                var importedCount = 0;

                // Fetch ALL events dynamically from API
                var apiEvents = await _apiService.GetAllEventsAsync();

                if (apiEvents == null || !apiEvents.Any())
                    return 0;

                foreach (var apiEvent in apiEvents)
                {
                    // Check if already imported
                    var existingEvent = _eventService.GetAll()
                        .FirstOrDefault(e => e.ExternalEventCode == apiEvent.EventCode);

                    if (existingEvent != null)
                        continue; // Skip if already imported

                    // Create or find Location
                    var location = _locationService.GetAll()
                        .FirstOrDefault(l => l.Name == apiEvent.Venue?.Name);

                    if (location == null && apiEvent.Venue != null)
                    {
                        location = _locationService.Create(new Location
                        {
                            Id = Guid.NewGuid(),
                            Name = apiEvent.Venue.Name,
                            Address = apiEvent.Venue.Address,
                            PhoneNumber = apiEvent.Venue.PhoneNumber,
                            Type = DetermineVenueType(apiEvent.Venue.Name),
                            Capacity = 500,
                            City = "Skopje",
                            ImportedFromApi = true
                        });
                    }

                    // Create or find DJ
                    var dj = _performerService.GetAll()
                        .FirstOrDefault(p => p.StageName == apiEvent.Dj?.Name);

                    if (dj == null && apiEvent.Dj != null)
                    {
                        dj = _performerService.Create(new Performer
                        {
                            Id = Guid.NewGuid(),
                            StageName = apiEvent.Dj.Name,
                            Type = "DJ",
                            ImportedFromApi = true
                        });
                    }

                    // Create or find Main Act
                    var mainAct = _performerService.GetAll()
                        .FirstOrDefault(p => p.StageName == apiEvent.MainAct?.Name);

                    if (mainAct == null && apiEvent.MainAct != null)
                    {
                        var performerType = apiEvent.MainAct.Name.Contains("Band") ? "Band" : "Singer";

                        mainAct = _performerService.Create(new Performer
                        {
                            Id = Guid.NewGuid(),
                            StageName = apiEvent.MainAct.Name,
                            Type = performerType,
                            ImportedFromApi = true
                        });
                    }

                    // Get event details based on code
                    var eventDetails = GetEventDetailsFromCode(apiEvent.EventCode);

                    // Create Event
                    var newEvent = new Event
                    {
                        Id = Guid.NewGuid(),
                        Title = eventDetails.Title,
                        StartDateTime = eventDetails.StartDateTime,
                        PricePerPerson = eventDetails.Price,
                        AvailableSpots = 20,
                        PosterImageUrl = eventDetails.ImageUrl,
                        ExternalEventCode = apiEvent.EventCode,
                        ImportedFromApi = true,
                        LocationId = location?.Id ?? Guid.Empty,
                        DjId = dj?.Id,
                        MainActId = mainAct?.Id
                    };

                    _eventService.Create(newEvent);
                    importedCount++;
                }

                return importedCount;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private string DetermineVenueType(string venueName)
        {
            if (venueName.Contains("Club", StringComparison.OrdinalIgnoreCase))
                return "Club";
            if (venueName.Contains("Bar", StringComparison.OrdinalIgnoreCase))
                return "Bar";
            if (venueName.Contains("Lounge", StringComparison.OrdinalIgnoreCase))
                return "Lounge";
            if (venueName.Contains("Arena", StringComparison.OrdinalIgnoreCase) ||
                venueName.Contains("Hall", StringComparison.OrdinalIgnoreCase))
                return "Concert Hall";

            return "Venue";
        }

        private (string Title, DateTime StartDateTime, decimal Price, string ImageUrl) GetEventDetailsFromCode(string eventCode)
        {
            var eventNumber = eventCode.Replace("EVENT", "");

            return eventCode switch
            {
                "EVENT001" => ("Neon Nights", DateTime.Now.AddDays(7), 25m, "https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?w=800"),
                "EVENT002" => ("Rock Revolution", DateTime.Now.AddDays(10), 45m, "https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?w=800"),
                "EVENT003" => ("Velvet Sessions", DateTime.Now.AddDays(14), 30m, "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?w=800"),
                "EVENT004" => ("Midnight Madness", DateTime.Now.AddDays(17), 35m, "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=800"),
                _ => ($"Event {eventNumber}", DateTime.Now.AddDays(7 + int.Parse(eventNumber ?? "0") * 3), 25m, "https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?w=800")
            };
        }
    }
}