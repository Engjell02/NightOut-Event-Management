using Bogus;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Domain.DomainModels.Enums;
using Reservation_Management_App.Domain.Identity;

namespace Reservation_Management_App.Tests.TestUtilities
{
    public class TestDataGenerator
    {
        private readonly Faker _faker;

        public TestDataGenerator()
        {
            _faker = new Faker();
        }

        // Generate Location
        public Location GenerateLocation()
        {
            return new Location
            {
                Id = Guid.NewGuid(),
                Name = _faker.Company.CompanyName(),
                City = _faker.Address.City(),
                Address = _faker.Address.StreetAddress(),
                PhoneNumber = _faker.Phone.PhoneNumber(),
                Type = _faker.PickRandom("Club", "Bar", "Concert Hall", "Lounge"),
                Capacity = _faker.Random.Int(100, 1000),
                ImageUrl = _faker.Image.PicsumUrl(),
                ImportedFromApi = false
            };
        }

        // Generate Performer
        public Performer GeneratePerformer()
        {
            return new Performer
            {
                Id = Guid.NewGuid(),
                StageName = _faker.Name.FullName(),
                Type = _faker.PickRandom("DJ", "Rock Band", "Pop Artist", "Electronic", "Jazz"),
                ImageUrl = _faker.Image.PicsumUrl(),
                ImportedFromApi = false
            };
        }

        // Generate Event
        public Event GenerateEvent(Location? location = null, Performer? mainAct = null, Performer? dj = null)
        {
            var loc = location ?? GenerateLocation();
            var main = mainAct ?? GeneratePerformer();
            var djPerformer = dj ?? GeneratePerformer();

            return new Event
            {
                Id = Guid.NewGuid(),
                Title = _faker.Lorem.Sentence(3),
                StartDateTime = _faker.Date.Future(),
                PricePerPerson = _faker.Random.Decimal(200, 2000),
                AvailableSpots = _faker.Random.Int(50, 500),
                PosterImageUrl = _faker.Image.PicsumUrl(),
                LocationId = loc.Id,
                Location = loc,
                MainActId = main.Id,
                MainAct = main,
                DjId = djPerformer.Id,
                Dj = djPerformer,
                ImportedFromApi = false
            };
        }

        // Generate Reservation
        public Reservation GenerateReservation(Event? eventObj = null, string? userId = null)
        {
            var evt = eventObj ?? GenerateEvent();

            return new Reservation
            {
                Id = Guid.NewGuid(),
                ReservationName = _faker.Name.FullName(),
                NumberOfPeople = _faker.Random.Int(2, 6),
                PhoneNumber = _faker.Phone.PhoneNumber(),
                EventId = evt.Id,
                Event = evt,
                UserId = userId ?? Guid.NewGuid().ToString(),
                Status = _faker.PickRandom<ReservationStatus>(),
                CreatedAt = DateTime.UtcNow
            };
        }

        // Generate User
        public Reservation_Management_AppUser GenerateUser()
        {
            return new Reservation_Management_AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = _faker.Internet.Email(),
                Email = _faker.Internet.Email(),
                Firstname = _faker.Name.FirstName(),
                Lastname = _faker.Name.LastName(),
                Phone = _faker.Phone.PhoneNumber(),
                EmailConfirmed = true
            };
        }

        // Generate Multiple Locations
        public List<Location> GenerateLocations(int count)
        {
            var locations = new List<Location>();
            for (int i = 0; i < count; i++)
            {
                locations.Add(GenerateLocation());
            }
            return locations;
        }

        // Generate Multiple Performers
        public List<Performer> GeneratePerformers(int count)
        {
            var performers = new List<Performer>();
            for (int i = 0; i < count; i++)
            {
                performers.Add(GeneratePerformer());
            }
            return performers;
        }

        // Generate Multiple Events
        public List<Event> GenerateEvents(int count)
        {
            var events = new List<Event>();
            for (int i = 0; i < count; i++)
            {
                events.Add(GenerateEvent());
            }
            return events;
        }

        // Generate Multiple Reservations
        public List<Reservation> GenerateReservations(int count)
        {
            var reservations = new List<Reservation>();
            for (int i = 0; i < count; i++)
            {
                reservations.Add(GenerateReservation());
            }
            return reservations;
        }

        // Generate Reservation with Specific Status
        public Reservation GenerateReservationWithStatus(ReservationStatus status, decimal eventPrice, int numberOfPeople)
        {
            var eventObj = GenerateEvent();
            eventObj.PricePerPerson = eventPrice;

            return new Reservation
            {
                Id = Guid.NewGuid(),
                ReservationName = _faker.Name.FullName(),
                NumberOfPeople = numberOfPeople,
                PhoneNumber = _faker.Phone.PhoneNumber(),
                EventId = eventObj.Id,
                Event = eventObj,
                UserId = Guid.NewGuid().ToString(),
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Generate Approved Reservation (for revenue testing)
        public Reservation GenerateApprovedReservation(decimal eventPrice, int numberOfPeople)
        {
            return GenerateReservationWithStatus(ReservationStatus.Approved, eventPrice, numberOfPeople);
        }

        // Generate Pending Reservation
        public Reservation GeneratePendingReservation()
        {
            return GenerateReservationWithStatus(ReservationStatus.Pending, 500, 3);
        }

        // Generate Rejected Reservation
        public Reservation GenerateRejectedReservation()
        {
            return GenerateReservationWithStatus(ReservationStatus.Rejected, 500, 3);
        }
    }
}