using Xunit;
using Moq;
using FluentAssertions;
using Reservation_Management_App.Service.Implementation;
using Reservation_Management_App.Repository;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Tests.TestUtilities;
using System.Linq.Expressions;

namespace Reservation_Management_App.Tests.UnitTests.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IRepository<Event>> _mockEventRepo;
        private readonly EventService _service;
        private readonly TestDataGenerator _testData;

        public EventServiceTests()
        {
            _mockEventRepo = new Mock<IRepository<Event>>();
            _service = new EventService(_mockEventRepo.Object);
            _testData = new TestDataGenerator();
        }

        [Fact]
        public void GetAll_ShouldReturnAllEvents()
        {
            // Arrange
            var events = _testData.GenerateEvents(5);
            _mockEventRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(events);

            // Act
            var result = _service.GetAll();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }

        [Fact]
        public void GetAll_ShouldOrderByStartDateTime()
        {
            // Arrange
            var event1 = _testData.GenerateEvent();
            event1.StartDateTime = DateTime.UtcNow.AddDays(10);

            var event2 = _testData.GenerateEvent();
            event2.StartDateTime = DateTime.UtcNow.AddDays(5);

            var event3 = _testData.GenerateEvent();
            event3.StartDateTime = DateTime.UtcNow.AddDays(15);

            var events = new List<Event> { event1, event2, event3 }
                .OrderBy(e => e.StartDateTime)
                .ToList();

            _mockEventRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(events);

            // Act
            var result = _service.GetAll().ToList();

            // Assert
            result.Should().HaveCount(3);
            result[0].StartDateTime.Should().BeBefore(result[1].StartDateTime);
            result[1].StartDateTime.Should().BeBefore(result[2].StartDateTime);
        }

        [Fact]
        public void GetUpcoming_ShouldReturnOnlyFutureEvents()
        {
            // Arrange
            var futureEvent1 = _testData.GenerateEvent();
            futureEvent1.StartDateTime = DateTime.UtcNow.AddDays(5);

            var futureEvent2 = _testData.GenerateEvent();
            futureEvent2.StartDateTime = DateTime.UtcNow.AddDays(10);

            var upcomingEvents = new List<Event> { futureEvent1, futureEvent2 };

            _mockEventRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(upcomingEvents);

            // Act
            var result = _service.GetUpcoming();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.StartDateTime >= DateTime.UtcNow);
        }

        [Fact]
        public void GetUpcoming_ShouldExcludePastEvents()
        {
            // Arrange
            var futureEvent = _testData.GenerateEvent();
            futureEvent.StartDateTime = DateTime.UtcNow.AddDays(5);

            var upcomingEvents = new List<Event> { futureEvent };

            _mockEventRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(upcomingEvents);

            // Act
            var result = _service.GetUpcoming();

            // Assert
            result.Should().NotContain(e => e.StartDateTime < DateTime.UtcNow);
        }

        [Fact]
        public void GetById_WithValidId_ShouldReturnEvent()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            var events = new List<Event> { eventObj };

            _mockEventRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(events);

            // Act
            var result = _service.GetById(eventObj.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(eventObj.Id);
            result.Title.Should().Be(eventObj.Title);
        }

        [Fact]
        public void GetById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            var emptyList = new List<Event>();

            _mockEventRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(emptyList);

            // Act
            var result = _service.GetById(invalidId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Create_ShouldGenerateNewIdAndInsert()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            var originalId = eventObj.Id;

            _mockEventRepo.Setup(r => r.Insert(It.IsAny<Event>()))
                .Returns<Event>(e => e);

            // Act
            var result = _service.Create(eventObj);

            // Assert
            result.Id.Should().NotBe(Guid.Empty);
            result.Id.Should().NotBe(originalId);
            _mockEventRepo.Verify(r => r.Insert(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public void Create_ShouldPreserveEventData()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            var title = eventObj.Title;
            var price = eventObj.PricePerPerson;

            _mockEventRepo.Setup(r => r.Insert(It.IsAny<Event>()))
                .Returns<Event>(e => e);

            // Act
            var result = _service.Create(eventObj);

            // Assert
            result.Title.Should().Be(title);
            result.PricePerPerson.Should().Be(price);
        }

        [Fact]
        public void Update_ShouldUpdateEvent()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            _mockEventRepo.Setup(r => r.Update(eventObj))
                .Returns(eventObj);

            // Act
            var result = _service.Update(eventObj);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(eventObj);
            _mockEventRepo.Verify(r => r.Update(eventObj), Times.Once);
        }

        [Fact]
        public void Delete_WithExistingEvent_ShouldRemoveEvent()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            _mockEventRepo.Setup(r => r.Get(eventObj.Id))
                .Returns(eventObj);
            _mockEventRepo.Setup(r => r.Delete(eventObj))
                .Returns(eventObj);

            // Act
            var result = _service.Delete(eventObj.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(eventObj);
            _mockEventRepo.Verify(r => r.Delete(eventObj), Times.Once);
        }

        [Fact]
        public void Delete_WithNonExistentEvent_ShouldThrowException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _mockEventRepo.Setup(r => r.Get(invalidId))
                .Returns((Event?)null);

            // Act & Assert
            var act = () => _service.Delete(invalidId);

            act.Should().Throw<Exception>().WithMessage("Event not found.");
        }

        [Theory]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(2500)]
        public void Create_WithDifferentPrices_ShouldCreateEvent(decimal price)
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            eventObj.PricePerPerson = price;

            _mockEventRepo.Setup(r => r.Insert(It.IsAny<Event>()))
                .Returns<Event>(e => e);

            // Act
            var result = _service.Create(eventObj);

            // Assert
            result.PricePerPerson.Should().Be(price);
            _mockEventRepo.Verify(r => r.Insert(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public void Create_WithAllRelationships_ShouldIncludeLocationAndPerformers()
        {
            // Arrange
            var location = _testData.GenerateLocation();
            var mainAct = _testData.GeneratePerformer();
            var dj = _testData.GeneratePerformer();
            var eventObj = _testData.GenerateEvent(location, mainAct, dj);

            _mockEventRepo.Setup(r => r.Insert(It.IsAny<Event>()))
                .Returns<Event>(e => e);

            // Act
            var result = _service.Create(eventObj);

            // Assert
            result.Location.Should().NotBeNull();
            result.MainAct.Should().NotBeNull();
            result.Dj.Should().NotBeNull();
            result.LocationId.Should().Be(location.Id);
        }
    }
}