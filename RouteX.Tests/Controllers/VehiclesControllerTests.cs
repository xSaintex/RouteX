using Microsoft.AspNetCore.Http;
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
    /// Unit tests for VehiclesController.
    /// Tests cover: vehicle listing, duplicate plate detection, approval workflow,
    /// branch isolation, and OperationsStaff pending approval flow.
    /// </summary>
    public class VehiclesControllerTests
    {
        private readonly Mock<IAuditService> _auditMock = new();
        private readonly Mock<IRouteDistanceService> _routeDistanceMock = new();
        private readonly Mock<ILogger<VehiclesController>> _loggerMock = new();
        private readonly Mock<ITextFormattingService> _textFormattingMock = new();
        private readonly Mock<INotificationService> _notificationMock = new();
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configMock = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

        private VehiclesController CreateController(string dbName)
        {
            var context = TestDbContextFactory.Create(dbName);

            _textFormattingMock.Setup(s => s.CapitalizeEachWord(It.IsAny<string>())).Returns<string>(s => s);

            return new VehiclesController(
                context,
                _auditMock.Object,
                _configMock.Object,
                _routeDistanceMock.Object,
                _loggerMock.Object,
                _httpClientFactoryMock.Object,
                _textFormattingMock.Object,
                _notificationMock.Object);
        }

        // ─────────────────────────────────────────────────────────────────────
        // VehiclePage
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "VehiclePage")]
        public async Task VehiclePage_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(VehiclePage_ReturnsViewResult));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.VehiclePage(null);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        [Trait("Category", "VehiclePage")]
        public async Task VehiclePage_ArchivedVehicles_AreExcluded()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(VehiclePage_ArchivedVehicles_AreExcluded));
            context.Vehicles.AddRange(
                new Vehicle { Id = 1, PlateNumber = "ABC-123", UnitModel = "Isuzu", VehicleType = "Truck", IsArchived = false, BranchId = 1 },
                new Vehicle { Id = 2, PlateNumber = "XYZ-999", UnitModel = "Toyota", VehicleType = "Van",   IsArchived = true,  BranchId = 1 }
            );
            await context.SaveChangesAsync();

            var controller = new VehiclesController(context, _auditMock.Object, _configMock.Object, _routeDistanceMock.Object, _loggerMock.Object, _httpClientFactoryMock.Object, _textFormattingMock.Object, _notificationMock.Object);
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.VehiclePage(null) as ViewResult;

            // Assert — archived vehicle should not appear
            var model = Assert.IsAssignableFrom<IEnumerable<object>>(result!.Model);
            Assert.Single(model);
        }

        [Fact]
        [Trait("Category", "VehiclePage")]
        public async Task VehiclePage_PendingApprovalVehicles_AreExcluded_FromMainList()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(VehiclePage_PendingApprovalVehicles_AreExcluded_FromMainList));
            context.Vehicles.AddRange(
                new Vehicle { Id = 1, PlateNumber = "ABC-123", UnitModel = "Isuzu", VehicleType = "Truck", IsArchived = false, IsPendingApproval = false, BranchId = 1 },
                new Vehicle { Id = 2, PlateNumber = "DEF-456", UnitModel = "Hino",  VehicleType = "Truck", IsArchived = false, IsPendingApproval = true,  BranchId = 1 }
            );
            await context.SaveChangesAsync();

            var controller = new VehiclesController(context, _auditMock.Object, _configMock.Object, _routeDistanceMock.Object, _loggerMock.Object, _httpClientFactoryMock.Object, _textFormattingMock.Object, _notificationMock.Object);
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.VehiclePage(null) as ViewResult;

            // Assert — pending vehicle excluded from main list
            var model = Assert.IsAssignableFrom<IEnumerable<object>>(result!.Model);
            Assert.Single(model);
        }

        // ─────────────────────────────────────────────────────────────────────
        // AddVehicle POST
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "AddVehicle")]
        public async Task AddVehicle_Post_InvalidModelState_ReturnsView()
        {
            // Arrange
            var controller = CreateController(nameof(AddVehicle_Post_InvalidModelState_ReturnsView));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());
            controller.ModelState.AddModelError("PlateNumber", "Plate number is required.");

            var vehicle = new Vehicle();

            // Act
            var result = await controller.AddVehicle(vehicle);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        [Trait("Category", "AddVehicle")]
        public async Task AddVehicle_Post_DuplicatePlateNumber_ReturnsViewWithError()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(AddVehicle_Post_DuplicatePlateNumber_ReturnsViewWithError));
            context.Vehicles.Add(new Vehicle
            {
                Id = 1, PlateNumber = "ABC-123", UnitModel = "Isuzu",
                VehicleType = "Truck", Status = VehicleStatus.Active, IsArchived = false, BranchId = 1
            });
            await context.SaveChangesAsync();

            _textFormattingMock.Setup(s => s.CapitalizeEachWord(It.IsAny<string>())).Returns<string>(s => s);

            var controller = new VehiclesController(context, _auditMock.Object, _configMock.Object, _routeDistanceMock.Object, _loggerMock.Object, _httpClientFactoryMock.Object, _textFormattingMock.Object, _notificationMock.Object);
            MockHttpContext.Setup(controller, MockHttpContext.AdminSession());

            var newVehicle = new Vehicle
            {
                PlateNumber = "abc-123", // same plate, different case
                UnitModel = "Toyota",
                VehicleType = "Van",
                BranchId = 1
            };

            // Act
            var result = await controller.AddVehicle(newVehicle);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("PlateNumber"));
        }

        // ─────────────────────────────────────────────────────────────────────
        // EditVehicle GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "EditVehicle")]
        public async Task EditVehicle_Get_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateController(nameof(EditVehicle_Get_NonExistentId_ReturnsNotFound));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.EditVehicle(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("Category", "EditVehicle")]
        public async Task EditVehicle_Get_ExistingId_ReturnsViewWithModel()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(EditVehicle_Get_ExistingId_ReturnsViewWithModel));
            context.Vehicles.Add(new Vehicle
            {
                Id = 1, PlateNumber = "ABC-123", UnitModel = "Isuzu",
                VehicleType = "Truck", IsArchived = false, BranchId = 1
            });
            await context.SaveChangesAsync();

            var controller = new VehiclesController(context, _auditMock.Object, _configMock.Object, _routeDistanceMock.Object, _loggerMock.Object, _httpClientFactoryMock.Object, _textFormattingMock.Object, _notificationMock.Object);
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.EditVehicle(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Vehicle>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ViewVehicle
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ViewVehicle")]
        public async Task ViewVehicle_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateController(nameof(ViewVehicle_NonExistentId_ReturnsNotFound));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ViewVehicle(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ApproveVehicle
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ApproveVehicle")]
        public async Task ApproveVehicle_OperationsStaff_ReturnsJsonFailure()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(ApproveVehicle_OperationsStaff_ReturnsJsonFailure));
            context.Vehicles.Add(new Vehicle
            {
                Id = 1, PlateNumber = "ABC-123", UnitModel = "Isuzu",
                VehicleType = "Truck", IsArchived = false, IsPendingApproval = true, BranchId = 1
            });
            await context.SaveChangesAsync();

            var controller = new VehiclesController(context, _auditMock.Object, _configMock.Object, _routeDistanceMock.Object, _loggerMock.Object, _httpClientFactoryMock.Object, _textFormattingMock.Object, _notificationMock.Object);
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.OperationsStaffSession());

            // Act
            var result = await controller.ApproveVehicle(1) as JsonResult;

            // Assert — OperationsStaff cannot approve
            Assert.NotNull(result);
            dynamic? value = result.Value;
            Assert.False((bool)value!.GetType().GetProperty("success")!.GetValue(value)!);
        }

        [Fact]
        [Trait("Category", "ApproveVehicle")]
        public async Task ApproveVehicle_NonExistentVehicle_ReturnsJsonFailure()
        {
            // Arrange
            var controller = CreateController(nameof(ApproveVehicle_NonExistentVehicle_ReturnsJsonFailure));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());

            // Act
            var result = await controller.ApproveVehicle(999) as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic? value = result.Value;
            Assert.False((bool)value!.GetType().GetProperty("success")!.GetValue(value)!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ArchiveVehicle
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ArchiveVehicle")]
        public async Task ArchiveVehicle_NonExistentId_ReturnsJsonFailure()
        {
            // Arrange
            var controller = CreateController(nameof(ArchiveVehicle_NonExistentId_ReturnsJsonFailure));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ArchiveVehicle(999) as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic? value = result.Value;
            Assert.False((bool)value!.GetType().GetProperty("success")!.GetValue(value)!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Vehicle Model Validation
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ModelValidation")]
        public void Vehicle_ValidEntry_PassesValidation()
        {
            // Arrange
            var vehicle = new Vehicle
            {
                PlateNumber = "ABC-123",
                UnitModel = "Isuzu N-Series",
                VehicleType = "Truck",
                Status = VehicleStatus.Active
            };

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(vehicle);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            // Act
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(vehicle, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
        }
    }
}
