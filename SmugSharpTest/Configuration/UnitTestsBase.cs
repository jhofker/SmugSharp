using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmugSharpTest.Configuration
{
    public class UnitTestsBase
    {
        protected string ApiKey { get { return Configuration.Authentication.ApiKey; } }
        protected string ApiSecret { get { return Configuration.Authentication.ApiSecret; } }
        protected string CallbackUrl { get { return Configuration.Authentication.CallbackUrl; } }
        protected string AccessToken { get { return Configuration.Authentication.AccessToken; } }
        protected string AccessTokenSecret { get { return Configuration.Authentication.AccessTokenSecret; } }
    }
}
