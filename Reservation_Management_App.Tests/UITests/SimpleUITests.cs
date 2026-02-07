using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Reservation_Management_App.Tests.TestUtilities;

namespace Reservation_Management_App.Tests.UITests
{
    public class SimpleUITests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SimpleUITests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task HomePage_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task HomePage_ShouldContainNightOut()
        {
            // Act
            var response = await _client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            content.Should().Contain("NightOut", "Homepage should display app name");
        }

        [Fact]
        public async Task EventsPage_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task EventsPage_ShouldShowEvents()
        {
            // Act
            var response = await _client.GetAsync("/Events");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LocationsPage_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Locations");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PerformersPage_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Performers");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task LoginPage_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Identity/Account/Login");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task LoginPage_ShouldContainLoginForm()
        {
            // Act
            var response = await _client.GetAsync("/Identity/Account/Login");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            content.Should().Contain("Login", "Login page should have login text");
            content.Should().Contain("Email", "Login page should have email field");
            content.Should().Contain("Password", "Login page should have password field");
        }

        [Fact]
        public async Task RegisterPage_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Identity/Account/Register");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminRegisterPage_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Identity/Account/AdminRegister");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminRegisterPage_ShouldContainAdminText()
        {
            // Act
            var response = await _client.GetAsync("/Identity/Account/AdminRegister");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            content.Should().Contain("Admin", "Admin register page should indicate admin access");
        }

        [Fact]
        public async Task Dashboard_WithoutAuth_ShouldRedirect()
        {
            // Act
            var response = await _client.GetAsync("/Home/Dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task MyReservations_WithoutAuth_ShouldRedirect()
        {
            // Act
            var response = await _client.GetAsync("/Reservations/My");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task NonExistentPage_ShouldReturn404()
        {
            // Act
            var response = await _client.GetAsync("/ThisPageDoesNotExist");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}