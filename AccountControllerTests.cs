using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Contract_Claim_System.Controllers;

namespace Contract_Claim_System.Tests
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<HttpContext> _mockHttpContext;
        private Mock<ISession> _mockSession;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext.Setup(s => s.Session).Returns(_mockSession.Object);

            _controller = new AccountController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public void Login_Get_ReturnsView()
        {
            
            IActionResult result = _controller.Login();

            
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void Login_Post_ValidUser_RedirectsToDashboard()
        {
            
            string username = "Peace";
            string password = "Peace@123";
            string role = "Lecturer";

           
            IActionResult result = _controller.Login(username, password, role);

           
            Assert.IsNotNull(result); // Ensures result is not null
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult); // Ensures result is of correct type
            Assert.AreEqual("LecturerDashboard", redirectResult?.ActionName);
            Assert.AreEqual("Claims", redirectResult?.ControllerName);
        }

        [Test]
        public void Login_Post_InvalidUser_ShowsErrorMessage()
        {
            
            string username = "InvalidUser";
            string password = "InvalidPassword";
            string role = "InvalidRole";

            
            IActionResult result = _controller.Login(username, password, role);

            
            Assert.IsNotNull(result); // Ensures result is not null
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult); // Ensures result is of correct type
            Assert.AreEqual("Invalid username, password, or role. Please try again.", _controller.ViewBag.ErrorMessage);
        }

        [Test]
        public void Logout_Post_ClearsSessionAndRedirects()
        {
          
            IActionResult result = _controller.Logout();

            
            Assert.IsNotNull(result); // Ensures result is not null
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult); // Ensures result is of correct type
            Assert.AreEqual("Login", redirectResult?.ActionName);
            _mockSession.Verify(s => s.Clear(), Times.Once);
        }

    }
}
