using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RouteX.Controllers;
using RouteX.Models;
using RouteX.Services;
using RouteX.Tests.Helpers;

namespace RouteX.Tests.Controllers
{
    /// <summary>
    /// Unit tests for UsersController.
    /// Tests cover: SuperAdmin-only access enforcement, user creation, archive protection.
    /// </summary>
    public class UsersControllerTests
    {
        private readonly Mock<IAuditService> _auditMock = new();
        private readonly Mock<ILogger<UsersController>> _loggerMock = new();
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;

        public UsersControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private UsersController CreateController(string dbName)
        {
            var context = TestDbContextFactory.Create(dbName);
            return new UsersController(context, _userManagerMock.Object, _auditMock.Object, _loggerMock.Object);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UsersPage — SuperAdmin only
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "UsersPage")]
        public async Task UsersPage_SuperAdmin_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(UsersPage_SuperAdmin_ReturnsViewResult));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.UsersPage();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        [Trait("Category", "UsersPage")]
        public async Task UsersPage_AdminRole_ReturnsForbid()
        {
            // Arrange
            var controller = CreateController(nameof(UsersPage_AdminRole_ReturnsForbid));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());

            // Act
            var result = await controller.UsersPage();

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        [Trait("Category", "UsersPage")]
        public async Task UsersPage_FinanceRole_ReturnsForbid()
        {
            // Arrange
            var controller = CreateController(nameof(UsersPage_FinanceRole_ReturnsForbid));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.FinanceSession());

            // Act
            var result = await controller.UsersPage();

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        [Trait("Category", "UsersPage")]
        public async Task UsersPage_OperationsStaff_ReturnsForbid()
        {
            // Arrange
            var controller = CreateController(nameof(UsersPage_OperationsStaff_ReturnsForbid));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.OperationsStaffSession());

            // Act
            var result = await controller.UsersPage();

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        [Trait("Category", "UsersPage")]
        public async Task UsersPage_ExcludesArchivedUsers()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(UsersPage_ExcludesArchivedUsers));
            context.Users.AddRange(
                new User { UserId = 1, Email = "active@routex.com",   Role = "Admin",   Status = UserStatus.Active.ToString(),   FirstName = "A", LastName = "B" },
                new User { UserId = 2, Email = "archived@routex.com", Role = "Finance", Status = UserStatus.Archived.ToString(), FirstName = "C", LastName = "D" }
            );
            await context.SaveChangesAsync();

            var controller = new UsersController(context, _userManagerMock.Object, _auditMock.Object, _loggerMock.Object);
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.UsersPage() as ViewResult;

            // Assert
            var model = Assert.IsAssignableFrom<List<User>>(result!.Model);
            Assert.Single(model);
            Assert.NotEqual(UserStatus.Archived.ToString(), model[0].Status);
        }

        // ─────────────────────────────────────────────────────────────────────
        // AddUser GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "AddUser")]
        public async Task AddUser_Get_SuperAdmin_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(AddUser_Get_SuperAdmin_ReturnsViewResult));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.AddUser();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        [Trait("Category", "AddUser")]
        public async Task AddUser_Get_NonSuperAdmin_ReturnsForbid()
        {
            // Arrange
            var controller = CreateController(nameof(AddUser_Get_NonSuperAdmin_ReturnsForbid));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());

            // Act
            var result = await controller.AddUser();

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // EditUser GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "EditUser")]
        public async Task EditUser_Get_NonSuperAdmin_ReturnsForbid()
        {
            // Arrange
            var controller = CreateController(nameof(EditUser_Get_NonSuperAdmin_ReturnsForbid));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.FinanceSession());

            // Act
            var result = await controller.EditUser(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        [Trait("Category", "EditUser")]
        public async Task EditUser_Get_NonExistentUser_RedirectsToUsersPage()
        {
            // Arrange
            var controller = CreateController(nameof(EditUser_Get_NonExistentUser_RedirectsToUsersPage));
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.EditUser(999);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UsersPage", redirect.ActionName);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ViewUser
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ViewUser")]
        public async Task ViewUser_NonSuperAdmin_ReturnsForbid()
        {
            // Arrange
            var controller = CreateController(nameof(ViewUser_NonSuperAdmin_ReturnsForbid));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.OperationsStaffSession());

            // Act
            var result = await controller.ViewUser(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        [Trait("Category", "ViewUser")]
        public async Task ViewUser_SuperAdmin_ExistingUser_ReturnsViewWithModel()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(ViewUser_SuperAdmin_ExistingUser_ReturnsViewWithModel));
            context.Users.Add(new User
            {
                UserId = 1, Email = "test@routex.com", Role = "Admin",
                Status = UserStatus.Active.ToString(), FirstName = "Test", LastName = "User"
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context, _userManagerMock.Object, _auditMock.Object, _loggerMock.Object);
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ViewUser(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<User>(result.Model);
            Assert.Equal(1, model.UserId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ArchiveUser
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ArchiveUser")]
        public async Task ArchiveUser_NonSuperAdmin_ReturnsJsonFailure()
        {
            // Arrange
            var controller = CreateController(nameof(ArchiveUser_NonSuperAdmin_ReturnsJsonFailure));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());

            // Act
            var result = await controller.ArchiveUser(1) as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic? value = result.Value;
            Assert.False((bool)value!.GetType().GetProperty("success")!.GetValue(value)!);
        }

        [Fact]
        [Trait("Category", "ArchiveUser")]
        public async Task ArchiveUser_ProtectedEmail_ReturnsJsonFailure()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(ArchiveUser_ProtectedEmail_ReturnsJsonFailure));
            context.Users.Add(new User
            {
                UserId = 1, Email = "superadmin@routex.com", Role = "SuperAdmin",
                Status = UserStatus.Active.ToString(), FirstName = "Super", LastName = "Admin"
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context, _userManagerMock.Object, _auditMock.Object, _loggerMock.Object);
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ArchiveUser(1) as JsonResult;

            // Assert — protected accounts cannot be archived
            Assert.NotNull(result);
            dynamic? value = result.Value;
            Assert.False((bool)value!.GetType().GetProperty("success")!.GetValue(value)!);
        }

        [Fact]
        [Trait("Category", "ArchiveUser")]
        public async Task ArchiveUser_NonExistentUser_ReturnsJsonFailure()
        {
            // Arrange
            var controller = CreateController(nameof(ArchiveUser_NonExistentUser_ReturnsJsonFailure));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ArchiveUser(999) as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic? value = result.Value;
            Assert.False((bool)value!.GetType().GetProperty("success")!.GetValue(value)!);
        }
    }
}
