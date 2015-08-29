using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SmugSharp;
using Windows.Security.Authentication.Web;

namespace SmugSharpTest
{
    [TestClass]
    public class UnitTest1
    {
        private string ApiKey { get { return TestAuthentication.ApiKey; } }
        private string ApiSecret { get { return TestAuthentication.ApiSecret; } }
        private string CallbackUrl { get { return TestAuthentication.CallbackUrl; } }        
        private string AccessToken { get { return TestAuthentication.AccessToken; } }
        private string AccessTokenSecret { get { return TestAuthentication.AccessTokenSecret; } }


        [TestMethod]
        public void TestSmugMugCtorWorks()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);

            Assert.IsNotNull(smugmug, "SmugMug should not be null after constructing.");
            Assert.IsNotNull(smugmug.Authentication, "SmugMug Authentication should not be null.");
            Assert.AreEqual(ApiKey, smugmug.ApiKey, "API key should be kept.");
        }

        [TestMethod]
        public async Task TestGetRequestTokenForApiKey()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);

            var requestToken = await smugmug.Authentication.GetRequestToken();

            Assert.IsNotNull(requestToken, "Request token should be not null.");

            var cachedRT = await smugmug.Authentication.GetRequestToken();

            Assert.AreEqual(requestToken, cachedRT);
        }

        [TestMethod]
        public async Task TestAuthorizationUrl()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);
            var requestToken = await smugmug.Authentication.GetRequestToken();

            var authUrl = smugmug.Authentication.GetAuthorizationUrl();

            Assert.IsNotNull(authUrl);

            Assert.IsTrue(authUrl.StartsWith("https"), "Authorization url must be secure.");
        }

        [TestMethod]
        public async Task TestAuthorization()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var authUserUrl = $"{SmugMug.BaseApiUrl}!authuser";
            var response = await smugmug.GetResponseWithHeaders(authUserUrl);

            Assert.IsFalse(response.Contains("\"Code\":4"));
        }
    }
}
