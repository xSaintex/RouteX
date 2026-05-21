using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace RouteX.Tests.Helpers
{
    /// <summary>
    /// Provides a real DefaultHttpContext with a real session for controller tests.
    /// This avoids issues with mock session byte encoding that breaks GetString().
    /// </summary>
    public static class MockHttpContext
    {
        /// <summary>
        /// Applies a real HttpContext with a real in-memory session to the controller,
        /// and wires up TempData so controllers that use TempData don't throw NullReferenceException.
        /// </summary>
        public static void Setup(Microsoft.AspNetCore.Mvc.Controller controller, Dictionary<string, string> sessionValues)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(sessionValues);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());
        }

        /// <summary>
        /// Creates a ControllerContext only (no TempData). Use Setup() instead when the action uses TempData.
        /// </summary>
        public static ControllerContext Create(Dictionary<string, string> sessionValues)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(sessionValues);
            return new ControllerContext { HttpContext = httpContext };
        }

        public static Dictionary<string, string> SuperAdminSession(string email = "superadmin@routex.com")
            => new()
            {
                ["UserEmail"] = email,
                ["UserRole"] = "SuperAdmin"
            };

        public static Dictionary<string, string> AdminSession(string email = "admin@routex.com", int branchId = 1)
            => new()
            {
                ["UserEmail"] = email,
                ["UserRole"] = "Admin",
                ["UserBranchId"] = branchId.ToString()
            };

        public static Dictionary<string, string> FinanceSession(string email = "finance@routex.com", int branchId = 1)
            => new()
            {
                ["UserEmail"] = email,
                ["UserRole"] = "Finance",
                ["UserBranchId"] = branchId.ToString()
            };

        public static Dictionary<string, string> OperationsStaffSession(string email = "ops@routex.com", int branchId = 1)
            => new()
            {
                ["UserEmail"] = email,
                ["UserRole"] = "OperationsStaff",
                ["UserBranchId"] = branchId.ToString()
            };
    }

    /// <summary>
    /// A simple in-memory ISession implementation that correctly supports GetString/SetString.
    /// </summary>
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public TestSession(Dictionary<string, string> initialValues)
        {
            foreach (var kv in initialValues)
                Set(kv.Key, System.Text.Encoding.UTF8.GetBytes(kv.Value));
        }

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_store.TryGetValue(key, out var stored))
            {
                value = stored;
                return true;
            }
            value = Array.Empty<byte>();
            return false;
        }
    }
}
