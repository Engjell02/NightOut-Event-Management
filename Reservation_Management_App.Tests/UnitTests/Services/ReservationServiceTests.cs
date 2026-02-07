using Xunit;
using Moq;
using FluentAssertions;
using Reservation_Management_App.Service.Implementation;
using Reservation_Management_App.Repository;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Domain.DomainModels.Enums;
using Reservation_Management_App.Tests.TestUtilities;
using System.Linq.Expressions;

namespace Reservation_Management_App.Tests.UnitTests.Services
{
    public class ReservationServiceTests
    {
        private readonly Mock<IRepository<Reservation>> _mockReservationRepo;
        private readonly Mock<IRepository<Event>> _mockEventRepo;
        private readonly ReservationService _service;
        private readonly TestDataGenerator _testData;

        public ReservationServiceTests()
        {
            _mockReservationRepo = new Mock<IRepository<Reservation>>();
            _mockEventRepo = new Mock<IRepository<Event>>();
            _service = new ReservationService(_mockReservationRepo.Object, _mockEventRepo.Object);
            _testData = new TestDataGenerator();
        }

        [Fact]
        public void GetAll_ShouldReturnAllReservations()
        {
            // Arrange
            var reservations = _testData.GenerateReservations(5);
            _mockReservationRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Reservation, bool>>>(),
                It.IsAny<Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>>>(),
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns(reservations);

            // Act
            var result = _service.GetAll();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }

        [Fact]
        public void GetByUser_ShouldReturnUserReservations()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userReservations = new List<Reservation>
            {
                _testData.GenerateReservation(userId: userId),
                _testData.GenerateReservation(userId: userId)
            };

            _mockReservationRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Reservation, bool>>>(),
                It.IsAny<Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>>>(),
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns(userReservations);

            // Act
            var result = _service.GetByUser(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(r => r.UserId == userId);
        }

        [Fact]
        public void CreateReservation_WithValidData_ShouldCreateReservation()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            eventObj.AvailableSpots = 10;
            var userId = Guid.NewGuid().ToString();

