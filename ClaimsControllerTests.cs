using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Contract_Claim_System.Controllers;
using Contract_Claim_System.ViewModels;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Contract_Claim_System.Tests
{
    [TestFixture]
    public class ClaimsControllerTests
    {
        private ClaimsController _controller;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void Setup()
        {
            _httpContext = new DefaultHttpContext();
            var session = new MockHttpSession();
            session.SetString("UserRole", "Lecturer");
            session.SetString("Username", "Peace");
            _httpContext.Session = session;

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);

            var tempData = new Mock<ITempDataDictionary>();
            tempData.Setup(t => t["SuccessMessage"]).Returns("Congrats, its a success!!");
            _controller = new ClaimsController(httpContextAccessorMock.Object)
            {
                TempData = tempData.Object
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller is IDisposable disposableController)
            {
                disposableController.Dispose();
            }
            var claimsField = typeof(ClaimsController).GetField("_claims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            claimsField.SetValue(null, new List<ClaimsViewModel>());
        }

        [Test]
        public void LecturerDashboard_WhenUserIsLecturer_ReturnsViewWithClaims()
        {
            _controller.TempData["SuccessMessage"] = "Congrats, its a success!!";
            var claim = new ClaimsViewModel { ClaimID = 1, LecturerName = "Peace" };
            var claimsField = typeof(ClaimsController).GetField("_claims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            claimsField.SetValue(null, new List<ClaimsViewModel> { claim });

            var result = _controller.LecturerDashboard() as ViewResult;

            Assert.IsNotNull(result, "Result is null. Check if CheckUserRole is causing a redirect.");
            Assert.AreEqual("Congrats, its a success!!", _controller.ViewBag.SuccessMessage, "SuccessMessage not correctly set in ViewBag.");
            Assert.IsInstanceOf<IEnumerable<ClaimsViewModel>>(result.Model, "Model is not of the expected type.");
            var model = result.Model as IEnumerable<ClaimsViewModel>;
            Assert.IsNotNull(model, "Model is null. Expected a list of ClaimsViewModel.");
            Assert.AreEqual(1, model.Count(), "Expected one claim in the model.");
            Assert.AreEqual("Peace", model.First().LecturerName, "LecturerName in the model does not match.");
        }
    }

    // Custom mock session class
    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => "MockSessionId";

        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);
    }
}
