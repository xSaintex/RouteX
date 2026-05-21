using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RouteX.Controllers;
using RouteX.Models;
using RouteX.Services;
using RouteX.Tests.Helpers;

namespace RouteX.Tests.Controllers
{
    /// <summary>
    /// Unit tests for AccountController.
    /// Tests cover: login validation, inactive account blocking, role-based redirect, logout.
    /// </summary>
    public class AccountControllerTests
    {
        private readonly Mock<IAuditService> _auditMock = new();
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;

        public AccountControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                null!, null!, null!, null!);
        }

        private AccountController CreateController(string dbName)
        {
            var context = TestDbContextFactory.Create(dbName);
            return new AccountController(context, _signInManagerMock.Object, _userManagerMock.Object, _auditMock.Object);
        }

        // ─────────────────────────────────────────────────────────────────────
        // LoginPage GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "Login")]
        public void LoginPage_Get_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(LoginPage_Get_ReturnsViewResult));

            // Act
            var result = controller.LoginPage();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // AccessDenied GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "AccessDenied")]
        public void AccessDenied_Get_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(AccessDenied_Get_ReturnsViewResult));

            // Act
            var result = controller.AccessDenied();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Login POST — Validation
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_InvalidModelState_ReturnsLoginPage()
        {
            // Arrange
            var controller = CreateController(nameof(Login_Post_InvalidModelState_ReturnsLoginPage));
            controller.ControllerContext = MockHttpContext.Create(new Dictionary<string, string>());
            controller.ModelState.AddModelError("Email", "Email is required.");

            var model = new User { Email = "", Password = "" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("LoginPage", viewResult.ViewName);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_EmptyEmail_ReturnsLoginPage()
        {
            // Arrange
            var controller = CreateController(nameof(Login_Post_EmptyEmail_ReturnsLoginPage));
            controller.ControllerContext = MockHttpContext.Create(new Dictionary<string, string>());

            var model = new User { Email = "", Password = "SomePassword1!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("LoginPage", viewResult.ViewName);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_InactiveAccount_ReturnsLoginPageWithError()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(Login_Post_InactiveAccount_ReturnsLoginPageWithError));
            context.Users.Add(new User
            {
                UserId = 1,
                Email = "inactive@routex.com",
                Password = "hashed",
                Role = "Admin",
                Status = UserStatus.Inactive.ToString(),
                FirstName = "Test",
                LastName = "User"
            });
            await context.SaveChangesAsync();

            var controller = new AccountController(context, _signInManagerMock.Object, _userManagerMock.Object, _auditMock.Object);
            controller.ControllerContext = MockHttpContext.Create(new Dictionary<string, string>());

            var model = new User { Email = "inactive@routex.com", Password = "Password1!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("LoginPage", viewResult.ViewName);
            Assert.NotNull(controller.ViewBag.ErrorMessage);
            Assert.Contains("inactive", controller.ViewBag.ErrorMessage.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_ArchivedAccount_ReturnsLoginPageWithError()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(Login_Post_ArchivedAccount_ReturnsLoginPageWithError));
            context.Users.Add(new User
            {
                UserId = 2,
                Email = "archived@routex.com",
                Password = "hashed",
                Role = "Finance",
                Status = UserStatus.Archived.ToString(),
                FirstName = "Archived",
                LastName = "User"
            });
            await context.SaveChangesAsync();

            var controller = new AccountController(context, _signInManagerMock.Object, _userManagerMock.Object, _auditMock.Object);
            controller.ControllerContext = MockHttpContext.Create(new Dictionary<string, string>());

            var model = new User { Email = "archived@routex.com", Password = "Password1!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("LoginPage", viewResult.ViewName);
            Assert.NotNull(controller.ViewBag.ErrorMessage);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_FailedSignIn_LogsFailedAttempt()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(Login_Post_FailedSignIn_LogsFailedAttempt));

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var controller = new AccountController(context, _signInManagerMock.Object, _userManagerMock.Object, _auditMock.Object);
            controller.ControllerContext = MockHttpContext.Create(new Dictionary<string, string>());

            var model = new User { Email = "wrong@routex.com", Password = "WrongPassword1!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("LoginPage", viewResult.ViewName);

            // Verify failed login was logged to audit
            _auditMock.Verify(a => a.LogActionAsync(
                It.Is<string>(e => e == "wrong@routex.com"),
                It.Is<string>(s => s.StartsWith("FailedLogin:"))),
                Times.Once);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_LockedOutAccount_ShowsLockoutMessage()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(Login_Post_LockedOutAccount_ShowsLockoutMessage));

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            var controller = new AccountController(context, _signInManagerMock.Object, _userManagerMock.Object, _auditMock.Object);
            controller.ControllerContext = MockHttpContext.Create(new Dictionary<string, string>());

            var model = new User { Email = "locked@routex.com", Password = "Password1!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Contains("locked", controller.ViewBag.ErrorMessage.ToString(), StringComparison.OrdinalIgnoreCase);

            // Verify lockout was logged
            _auditMock.Verify(a => a.LogActionAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("AccountLocked"))),
                Times.Once);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_SuccessfulLogin_AdminRole_RedirectsToHome()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(Login_Post_SuccessfulLogin_AdminRole_RedirectsToHome));
            context.Users.Add(new User
            {
                UserId = 1,
                Email = "admin@routex.com",
                Password = "hashed",
                Role = "Admin",
                Status = UserStatus.Active.ToString(),
                FirstName = "Admin",
                LastName = "User",
                BranchId = 1
            });
            await context.SaveChangesAsync();

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync("admin@routex.com", It.IsAny<string>(), false, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _userManagerMock
                .Setup(u => u.FindByEmailAsync("admin@routex.com"))
                .ReturnsAsync(new IdentityUser { Email = "admin@routex.com" });

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);

            var controller = new AccountController(context, _signInManagerMock.Object, _userManagerMock.Object, _auditMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object };

            var model = new User { Email = "admin@routex.com", Password = "Password1!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_SuccessfulLogin_FinanceRole_RedirectsToFinanceDashboard()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(Login_Post_SuccessfulLogin_FinanceRole_RedirectsToFinanceDashboard));
            context.Users.Add(new User
            {
                UserId = 2,
                Email = "finance@routex.com",
                Password = "hashed",
                Role = "Finance",
                Status = UserStatus.Active.ToString(),
                FirstName = "Finance",
                LastName = "User",
                BranchId = 1
            });
            await context.SaveChangesAsync();

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync("finance@routex.com", It.IsAny<string>(), false, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _userManagerMock
                .Setup(u => u.FindByEmailAsync("finance@routex.com"))
                .ReturnsAsync(new IdentityUser { Email = "finance@routex.com" });

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);

            var controller = new AccountController(context, _signInManagerMock.Object, _userManagerMock.Object, _auditMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object };

            var model = new User { Email = "finance@routex.com", Password = "Password1!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("FinanceDashboard", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        [Trait("Category", "Login")]
        public async Task Login_Post_SuccessfulLogin_OperationsStaff_RedirectsToOpStaffDashboard()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(Login_Post_SuccessfulLogin_OperationsStaff_RedirectsToOpStaffDashboard));
            context.Users.Add(new User
            {
                UserId = 3,
                Email = "ops@routex.com",
                Password = "hashed",
                Role = "OperationsStaff",
                Status = UserStatus.Active.ToString(),
                FirstName = "Ops",
                LastName = "Staff",
                BranchId = 1
            });
            await context.SaveChangesAsync();

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync("ops@routex.com", It.IsAny<string>(), false, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _userManagerMock
                .Setup(u => u.FindByEmailAsync("ops@routex.com"))
                .ReturnsAsync(new IdentityUser { Email = "ops@routex.com" });

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);

            var controller = new AccountController(context, _signInManagerMock.Object, _userManagerMock.Object, _auditMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object };

            var model = new User { Email = "ops@routex.com", Password = "Password1!" };

            // Act
            var result = await controller.Login(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OpStaffDashboard", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }
    }
}
