using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SmugSharp;

namespace SmugSharpTest
{
    [TestClass]
    public class UnitTest1
    {
        private string ApiKey { get { return Configuration.Authentication.ApiKey; } }
        private string ApiSecret { get { return Configuration.Authentication.ApiSecret; } }
        private string CallbackUrl { get { return Configuration.Authentication.CallbackUrl; } }
        private string AccessToken { get { return Configuration.Authentication.AccessToken; } }
        private string AccessTokenSecret { get { return Configuration.Authentication.AccessTokenSecret; } }


        [TestMethod]
        public void TestSmugMugCtorWorks()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);

            Assert.IsNotNull(smugmug, "SmugMug should not be null after constructing.");
            Assert.IsNotNull(smugmug.Authentication, "SmugMug Authentication should not be null.");
            Assert.AreEqual(ApiKey, smugmug.ApiKey, "API key should be kept.");
        }

        [TestMethod]
        public async Task TestGetResponseWithHeaders()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var authUserUrl = $"{SmugMug.BaseApiUrl}!authuser";
            var response = await smugmug.GetResponseWithHeaders(authUserUrl);

            Assert.IsFalse(response.Contains("\"Code\":4"));
        }

        [TestMethod]
        public async Task TestGetPublicResponse()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var authUserUrl = $"{SmugMug.BaseApiUrl}/user/cmac";
            var response = await smugmug.GetResponseWithHeaders(authUserUrl);

            Assert.IsFalse(response.Contains("\"Code\":4"));
        }
    }
}
