using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SmugSharp;
using SmugSharpTest.Configuration;

namespace SmugSharpTest
{
    [TestClass]
    public class SmugMugUnitTests : UnitTestsBase
    {
        [TestMethod]
        public void TestSmugMugCtorWorks()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);

            Assert.IsNotNull(smugmug, "SmugMug should not be null after constructing.");
            Assert.AreEqual(ApiKey, smugmug.ConsumerKey, "API key should be kept.");
        }

        [TestMethod]
        public async Task TestGetResponseWithHeaders()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var authUserUrl = $"{SmugMug.BaseApiUrl}!authuser";
            var response = await SmugMug.GetResponseForProtectedRequest(authUserUrl);

            Assert.IsFalse(response.Contains("\"Code\":4"));
        }

        [TestMethod]
        public async Task TestGetPublicResponse()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var authUserUrl = $"{SmugMug.BaseApiUrl}/user/cmac";
            var response = await SmugMug.GetResponseForProtectedRequest(authUserUrl);

            Assert.IsFalse(response.Contains("\"Code\":4"));
        }

        [TestMethod]
        public async Task TestGetCurrentUserNotNull()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var user = await smugmug.GetCurrentUser();

            Assert.IsNotNull(user);
        }  
    }
}
