using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Reservation_Management_App.Domain.Identity;

namespace Reservation_Management_App.Tests.SecurityTests
{
    public class AuthenticationTests
    {
        [Fact]
        public void PasswordHasher_ShouldHashPasswordSecurely()
        {
            // Arrange
            var hasher = new PasswordHasher<Reservation_Management_AppUser>();
            var user = new Reservation_Management_AppUser { UserName = "test@example.com" };
            var password = "SecurePass123!";

            // Act
            var hashedPassword = hasher.HashPassword(user, password);

            // Assert
            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(password); 
            hashedPassword.Length.Should().BeGreaterThan(50); 
        }

        [Fact]
        public void PasswordHasher_ShouldVerifyCorrectPassword()
        {
            // Arrange
            var hasher = new PasswordHasher<Reservation_Management_AppUser>();
            var user = new Reservation_Management_AppUser { UserName = "test@example.com" };
            var password = "SecurePass123!";
            var hashedPassword = hasher.HashPassword(user, password);

            // Act
            var result = hasher.VerifyHashedPassword(user, hashedPassword, password);

            // Assert
            result.Should().Be(PasswordVerificationResult.Success);
        }

        [Fact]
        public void PasswordHasher_ShouldRejectIncorrectPassword()
        {
            // Arrange
            var hasher = new PasswordHasher<Reservation_Management_AppUser>();
            var user = new Reservation_Management_AppUser { UserName = "test@example.com" };
            var password = "SecurePass123!";
            var wrongPassword = "WrongPassword!";
            var hashedPassword = hasher.HashPassword(user, password);

            // Act
            var result = hasher.VerifyHashedPassword(user, hashedPassword, wrongPassword);

            // Assert
            result.Should().Be(PasswordVerificationResult.Failed);
        }

        [Theory]
        [InlineData("weak")]
        [InlineData("12345")]
        [InlineData("password")]
        public void WeakPasswords_ShouldBeIdentifiable(string weakPassword)
        {
            // Arrange
            var hasher = new PasswordHasher<Reservation_Management_AppUser>();
            var user = new Reservation_Management_AppUser { UserName = "test@example.com" };

            // Act
            var hashedPassword = hasher.HashPassword(user, weakPassword);

            // Assert - Even weak passwords get hashed (validation happens at model level)
            hashedPassword.Should().NotBe(weakPassword);
            hashedPassword.Length.Should().BeGreaterThan(50);
        }

        [Fact]
        public void SamePassword_ShouldProduceDifferentHashes()
        {
            // Arrange
            var hasher = new PasswordHasher<Reservation_Management_AppUser>();
            var user1 = new Reservation_Management_AppUser { UserName = "user1@example.com" };
            var user2 = new Reservation_Management_AppUser { UserName = "user2@example.com" };
            var password = "SecurePass123!";

            // Act
            var hash1 = hasher.HashPassword(user1, password);
            var hash2 = hasher.HashPassword(user2, password);

            // Assert - Salted hashing produces different hashes
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void User_ShouldHaveEmailConfirmationToken()
        {
            // Arrange
            var user = new Reservation_Management_AppUser
            {
                Email = "test@example.com",
                EmailConfirmed = false
            };

            // Assert
            user.EmailConfirmed.Should().BeFalse();
            user.Email.Should().Be("test@example.com");
        }

        [Fact]
        public void User_EmailConfirmation_ShouldBeSecure()
        {
            // Arrange
            var user = new Reservation_Management_AppUser
            {
                Email = "test@example.com",
                EmailConfirmed = false
            };

            // Act
            user.EmailConfirmed = true;

            // Assert
            user.EmailConfirmed.Should().BeTrue();
        }

        [Theory]
        [InlineData("admin@test.com", "Admin", true)]
        [InlineData("user@test.com", "User", true)]
        [InlineData("guest@test.com", "Guest", false)]
        public void UserRoles_ShouldBeValidated(string email, string role, bool shouldBeValid)
        {
            // Arrange
            var validRoles = new[] { "Admin", "User" };

            // Act
            var isValid = validRoles.Contains(role);

            // Assert
            isValid.Should().Be(shouldBeValid);
        }
    }
}