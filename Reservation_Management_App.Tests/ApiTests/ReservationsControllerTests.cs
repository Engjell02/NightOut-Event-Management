using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using Reservation_Management_App.Web.Controllers;
using Reservation_Management_App.Service.Interface;
using Reservation_Management_App.Domain.Identity;
using Reservation_Management_App.Domain.DomainModels;
using Reservation_Management_App.Domain.DomainModels.Enums;
using Reservation_Management_App.Tests.TestUtilities;

namespace Reservation_Management_App.Tests.ApiTests
{
    public class ReservationsControllerTests
    {
        private readonly Mock<IReservationService> _mockService;
        private readonly Mock<UserManager<Reservation_Management_AppUser>> _mockUserManager;
        private readonly ReservationsController _controller;
        private readonly TestDataGenerator _testData;
        private readonly string _testUserId = Guid.NewGuid().ToString();

        public ReservationsControllerTests()
        {
            _mockService = new Mock<IReservationService>();

            // Mock UserManager (it has complex dependencies)
            var userStoreMock = new Mock<IUserStore<Reservation_Management_AppUser>>();
            _mockUserManager = new Mock<UserManager<Reservation_Management_AppUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new ReservationsController(_mockService.Object, _mockUserManager.Object);
            _testData = new TestDataGenerator();

            SetupTempData();
            SetupUserContext();
        }

        private void SetupTempData()
        {
            var tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        private void SetupUserContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId)
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(_testUserId);
        }

        [Fact]
        public void My_ShouldReturnViewWithUserReservations()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                _testData.GenerateReservation(userId: _testUserId),
                _testData.GenerateReservation(userId: _testUserId)
            };
            _mockService.Setup(s => s.GetByUser(_testUserId)).Returns(reservations);

            // Act
            var result = _controller.My();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Reservation>>().Subject;
            model.Should().HaveCount(2);
        }

        [Fact]
        public void Reserve_Get_WithValidEventId_ShouldReturnView()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            // Act
            var result = _controller.Reserve(eventId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            // ✅ Fixed - cast ViewBag.EventId to Guid first
            Guid actualEventId = _controller.ViewBag.EventId;
            actualEventId.Should().Be(eventId);
        }

        [Fact]
        public void Reserve_Get_WithEmptyEventId_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.Reserve(Guid.Empty);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void Reserve_Post_WithValidData_ShouldRedirectToMy()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var reservation = _testData.GenerateReservation();

            _mockService.Setup(s => s.CreateReservation(
                eventId,
                _testUserId,
                "John Doe",
                4,
                "+389 70 123 456"))
                .Returns(reservation);

            // Act
            var result = _controller.Reserve(eventId, "John Doe", 4, "+389 70 123 456");

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("My");
            _controller.TempData["Success"].Should().NotBeNull();
        }

        [Fact]
        public void Reserve_Post_WithInvalidNumberOfPeople_ShouldReturnView()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            // Act
            var result = _controller.Reserve(eventId, "John Doe", 8, "+389 70 123 456"); // 8 > 6

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Reserve_Post_WithException_ShouldSetErrorAndRedirect()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            _mockService.Setup(s => s.CreateReservation(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
                .Throws(new Exception("No tables available"));

            // Act
            var result = _controller.Reserve(eventId, "John Doe", 4, "+389 70 123 456");

            // Assert
            _controller.TempData["Error"].Should().Be("No tables available");
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Reserve");
        }

        [Fact]
        public void Cancel_WithSuccessfulCancellation_ShouldSetSuccessMessage()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            _mockService.Setup(s => s.CancelReservation(reservationId, _testUserId))
                .Returns(true);

            // Act
            var result = _controller.Cancel(reservationId);

            // Assert
            _controller.TempData["Success"].Should().Be("Reservation cancelled successfully.");
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("My");
        }

        [Fact]
        public void Cancel_Within24Hours_ShouldSetErrorMessage()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            _mockService.Setup(s => s.CancelReservation(reservationId, _testUserId))
                .Returns(false); // Cannot cancel

            // Act
            var result = _controller.Cancel(reservationId);

            // Assert
            _controller.TempData["Error"].Should().Be("You can cancel only 24 hours or more before the event.");
        }

        [Fact]
        public void Cancel_WithException_ShouldSetErrorMessage()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            _mockService.Setup(s => s.CancelReservation(reservationId, _testUserId))
                .Throws(new Exception("Cancellation failed"));

            // Act
            var result = _controller.Cancel(reservationId);

            // Assert
            _controller.TempData["Error"].Should().Be("Cancellation failed");
        }

        [Fact]
        public void Index_WithoutFilter_ShouldReturnAllReservations()
        {
            // Arrange
            var reservations = _testData.GenerateReservations(5);
            _mockService.Setup(s => s.GetAll()).Returns(reservations);

            // Act
            var result = _controller.Index(null);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Reservation>>().Subject;
            model.Should().HaveCount(5);
        }

        [Fact]
        public void Index_WithEventFilter_ShouldReturnFilteredReservations()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var eventObj = _testData.GenerateEvent();
            eventObj.Id = eventId;

            var reservations = new List<Reservation>
            {
                _testData.GenerateReservation(eventObj),
                _testData.GenerateReservation(eventObj),
                _testData.GenerateReservation() // Different event
            };

            _mockService.Setup(s => s.GetAll()).Returns(reservations);

            // Act
            var result = _controller.Index(eventId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Reservation>>().Subject;
            model.Should().HaveCount(2);
            model.Should().OnlyContain(r => r.EventId == eventId);
        }

        [Fact]
        public void Approve_WithValidId_ShouldRedirectToIndex()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var reservation = _testData.GenerateApprovedReservation(500, 3);
            _mockService.Setup(s => s.Approve(reservationId)).Returns(reservation);

            // Act
            var result = _controller.Approve(reservationId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["Success"].Should().Be("Reservation approved successfully.");
            _mockService.Verify(s => s.Approve(reservationId), Times.Once);
        }

        [Fact]
        public void Approve_WithException_ShouldSetErrorMessage()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            _mockService.Setup(s => s.Approve(reservationId))
                .Throws(new Exception("Approval failed"));

            // Act
            var result = _controller.Approve(reservationId);

            // Assert
            _controller.TempData["Error"].Should().Be("Approval failed");
        }

        [Fact]
        public void Reject_WithValidId_ShouldRedirectToIndex()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var reservation = _testData.GenerateRejectedReservation();
            _mockService.Setup(s => s.Reject(reservationId)).Returns(reservation);

            // Act
            var result = _controller.Reject(reservationId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["Success"].Should().Be("Reservation rejected successfully.");
            _mockService.Verify(s => s.Reject(reservationId), Times.Once);
        }

        [Fact]
        public void Reject_WithException_ShouldSetErrorMessage()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            _mockService.Setup(s => s.Reject(reservationId))
                .Throws(new Exception("Rejection failed"));

            // Act
            var result = _controller.Reject(reservationId);

            // Assert
            _controller.TempData["Error"].Should().Be("Rejection failed");
        }
    }
}