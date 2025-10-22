using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Rospand_IMS.Tests
{
    public class UserServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            // Set up in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "UserManagementTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _userService = new UserService(_context);
            
            // Clear database before each test
            _context.Database.EnsureCreated();
            _context.Users.RemoveRange(_context.Users);
            _context.RoleMasters.RemoveRange(_context.RoleMasters);
            _context.PageAccesses.RemoveRange(_context.PageAccesses);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUserWithHashedPassword()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Role = "User",
                IsActive = true
            };
            string password = "testpassword123";

            // Act
            var createdUser = await _userService.CreateUserAsync(user, password);

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal("testuser", createdUser.Username);
            Assert.NotEqual(password, createdUser.PasswordHash); // Password should be hashed
            Assert.True(_userService.VerifyPassword(password, createdUser.PasswordHash));
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnUser_WhenValidCredentialsProvided()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Role = "User",
                IsActive = true
            };
            string password = "testpassword123";
            
            await _userService.CreateUserAsync(user, password);

            // Act
            var authenticatedUser = await _userService.AuthenticateAsync("testuser", password);

            // Assert
            Assert.NotNull(authenticatedUser);
            Assert.Equal("testuser", authenticatedUser.Username);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenInvalidPasswordProvided()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Role = "User",
                IsActive = true
            };
            string password = "testpassword123";
            
            await _userService.CreateUserAsync(user, password);

            // Act
            var authenticatedUser = await _userService.AuthenticateAsync("testuser", "wrongpassword");

            // Assert
            Assert.Null(authenticatedUser);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldCreateRole()
        {
            // Arrange
            var role = new RoleMaster
            {
                RoleName = "TestRole"
            };

            // Act
            var createdRole = await _userService.CreateRoleAsync(role);

            // Assert
            Assert.NotNull(createdRole);
            Assert.Equal("TestRole", createdRole.RoleName);
        }

        [Fact]
        public async Task HashPassword_ShouldReturnDifferentHashes_ForSamePassword()
        {
            // Arrange
            string password = "testpassword123";

            // Act
            var hash1 = _userService.HashPassword(password);
            var hash2 = _userService.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2); // BCrypt should generate different hashes each time
            Assert.True(_userService.VerifyPassword(password, hash1));
            Assert.True(_userService.VerifyPassword(password, hash2));
        }
    }
}