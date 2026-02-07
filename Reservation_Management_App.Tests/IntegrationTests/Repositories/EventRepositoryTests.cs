using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reservation_Management_App.Repository;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Tests.TestUtilities;

namespace Reservation_Management_App.Tests.IntegrationTests.Repositories
{
    public class EventRepositoryTests : IDisposable
    {
        private readonly DatabaseFixture _fixture;
        private readonly Repository<Event> _repository;
        private readonly TestDataGenerator _testData;

        public EventRepositoryTests()
        {
            _fixture = new DatabaseFixture();
            _repository = new Repository<Event>(_fixture.Context);
            _testData = new TestDataGenerator();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void Insert_ShouldAddEventToDatabase()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();

            // Act
            var result = _repository.Insert(eventObj);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(eventObj.Id);

            var savedEvent = _fixture.Context.Events.Find(eventObj.Id);
            savedEvent.Should().NotBeNull();
            savedEvent!.Title.Should().Be(eventObj.Title);
        }

        [Fact]
        public void Get_WithValidId_ShouldReturnEvent()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            _repository.Insert(eventObj);

            // Act
            var result = _repository.Get(eventObj.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(eventObj.Id);
            result.Title.Should().Be(eventObj.Title);
        }

        [Fact]
        public void GetAll_ShouldReturnAllEvents()
        {
            // Arrange
            _fixture.ResetDatabase();
            var events = _testData.GenerateEvents(3);
            foreach (var evt in events)
            {
                _repository.Insert(evt);
            }

            // Act
            var result = _repository.GetAll();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public void Update_ShouldModifyExistingEvent()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            _repository.Insert(eventObj);

            // Act
            eventObj.Title = "Updated Event Title";
            eventObj.PricePerPerson = 999;
            var result = _repository.Update(eventObj);

            // Assert
            result.Title.Should().Be("Updated Event Title");
            result.PricePerPerson.Should().Be(999);

            var updated = _repository.Get(eventObj.Id);
            updated!.Title.Should().Be("Updated Event Title");
        }

        [Fact]
        public void Delete_ShouldRemoveEvent()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            _repository.Insert(eventObj);

            // Act
            _repository.Delete(eventObj);

            // Assert
            var deleted = _repository.Get(eventObj.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public void GetAll_WithLocationFilter_ShouldReturnMatchingEvents()
        {
            // Arrange
            _fixture.ResetDatabase();
            var location1 = _testData.GenerateLocation();
            var location2 = _testData.GenerateLocation();

            var event1 = _testData.GenerateEvent(location1);
            var event2 = _testData.GenerateEvent(location1);
            var event3 = _testData.GenerateEvent(location2);

            _repository.Insert(event1);
            _repository.Insert(event2);
            _repository.Insert(event3);

            // Act
            var result = _repository.GetAll(e => e.LocationId == location1.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.LocationId == location1.Id);
        }

        [Fact]
        public void GetAll_WithDateFilter_ShouldReturnUpcomingEvents()
        {
            // Arrange
            _fixture.ResetDatabase();
            var futureEvent = _testData.GenerateEvent();
            futureEvent.StartDateTime = DateTime.UtcNow.AddDays(10);

            var pastEvent = _testData.GenerateEvent();
            pastEvent.StartDateTime = DateTime.UtcNow.AddDays(-10);

            _repository.Insert(futureEvent);
            _repository.Insert(pastEvent);

            // Act
            var result = _repository.GetAll(e => e.StartDateTime > DateTime.UtcNow);

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(e => e.Id == futureEvent.Id);
        }

        [Fact]
        public void Insert_EventWithRelationships_ShouldPersistAllData()
        {
            // Arrange
            var location = _testData.GenerateLocation();
            var mainAct = _testData.GeneratePerformer();
            var dj = _testData.GeneratePerformer();
            var eventObj = _testData.GenerateEvent(location, mainAct, dj);

            // Act
            _repository.Insert(eventObj);

            // Assert
            var saved = _repository.Get(eventObj.Id);
            saved.Should().NotBeNull();
            saved!.LocationId.Should().Be(location.Id);
            saved.MainActId.Should().Be(mainAct.Id);
            saved.DjId.Should().Be(dj.Id);
        }

        [Fact]
        public void GetAll_WithPriceFilter_ShouldReturnMatchingEvents()
        {
            // Arrange
            _fixture.ResetDatabase();
            var expensiveEvent = _testData.GenerateEvent();
            expensiveEvent.PricePerPerson = 2000;

            var cheapEvent = _testData.GenerateEvent();
            cheapEvent.PricePerPerson = 500;

            _repository.Insert(expensiveEvent);
            _repository.Insert(cheapEvent);

            // Act
            var result = _repository.GetAll(e => e.PricePerPerson > 1000);

            // Assert
            result.Should().HaveCount(1);
            result.First().PricePerPerson.Should().Be(2000);
        }

        [Fact]
        public void Update_EventPrice_ShouldPersistChange()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            eventObj.PricePerPerson = 500;
            _repository.Insert(eventObj);

            // Act
            eventObj.PricePerPerson = 750;
            _repository.Update(eventObj);

            // Assert
            var updated = _repository.Get(eventObj.Id);
            updated!.PricePerPerson.Should().Be(750);
        }

        [Fact]
        public void GetAll_OrderedByDate_ShouldReturnInCorrectOrder()
        {
            // Arrange
            _fixture.ResetDatabase();
            var event1 = _testData.GenerateEvent();
            event1.StartDateTime = DateTime.UtcNow.AddDays(5);

            var event2 = _testData.GenerateEvent();
            event2.StartDateTime = DateTime.UtcNow.AddDays(1);

            var event3 = _testData.GenerateEvent();
            event3.StartDateTime = DateTime.UtcNow.AddDays(10);

            _repository.Insert(event1);
            _repository.Insert(event2);
            _repository.Insert(event3);

            // Act
            var result = _repository.GetAll(null, q => q.OrderBy(e => e.StartDateTime));

            // Assert
            result.Should().HaveCount(3);
            var list = result.ToList();
            list[0].StartDateTime.Should().BeBefore(list[1].StartDateTime);
            list[1].StartDateTime.Should().BeBefore(list[2].StartDateTime);
        }
    }
}