            _mockEventRepo.Setup(e => e.GetWithIncludes(
                eventObj.Id,
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(eventObj);

            // Act
            var result = _service.CreateReservation(
                eventObj.Id,
                userId,
                "John Doe",
                4,
                "+389 70 123 456"
            );

            // Assert
            result.Should().NotBeNull();
            result.ReservationName.Should().Be("John Doe");
            result.NumberOfPeople.Should().Be(4);
            result.Status.Should().Be(ReservationStatus.Pending);
            eventObj.AvailableSpots.Should().Be(9); // Decreased by 1
            _mockReservationRepo.Verify(r => r.Insert(It.IsAny<Reservation>()), Times.Once);
            _mockEventRepo.Verify(e => e.Update(eventObj), Times.Once);
        }

        [Fact]
        public void CreateReservation_WithNoAvailableSpots_ShouldThrowException()
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            eventObj.AvailableSpots = 0;

            _mockEventRepo.Setup(e => e.GetWithIncludes(
                eventObj.Id,
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(eventObj);

            // Act & Assert
            var act = () => _service.CreateReservation(
                eventObj.Id,
                Guid.NewGuid().ToString(),
                "John Doe",
                4,
                "+389 70 123 456"
            );

            act.Should().Throw<Exception>().WithMessage("No tables available for this event.");
        }

        [Theory]
        [InlineData(1)]  // Too few people
        [InlineData(7)]  // Too many people
        public void CreateReservation_WithInvalidGroupSize_ShouldThrowException(int numberOfPeople)
        {
            // Arrange
            var eventObj = _testData.GenerateEvent();
            eventObj.AvailableSpots = 10;

            _mockEventRepo.Setup(e => e.GetWithIncludes(
                eventObj.Id,
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns(eventObj);

            // Act & Assert
            var act = () => _service.CreateReservation(
                eventObj.Id,
                Guid.NewGuid().ToString(),
                "John Doe",
                numberOfPeople,
                "+389 70 123 456"
            );

            act.Should().Throw<Exception>().WithMessage("Group size must be between 2 and 6 people.");
        }

        [Fact]
        public void CreateReservation_WithNonExistentEvent_ShouldThrowException()
        {
            // Arrange
            _mockEventRepo.Setup(e => e.GetWithIncludes(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<Event, object>>[]>()
            )).Returns((Event?)null);

            // Act & Assert
            var act = () => _service.CreateReservation(
                Guid.NewGuid(),
                Guid.NewGuid().ToString(),
                "John Doe",
                4,
                "+389 70 123 456"
            );

            act.Should().Throw<Exception>().WithMessage("Event not found.");
        }

        [Fact]
        public void CancelReservation_MoreThan24HoursBeforeEvent_ShouldSucceed()
        {
            // Arrange
            var reservation = _testData.GenerateReservation();
            reservation.Event.StartDateTime = DateTime.UtcNow.AddHours(48);
            reservation.Status = ReservationStatus.Pending;

            _mockReservationRepo.Setup(r => r.GetWithIncludes(
                reservation.Id,
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns(reservation);

            // Act
            var result = _service.CancelReservation(reservation.Id, reservation.UserId);

            // Assert
            result.Should().BeTrue();
            reservation.Status.Should().Be(ReservationStatus.Cancelled);
            reservation.Event.AvailableSpots.Should().BeGreaterThan(0); 
            _mockReservationRepo.Verify(r => r.Update(reservation), Times.Once);
        }

        [Fact]
        public void CancelReservation_Within24Hours_ShouldReturnFalse()
        {
            // Arrange
            var reservation = _testData.GenerateReservation();
            reservation.Event.StartDateTime = DateTime.UtcNow.AddHours(12); 
            reservation.Status = ReservationStatus.Pending; 

            _mockReservationRepo.Setup(r => r.GetWithIncludes(
                reservation.Id,
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns(reservation);

            // Act
            var result = _service.CancelReservation(reservation.Id, reservation.UserId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CancelReservation_WrongUser_ShouldThrowException()
        {
            // Arrange
            var reservation = _testData.GenerateReservation();
            var wrongUserId = Guid.NewGuid().ToString();

            _mockReservationRepo.Setup(r => r.GetWithIncludes(
                reservation.Id,
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns(reservation);

            // Act & Assert
            var act = () => _service.CancelReservation(reservation.Id, wrongUserId);

            act.Should().Throw<Exception>()
                .WithMessage("Reservation not found or you don't have permission.");
        }

        [Fact]
        public void Approve_PendingReservation_ShouldChangeStatusToApproved()
        {
            // Arrange
            var reservation = _testData.GeneratePendingReservation();

            _mockReservationRepo.Setup(r => r.GetWithIncludes(
                reservation.Id,
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns(reservation);

            _mockReservationRepo.Setup(r => r.Update(It.IsAny<Reservation>()))
                .Returns(reservation);

            // Act
            var result = _service.Approve(reservation.Id);

            // Assert
            result.Status.Should().Be(ReservationStatus.Approved);
            _mockReservationRepo.Verify(r => r.Update(It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public void Approve_RejectedReservation_ShouldRestoreTableAndApprove()
        {
            // Arrange
            var reservation = _testData.GenerateRejectedReservation();
            var initialSpots = reservation.Event.AvailableSpots;

            _mockReservationRepo.Setup(r => r.GetWithIncludes(
                reservation.Id,
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns(reservation);

            _mockReservationRepo.Setup(r => r.Update(It.IsAny<Reservation>()))
                .Returns(reservation);

            // Act
            var result = _service.Approve(reservation.Id);

            // Assert
            result.Status.Should().Be(ReservationStatus.Approved);
            reservation.Event.AvailableSpots.Should().Be(initialSpots - 1); 
            _mockEventRepo.Verify(e => e.Update(reservation.Event), Times.Once);
        }

        [Fact]
        public void Reject_PendingReservation_ShouldChangeStatusToRejected()
        {
            // Arrange
            var reservation = _testData.GeneratePendingReservation();
            var initialSpots = reservation.Event.AvailableSpots;

            _mockReservationRepo.Setup(r => r.GetWithIncludes(
                reservation.Id,
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns(reservation);

            _mockReservationRepo.Setup(r => r.Update(It.IsAny<Reservation>()))
                .Returns(reservation);

            // Act
            var result = _service.Reject(reservation.Id);

            // Assert
            result.Status.Should().Be(ReservationStatus.Rejected);
            reservation.Event.AvailableSpots.Should().Be(initialSpots + 1); 
            _mockEventRepo.Verify(e => e.Update(reservation.Event), Times.Once);
        }

        [Fact]
        public void Reject_NonExistentReservation_ShouldThrowException()
        {
            // Arrange
            _mockReservationRepo.Setup(r => r.GetWithIncludes(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<Reservation, object>>[]>()
            )).Returns((Reservation?)null);

            // Act & Assert
            var act = () => _service.Reject(Guid.NewGuid());

            act.Should().Throw<Exception>().WithMessage("Reservation not found.");
        }
    }
}