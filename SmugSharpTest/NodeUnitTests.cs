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
    public class NodeUnitTests : UnitTestsBase
    {
        [TestMethod]
        public async Task TestRootNodeHasChildren()
        {
            var smugmug = new SmugMug(AccessToken, AccessTokenSecret, ApiKey, ApiSecret, CallbackUrl);

            var user = await smugmug.GetCurrentUser();
            var node = await user.GetRootNode();

            Assert.IsTrue(node.HasChildren);

            Assert.IsTrue(node.Children.Count > 0);
        }
    }
}
