using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SmugSharp;
using SmugSharpTest.Configuration;

namespace SmugSharpTest
{
    [TestClass]
    public class AuthenticationUnitTests : UnitTestsBase
    {                                              
        [TestMethod]
        public async Task TestGetRequestTokenForApiKey()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);

            var requestToken = await smugmug.GetRequestToken();

            Assert.IsNotNull(requestToken, "Request token should be not null.");

            var cachedRT = await smugmug.GetRequestToken();

            Assert.AreEqual(requestToken, cachedRT);
        }

        [TestMethod]
        public async Task TestAuthorizationUrl()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);
            var requestToken = await smugmug.GetRequestToken();

            var authUrl = await smugmug.GetAuthorizationUrl();

            Assert.IsNotNull(authUrl);
            Assert.IsTrue(authUrl.Contains("oauth_token"));

            Assert.IsTrue(authUrl.StartsWith("https"), "Authorization url must be secure.");
        }

        [TestMethod]
        public void TestViewportScale()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);
            var min = 0.0;
            var max = 1.0;
            var over = 100.0;
            var under = -100.0;

            Assert.AreEqual(smugmug.ViewportScale, min, "Viewport should default to 0.0");

            smugmug.ViewportScale = max;           
            Assert.AreEqual(smugmug.ViewportScale, max);

            smugmug.ViewportScale = over;
            Assert.AreEqual(smugmug.ViewportScale, max);

            smugmug.ViewportScale = under;
            Assert.AreEqual(smugmug.ViewportScale, min);
        }
    }
}
