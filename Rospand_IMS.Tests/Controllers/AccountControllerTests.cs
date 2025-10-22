using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Rospand_IMS.Controllers;
using Rospand_IMS.Models;
using Rospand_IMS.Services;
using System.Threading.Tasks;
using Xunit;

namespace Rospand_IMS.Tests.Controllers
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Login_Post_WithValidCredentials_ShouldRedirectToHome()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(x => x.AuthenticateAsync("testuser", "testpassword"))
                .ReturnsAsync(new User { Id = 1, Username = "testuser", Role = "User" });

            var controller = new AccountController(mockUserService.Object);
            
            // Set up session
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;
            
            var session = new Mock<ISession>();
            httpContext.Session = session.Object;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var model = new LoginViewModel
            {
                Username = "testuser",
                Password = "testpassword"
            };

            // Act
            var result = await controller.Login(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async Task Login_Post_WithInvalidCredentials_ShouldReturnViewWithModelError()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(x => x.AuthenticateAsync("testuser", "wrongpassword"))
                .ReturnsAsync((User)null);

            var controller = new AccountController(mockUserService.Object);
            
            // Set up session
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;
            
            var session = new Mock<ISession>();
            httpContext.Session = session.Object;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var model = new LoginViewModel
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            // Act
            var result = await controller.Login(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains("Invalid username or password.", controller.ModelState[string.Empty].Errors[0].ErrorMessage);
        }
    }
}