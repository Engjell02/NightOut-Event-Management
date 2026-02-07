using Xunit;
using FluentAssertions;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Domain.DomainModels.Enums;
using Reservation_Management_App.Tests.TestUtilities;

namespace Reservation_Management_App.Tests.UnitTests.BusinessLogic
{
    public class RevenueCalculationTests
    {
        private readonly TestDataGenerator _testData;

        public RevenueCalculationTests()
        {
            _testData = new TestDataGenerator();
        }

        [Fact]
        public void CalculateRevenue_WithApprovedReservations_ShouldReturnCorrectTotal()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                _testData.GenerateApprovedReservation(500, 3),  // 500 * 3 = 1500
                _testData.GenerateApprovedReservation(800, 2),  // 800 * 2 = 1600
                _testData.GenerateApprovedReservation(1000, 4)  // 1000 * 4 = 4000
            };

            // Act
            var revenue = reservations
                .Where(r => r.Status == ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            // Assert
            revenue.Should().Be(7100); // 1500 + 1600 + 4000
        }

        [Fact]
        public void CalculateRevenue_WithMixedStatuses_ShouldOnlyCountApproved()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                _testData.GenerateApprovedReservation(500, 3),   
                _testData.GeneratePendingReservation(),          
                _testData.GenerateRejectedReservation(),         
                _testData.GenerateApprovedReservation(1000, 2)   
            };

            // Act
            var revenue = reservations
                .Where(r => r.Status == ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            // Assert
            revenue.Should().Be(3500); 
        }

        [Fact]
        public void CalculateRevenue_WithNoPendingReservations_ShouldExcludeThem()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                _testData.GeneratePendingReservation(),
                _testData.GeneratePendingReservation(),
                _testData.GeneratePendingReservation()
            };

            // Act
            var revenue = reservations
                .Where(r => r.Status == ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            // Assert
            revenue.Should().Be(0); 
        }

        [Fact]
        public void CalculateRevenue_WithNoRejectedReservations_ShouldExcludeThem()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                _testData.GenerateRejectedReservation(),
                _testData.GenerateRejectedReservation()
            };

            // Act
            var revenue = reservations
                .Where(r => r.Status == ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            // Assert
            revenue.Should().Be(0); 
        }

        [Fact]
        public void CalculateRevenue_WithNoReservations_ShouldReturnZero()
        {
            // Arrange
            var reservations = new List<Reservation>();

            // Act
            var revenue = reservations
                .Where(r => r.Status == ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            // Assert
            revenue.Should().Be(0);
        }

        [Fact]
        public void CalculateRevenue_WithNullEventPrice_ShouldUseZero()
        {
            // Arrange
            var reservation = _testData.GenerateApprovedReservation(0, 5);
            reservation.Event = null;

            var reservations = new List<Reservation> { reservation };

            // Act
            var revenue = reservations
                .Where(r => r.Status == ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            // Assert
            revenue.Should().Be(0);
        }

        [Theory]
        [InlineData(100, 2, 200)]
        [InlineData(500, 4, 2000)]
        [InlineData(1000, 6, 6000)]
        [InlineData(250, 3, 750)]
        public void CalculateRevenue_WithDifferentPricesAndPeople_ShouldCalculateCorrectly(
            decimal price, int people, decimal expectedRevenue)
        {
            // Arrange
            var reservation = _testData.GenerateApprovedReservation(price, people);
            var reservations = new List<Reservation> { reservation };

            // Act
            var revenue = reservations
                .Where(r => r.Status == ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            // Assert
            revenue.Should().Be(expectedRevenue);
        }

        [Fact]
        public void CalculateRevenue_WithMultipleEvents_ShouldSumAllRevenue()
        {
            // Arrange
            var event1 = _testData.GenerateEvent();
            event1.PricePerPerson = 300;

            var event2 = _testData.GenerateEvent();
            event2.PricePerPerson = 600;

            var reservations = new List<Reservation>
            {
                _testData.GenerateReservation(event1),
                _testData.GenerateReservation(event1),
                _testData.GenerateReservation(event2)
            };

            // Mark all as approved
            foreach (var res in reservations)
            {
                res.Status = ReservationStatus.Approved;
                res.NumberOfPeople = 2;
            }

            // Act
            var revenue = reservations
                .Where(r => r.Status == ReservationStatus.Approved)
                .Sum(r => (r.Event?.PricePerPerson ?? 0) * r.NumberOfPeople);

            // Assert
            revenue.Should().Be(2400); 
        }

        [Fact]
        public void CountApprovedReservations_ShouldReturnCorrectCount()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                _testData.GenerateApprovedReservation(500, 3),
                _testData.GenerateApprovedReservation(600, 2),
                _testData.GeneratePendingReservation(),
                _testData.GenerateRejectedReservation()
            };

            // Act
            var count = reservations.Count(r => r.Status == ReservationStatus.Approved);

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public void CountPendingReservations_ShouldReturnCorrectCount()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                _testData.GeneratePendingReservation(),
                _testData.GeneratePendingReservation(),
                _testData.GeneratePendingReservation(),
                _testData.GenerateApprovedReservation(500, 3)
            };

            // Act
            var count = reservations.Count(r => r.Status == ReservationStatus.Pending);

            // Assert
            count.Should().Be(3);
        }
    }
}