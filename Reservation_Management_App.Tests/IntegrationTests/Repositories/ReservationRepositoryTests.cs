using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reservation_Management_App.Repository;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Domain.DomainModels.Enums;
using Reservation_Management_App.Tests.TestUtilities;

namespace Reservation_Management_App.Tests.IntegrationTests.Repositories
{
    public class ReservationRepositoryTests : IDisposable
    {
        private readonly DatabaseFixture _fixture;
        private readonly Repository<Reservation> _repository;
        private readonly TestDataGenerator _testData;

        public ReservationRepositoryTests()
        {
            _fixture = new DatabaseFixture();
            _repository = new Repository<Reservation>(_fixture.Context);
            _testData = new TestDataGenerator();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void Insert_ShouldAddReservationToDatabase()
        {
            // Arrange
            var reservation = _testData.GenerateReservation();

            // Act
            var result = _repository.Insert(reservation);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(reservation.Id);

            var savedReservation = _fixture.Context.Reservations.Find(reservation.Id);
            savedReservation.Should().NotBeNull();
            savedReservation!.ReservationName.Should().Be(reservation.ReservationName);
        }

        [Fact]
        public void Get_WithValidId_ShouldReturnReservation()
        {
            // Arrange
            var reservation = _testData.GenerateReservation();
            _repository.Insert(reservation);

            // Act
            var result = _repository.Get(reservation.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(reservation.Id);
            result.ReservationName.Should().Be(reservation.ReservationName);
        }

        [Fact]
        public void Get_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var result = _repository.Get(invalidId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetAll_ShouldReturnAllReservations()
        {
            // Arrange
            _fixture.ResetDatabase();
            var reservations = _testData.GenerateReservations(5);
            foreach (var res in reservations)
            {
                _repository.Insert(res);
            }

            // Act
            var result = _repository.GetAll();

            // Assert
            result.Should().HaveCount(5);
        }

        [Fact]
        public void Update_ShouldModifyExistingReservation()
        {
            // Arrange
            var reservation = _testData.GenerateReservation();
            _repository.Insert(reservation);

            // Act
            reservation.Status = ReservationStatus.Approved;
            var result = _repository.Update(reservation);

            // Assert
            result.Status.Should().Be(ReservationStatus.Approved);

            var updated = _repository.Get(reservation.Id);
            updated!.Status.Should().Be(ReservationStatus.Approved);
        }

        [Fact]
        public void Delete_ShouldRemoveReservation()
        {
            // Arrange
            var reservation = _testData.GenerateReservation();
            _repository.Insert(reservation);

            // Act
            _repository.Delete(reservation);

            // Assert
            var deleted = _repository.Get(reservation.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public void GetAll_WithFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            _fixture.ResetDatabase();
            var userId = Guid.NewGuid().ToString();

            var userReservation1 = _testData.GenerateReservation(userId: userId);
            var userReservation2 = _testData.GenerateReservation(userId: userId);
            var otherReservation = _testData.GenerateReservation();

            _repository.Insert(userReservation1);
            _repository.Insert(userReservation2);
            _repository.Insert(otherReservation);

            // Act
            var result = _repository.GetAll(r => r.UserId == userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(r => r.UserId == userId);
        }

        [Fact]
        public void Insert_MultipleReservations_ShouldPersistAll()
        {
            // Arrange
            _fixture.ResetDatabase();
            var reservations = _testData.GenerateReservations(3);

            // Act
            foreach (var res in reservations)
            {
                _repository.Insert(res);
            }

            // Assert
            var allReservations = _repository.GetAll();
            allReservations.Should().HaveCount(3);
        }

        [Fact]
        public void Update_NonExistentReservation_ShouldThrowException()
        {
            // Arrange
            var reservation = _testData.GenerateReservation();
            

            // Act & Assert
            var act = () => _repository.Update(reservation);

            act.Should().Throw<DbUpdateConcurrencyException>();
        }
    }
}