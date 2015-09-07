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
        public void TestViewportScale()
        {
            var smugmug = new SmugMug(ApiKey, ApiSecret, CallbackUrl);
            var min = 0.0;
            var max = 1.0;
            var over = 100.0;
            var under = -100.0;

            Assert.AreEqual(smugmug.Authentication.ViewportScale, min, "Viewport should default to 0.0");

            smugmug.Authentication.ViewportScale = max;           
            Assert.AreEqual(smugmug.Authentication.ViewportScale, max);

            smugmug.Authentication.ViewportScale = over;
            Assert.AreEqual(smugmug.Authentication.ViewportScale, max);

            smugmug.Authentication.ViewportScale = under;
            Assert.AreEqual(smugmug.Authentication.ViewportScale, min);
        }
    }
}
