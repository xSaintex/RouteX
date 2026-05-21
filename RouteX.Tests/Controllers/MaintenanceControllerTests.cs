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
    /// Unit tests for MaintenanceController.
    /// Tests cover: listing, validation, future date enforcement, branch isolation.
    /// </summary>
    public class MaintenanceControllerTests
    {
        private readonly Mock<IAuditService> _auditMock = new();
        private readonly Mock<ITextFormattingService> _textFormattingMock = new();
        private readonly Mock<ILogger<MaintenanceController>> _loggerMock = new();

        private MaintenanceController CreateController(string dbName)
        {
            var context = TestDbContextFactory.Create(dbName);

            _textFormattingMock.Setup(s => s.CapitalizeEachWord(It.IsAny<string>())).Returns<string>(s => s);
            _textFormattingMock.Setup(s => s.FormatName(It.IsAny<string>())).Returns<string>(s => s);
            _textFormattingMock.Setup(s => s.CapitalizeFirstLetter(It.IsAny<string>())).Returns<string>(s => s);

            return new MaintenanceController(context, _auditMock.Object, _textFormattingMock.Object, _loggerMock.Object);
        }

        // ─────────────────────────────────────────────────────────────────────
        // MaintenancePage
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "MaintenancePage")]
        public async Task MaintenancePage_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(MaintenancePage_ReturnsViewResult));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.MaintenancePage(null);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        [Trait("Category", "MaintenancePage")]
        public async Task MaintenancePage_ArchivedEntries_AreExcluded()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(MaintenancePage_ArchivedEntries_AreExcluded));
            context.MaintenanceEntries.AddRange(
                new MaintenanceEntry { Id = 1, IsArchived = false, BranchId = 1, PlateNumber = "ABC-123", ServiceDate = DateTime.Now },
                new MaintenanceEntry { Id = 2, IsArchived = true,  BranchId = 1, PlateNumber = "XYZ-999", ServiceDate = DateTime.Now }
            );
            await context.SaveChangesAsync();

            var controller = new MaintenanceController(context, _auditMock.Object, _textFormattingMock.Object, _loggerMock.Object);
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.MaintenancePage(null) as ViewResult;

            // Assert
            var model = Assert.IsAssignableFrom<List<MaintenanceEntry>>(result!.Model);
            Assert.Single(model);
            Assert.NotEqual(true, model[0].IsArchived);
        }

        // ─────────────────────────────────────────────────────────────────────
        // AddMaintenance GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "AddMaintenance")]
        public async Task AddMaintenance_Get_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(AddMaintenance_Get_ReturnsViewResult));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());

            // Act
            var result = await controller.AddMaintenance();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // AddMaintenance POST — Validation
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "AddMaintenance")]
        public async Task AddMaintenance_Post_InvalidModelState_ReturnsView()
        {
            // Arrange
            var controller = CreateController(nameof(AddMaintenance_Post_InvalidModelState_ReturnsView));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());
            controller.ModelState.AddModelError("ServiceType", "Service type is required.");

            var entry = new MaintenanceEntry();

            // Act
            var result = await controller.AddMaintenance(entry);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        [Trait("Category", "AddMaintenance")]
        public async Task AddMaintenance_Post_NextServiceDuePastDate_ReturnsViewWithError()
        {
            // Arrange
            var controller = CreateController(nameof(AddMaintenance_Post_NextServiceDuePastDate_ReturnsViewWithError));
            MockHttpContext.Setup(controller, MockHttpContext.AdminSession());

            var entry = new MaintenanceEntry
            {
                PlateNumber = "ABC-123",
                ServiceType = "Oil Change",
                ServiceDate = DateTime.Today,
                NextServiceDue = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await controller.AddMaintenance(entry);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("NextServiceDue"));
        }

        [Fact]
        [Trait("Category", "AddMaintenance")]
        public async Task AddMaintenance_Post_NextServiceDueTodayDate_ReturnsViewWithError()
        {
            // Arrange
            var controller = CreateController(nameof(AddMaintenance_Post_NextServiceDueTodayDate_ReturnsViewWithError));
            MockHttpContext.Setup(controller, MockHttpContext.AdminSession());

            var entry = new MaintenanceEntry
            {
                PlateNumber = "ABC-123",
                ServiceType = "Oil Change",
                ServiceDate = DateTime.Today,
                NextServiceDue = DateTime.Today
            };

            // Act
            var result = await controller.AddMaintenance(entry);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("NextServiceDue"));
        }

        [Fact]
        [Trait("Category", "AddMaintenance")]
        public async Task AddMaintenance_Post_NextServiceDueFutureDate_PassesDateValidation()
        {
            // Arrange
            var controller = CreateController(nameof(AddMaintenance_Post_NextServiceDueFutureDate_PassesDateValidation));
            MockHttpContext.Setup(controller, MockHttpContext.AdminSession());

            var entry = new MaintenanceEntry
            {
                PlateNumber = "ABC-123",
                ServiceType = "Oil Change",
                ServiceDate = DateTime.Today,
                NextServiceDue = DateTime.Today.AddDays(30)
            };

            // Act
            var result = await controller.AddMaintenance(entry);

            // Assert — NextServiceDue error should NOT be present
            Assert.False(controller.ModelState.ContainsKey("NextServiceDue"));
        }

        // ─────────────────────────────────────────────────────────────────────
        // EditMaintenance GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "EditMaintenance")]
        public async Task EditMaintenance_Get_NonExistentId_RedirectsToMaintenancePage()
        {
            // Arrange
            var controller = CreateController(nameof(EditMaintenance_Get_NonExistentId_RedirectsToMaintenancePage));
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.EditMaintenance(999);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MaintenancePage", redirect.ActionName);
        }

        [Fact]
        [Trait("Category", "EditMaintenance")]
        public async Task EditMaintenance_Get_ExistingId_ReturnsViewWithModel()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(EditMaintenance_Get_ExistingId_ReturnsViewWithModel));
            context.MaintenanceEntries.Add(new MaintenanceEntry
            {
                Id = 1, IsArchived = false, BranchId = 1,
                PlateNumber = "ABC-123", ServiceType = "Oil Change", ServiceDate = DateTime.Now
            });
            await context.SaveChangesAsync();

            var controller = new MaintenanceController(context, _auditMock.Object, _textFormattingMock.Object, _loggerMock.Object);
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.EditMaintenance(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MaintenanceEntry>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ViewMaintenance
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ViewMaintenance")]
        public async Task ViewMaintenance_NonExistentId_RedirectsToMaintenancePage()
        {
            // Arrange
            var controller = CreateController(nameof(ViewMaintenance_NonExistentId_RedirectsToMaintenancePage));
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ViewMaintenance(999);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MaintenancePage", redirect.ActionName);
        }

        [Fact]
        [Trait("Category", "ViewMaintenance")]
        public async Task ViewMaintenance_ExistingId_ReturnsViewWithModel()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(ViewMaintenance_ExistingId_ReturnsViewWithModel));
            context.MaintenanceEntries.Add(new MaintenanceEntry
            {
                Id = 3, IsArchived = false, BranchId = 1,
                PlateNumber = "DEF-456", ServiceType = "Tire Rotation", ServiceDate = DateTime.Now
            });
            await context.SaveChangesAsync();

            var controller = new MaintenanceController(context, _auditMock.Object, _textFormattingMock.Object, _loggerMock.Object);
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ViewMaintenance(3);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MaintenanceEntry>(viewResult.Model);
            Assert.Equal(3, model.Id);
        }
    }
}
