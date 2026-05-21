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
    /// Unit tests for FuelController.
    /// Tests cover: validation, branch isolation, archive access, and model state handling.
    /// </summary>
    public class FuelControllerTests
    {
        private readonly Mock<IAuditService> _auditMock = new();
        private readonly Mock<IFuelPriceService> _fuelPriceMock = new();
        private readonly Mock<ILogger<FuelController>> _loggerMock = new();
        private readonly Mock<ITextFormattingService> _textFormattingMock = new();

        private FuelController CreateController(string dbName)
        {
            var context = TestDbContextFactory.Create(dbName);

            // TextFormattingService returns input unchanged for simplicity in tests
            _textFormattingMock.Setup(s => s.FormatName(It.IsAny<string>())).Returns<string>(s => s);
            _textFormattingMock.Setup(s => s.CapitalizeEachWord(It.IsAny<string>())).Returns<string>(s => s);
            _textFormattingMock.Setup(s => s.CapitalizeFirstLetter(It.IsAny<string>())).Returns<string>(s => s);

            return new FuelController(
                context,
                _auditMock.Object,
                _fuelPriceMock.Object,
                _loggerMock.Object,
                _textFormattingMock.Object);
        }

        // ─────────────────────────────────────────────────────────────────────
        // FuelPage
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "FuelPage")]
        public async Task FuelPage_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(FuelPage_ReturnsViewResult));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.FuelPage();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        [Trait("Category", "FuelPage")]
        public async Task FuelPage_SuperAdmin_SeesAllBranchEntries()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(FuelPage_SuperAdmin_SeesAllBranchEntries));

            // Seed branches and vehicles so Include() navigation works in-memory
            context.Branches.AddRange(
                new Branch { BranchId = 1, BranchName = "Branch A", IsArchived = false, Status = BranchStatus.Active },
                new Branch { BranchId = 2, BranchName = "Branch B", IsArchived = false, Status = BranchStatus.Active }
            );
            context.Vehicles.AddRange(
                new Vehicle { Id = 1, PlateNumber = "AAA-111", UnitModel = "Isuzu", VehicleType = "Truck", BranchId = 1 },
                new Vehicle { Id = 2, PlateNumber = "BBB-222", UnitModel = "Toyota", VehicleType = "Van",   BranchId = 2 }
            );
            context.FuelEntries.AddRange(
                new FuelEntry { Id = 1, BranchId = 1, IsArchived = false, Driver = "A", FuelStation = "S1", FuelType = "Diesel", DateTime = DateTime.Now, Liters = 10, TotalCost = 500, VehicleId = 1 },
                new FuelEntry { Id = 2, BranchId = 2, IsArchived = false, Driver = "B", FuelStation = "S2", FuelType = "Diesel", DateTime = DateTime.Now, Liters = 20, TotalCost = 1000, VehicleId = 2 }
            );
            await context.SaveChangesAsync();

            var controller = new FuelController(context, _auditMock.Object, _fuelPriceMock.Object, _loggerMock.Object, _textFormattingMock.Object);
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.FuelPage() as ViewResult;

            // Assert
            var model = Assert.IsAssignableFrom<List<FuelEntry>>(result!.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        [Trait("Category", "FuelPage")]
        public async Task FuelPage_ArchivedEntries_AreExcluded()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(FuelPage_ArchivedEntries_AreExcluded));

            context.Branches.Add(new Branch { BranchId = 1, BranchName = "Branch A", IsArchived = false, Status = BranchStatus.Active });
            context.Vehicles.Add(new Vehicle { Id = 1, PlateNumber = "AAA-111", UnitModel = "Isuzu", VehicleType = "Truck", BranchId = 1 });
            context.FuelEntries.AddRange(
                new FuelEntry { Id = 1, BranchId = 1, IsArchived = false, Driver = "A", FuelStation = "S1", FuelType = "Diesel", DateTime = DateTime.Now, Liters = 10, TotalCost = 500, VehicleId = 1 },
                new FuelEntry { Id = 2, BranchId = 1, IsArchived = true,  Driver = "B", FuelStation = "S2", FuelType = "Diesel", DateTime = DateTime.Now, Liters = 20, TotalCost = 1000, VehicleId = 1 }
            );
            await context.SaveChangesAsync();

            var controller = new FuelController(context, _auditMock.Object, _fuelPriceMock.Object, _loggerMock.Object, _textFormattingMock.Object);
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.FuelPage() as ViewResult;

            // Assert
            var model = Assert.IsAssignableFrom<List<FuelEntry>>(result!.Model);
            Assert.Single(model);
            Assert.False(model[0].IsArchived);
        }

        // ─────────────────────────────────────────────────────────────────────
        // AddFuel GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "AddFuel")]
        public async Task AddFuel_Get_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController(nameof(AddFuel_Get_ReturnsViewResult));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());

            // Act
            var result = await controller.AddFuel();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // AddFuel POST — Validation
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "AddFuel")]
        public async Task AddFuel_Post_InvalidModelState_ReturnsViewWithErrors()
        {
            // Arrange
            var controller = CreateController(nameof(AddFuel_Post_InvalidModelState_ReturnsViewWithErrors));
            MockHttpContext.Setup(controller, MockHttpContext.AdminSession());
            controller.ModelState.AddModelError("Driver", "Driver name is required.");

            var entry = new FuelEntry();

            // Act
            var result = await controller.AddFuel(entry);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(entry, viewResult.Model);
            Assert.True(controller.TempData.ContainsKey("Error"));
        }

        [Fact]
        [Trait("Category", "AddFuel")]
        public async Task AddFuel_Post_ZeroLiters_FailsValidation()
        {
            // Arrange
            var controller = CreateController(nameof(AddFuel_Post_ZeroLiters_FailsValidation));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.AdminSession());

            var entry = new FuelEntry
            {
                VehicleId = 1,
                Driver = "Juan Dela Cruz",
                DateTime = DateTime.Now,
                FuelStation = "Shell",
                Odometer = 1000,
                Liters = 0,       // invalid — must be > 0
                TotalCost = 500,
                FuelType = "Diesel"
            };

            // Simulate model validation
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(entry);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entry, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("Liters"));
        }

        [Fact]
        [Trait("Category", "AddFuel")]
        public async Task AddFuel_Post_ZeroTotalCost_FailsValidation()
        {
            // Arrange
            var entry = new FuelEntry
            {
                VehicleId = 1,
                Driver = "Juan Dela Cruz",
                DateTime = DateTime.Now,
                FuelStation = "Shell",
                Odometer = 1000,
                Liters = 10,
                TotalCost = 0,    // invalid — must be > 0
                FuelType = "Diesel"
            };

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(entry);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entry, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("TotalCost"));
        }

        [Fact]
        [Trait("Category", "AddFuel")]
        public async Task AddFuel_Post_MissingDriver_FailsValidation()
        {
            // Arrange
            var entry = new FuelEntry
            {
                VehicleId = 1,
                Driver = "",      // invalid — required
                DateTime = DateTime.Now,
                FuelStation = "Shell",
                Odometer = 1000,
                Liters = 10,
                TotalCost = 500,
                FuelType = "Diesel"
            };

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(entry);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entry, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("Driver"));
        }

        [Fact]
        [Trait("Category", "AddFuel")]
        public async Task AddFuel_Post_MissingFuelType_FailsValidation()
        {
            // Arrange
            var entry = new FuelEntry
            {
                VehicleId = 1,
                Driver = "Juan",
                DateTime = DateTime.Now,
                FuelStation = "Shell",
                Odometer = 1000,
                Liters = 10,
                TotalCost = 500,
                FuelType = ""     // invalid — required
            };

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(entry);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entry, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("FuelType"));
        }

        [Fact]
        [Trait("Category", "AddFuel")]
        public async Task AddFuel_Post_VehicleIdZero_FailsValidation()
        {
            // Arrange
            var entry = new FuelEntry
            {
                VehicleId = 0,    // invalid — must be >= 1
                Driver = "Juan",
                DateTime = DateTime.Now,
                FuelStation = "Shell",
                Odometer = 1000,
                Liters = 10,
                TotalCost = 500,
                FuelType = "Diesel"
            };

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(entry);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entry, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("VehicleId"));
        }

        // ─────────────────────────────────────────────────────────────────────
        // EditFuel GET
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "EditFuel")]
        public async Task EditFuel_Get_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateController(nameof(EditFuel_Get_NonExistentId_ReturnsNotFound));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.EditFuel(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("Category", "EditFuel")]
        public async Task EditFuel_Get_ExistingId_ReturnsViewWithModel()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(EditFuel_Get_ExistingId_ReturnsViewWithModel));
            context.Vehicles.Add(new Vehicle { Id = 1, PlateNumber = "AAA-111", UnitModel = "Isuzu", VehicleType = "Truck", BranchId = 1 });
            context.FuelEntries.Add(new FuelEntry
            {
                Id = 1, BranchId = 1, IsArchived = false,
                Driver = "Juan", FuelStation = "Shell", FuelType = "Diesel",
                DateTime = DateTime.Now, Liters = 10, TotalCost = 500, VehicleId = 1
            });
            await context.SaveChangesAsync();

            var controller = new FuelController(context, _auditMock.Object, _fuelPriceMock.Object, _loggerMock.Object, _textFormattingMock.Object);
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.EditFuel(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<FuelEntry>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ViewFuel
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ViewFuel")]
        public async Task ViewFuel_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateController(nameof(ViewFuel_NonExistentId_ReturnsNotFound));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ViewFuel(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("Category", "ViewFuel")]
        public async Task ViewFuel_ExistingId_ReturnsViewWithModel()
        {
            // Arrange
            var context = TestDbContextFactory.Create(nameof(ViewFuel_ExistingId_ReturnsViewWithModel));
            context.Vehicles.Add(new Vehicle { Id = 1, PlateNumber = "AAA-111", UnitModel = "Isuzu", VehicleType = "Truck", BranchId = 1 });
            context.FuelEntries.Add(new FuelEntry
            {
                Id = 5, BranchId = 1, IsArchived = false,
                Driver = "Maria", FuelStation = "Petron", FuelType = "Gasoline",
                DateTime = DateTime.Now, Liters = 15, TotalCost = 750, VehicleId = 1
            });
            await context.SaveChangesAsync();

            var controller = new FuelController(context, _auditMock.Object, _fuelPriceMock.Object, _loggerMock.Object, _textFormattingMock.Object);
            MockHttpContext.Setup(controller, MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ViewFuel(5);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<FuelEntry>(viewResult.Model);
            Assert.Equal(5, model.Id);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ArchiveFuel
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ArchiveFuel")]
        public async Task ArchiveFuel_NonExistentId_ReturnsJsonFailure()
        {
            // Arrange
            var controller = CreateController(nameof(ArchiveFuel_NonExistentId_ReturnsJsonFailure));
            controller.ControllerContext = MockHttpContext.Create(MockHttpContext.SuperAdminSession());

            // Act
            var result = await controller.ArchiveFuel(999) as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic? value = result.Value;
            Assert.NotNull(value);
            Assert.False((bool)value!.GetType().GetProperty("success")!.GetValue(value)!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // FuelEntry Model Validation — Valid Entry
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "ModelValidation")]
        public void FuelEntry_ValidEntry_PassesValidation()
        {
            // Arrange
            var entry = new FuelEntry
            {
                VehicleId = 1,
                Driver = "Juan Dela Cruz",
                DateTime = DateTime.Now,
                FuelStation = "Shell EDSA",
                Odometer = 5000,
                Liters = 30.5m,
                TotalCost = 1525m,
                FuelType = "Diesel"
            };

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(entry);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            // Act
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entry, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }
    }
}
