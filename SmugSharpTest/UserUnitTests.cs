using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SmugSharp;
using SmugSharpTest.Configuration;

namespace SmugSharpTest
{
    [TestClass]
    public class UserUnitTests : UnitTestsBase
    {               
        [TestMethod]
        public async Task TestGetCurrentUserBioImageNotNull()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var user = await smugmug.GetCurrentUser();
            var bioImage = await user.GetBioImage();

            Assert.IsNotNull(bioImage);
        }

        [TestMethod]
        public async Task TestGetCurrentUserRootNodeNotNull()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var user = await smugmug.GetCurrentUser();
            var node = await user.GetRootNode();

            Assert.IsNotNull(node);
        }
    }
}
