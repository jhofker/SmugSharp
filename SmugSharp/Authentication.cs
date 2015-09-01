using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmugSharp
{
    /// <summary>
    /// Defines the level of data the app has access to.
    /// </summary>
    public enum Access { Full, Public }
    /// <summary>
    /// What the app can do with the data.
    /// </summary>
    public enum Permissions { Read, Add, Modify }

    /// <summary>
    /// 
    /// Documentation: https://api.smugmug.com/api/v2/doc/tutorial/authorization.html
    /// </summary>
    public class Authentication
    {
        private string ApiKey;
        private string ApiSecret;
        private string CallbackUrl;
        private string OAuthToken;
        private string OAuthTokenSecret;

        /// <summary>
        /// The level of data the app has access to.
        /// </summary>
        public Access AccessLevel { get; private set; }

        /// <summary>
        /// What the app can do with the data.
        /// </summary>
        public Permissions PermissionsLevel { get; private set; }

        /// <summary>
        /// When authenticating with OAuth, should the user be allowed
        /// to login to SmugMug with third-party logins like Facebook.
        /// Note: This is translated to "0" or "1" when used in the url.
        /// Default: false
        /// </summary>
        public bool AllowThirdPartyLogin { get; private set; }

        /// <summary>
        /// When authenticating with OAuth, should the user be shown
        /// a sign-up button for SmugMug.
        /// Default: true
        /// </summary>
        public bool ShowSignUpButton { get; private set; }

        /// <summary>
        /// When authenticating with OAuth, the username to pre-populate
        /// the email/nickname field with.
        /// Default: string.Empty
        /// </summary>
        public string DefaultUsername { get; private set; }

        private double viewportScale;
        /// <summary>
        /// The scale factor of the login page for mobile devices
        /// to inject into the viewport meta tag.
        /// Note: Constrained to between 0.0 and 1.0
        /// Default: 0.0
        /// </summary>
        public double ViewportScale
        {
            get
            {
                return viewportScale;
            }
            set
            {
                if (viewportScale != value)
                {
                    if (value > 1.0)
                    {
                        viewportScale = 1.0;
                    }
                    else if (value < 0)
                    {
                        viewportScale = 0;
                    }
                    else
                    {
                        viewportScale = value;
                    }
                }
            }
        }

        public const string RequestTokenUrl = "https://api.smugmug.com/services/oauth/1.0a/getRequestToken";
        public const string UserAuthorizationUrl = "https://api.smugmug.com/services/oauth/1.0a/authorize";
        public const string AccessTokenUrl = " https://api.smugmug.com/services/oauth/1.0a/getAccessToken";

        public Authentication(string authToken, string authSecret, string apiKey, string apiSecret, string callbackUrl)
        {
            OAuthToken = authToken;
            OAuthTokenSecret = authSecret;

            ApiKey = apiKey;
            ApiSecret = apiSecret;
            CallbackUrl = callbackUrl;

            InitializeDefaults();
        }

        public Authentication(string apiKey, string apiSecret, string callbackUrl)
        {
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            CallbackUrl = callbackUrl;

            InitializeDefaults();
        }

        /// <summary>
        /// Initializes default values per the API documentation.
        /// </summary>
        private void InitializeDefaults()
        {
            // Defaults via the documentation
            AccessLevel = Access.Public;
            PermissionsLevel = Permissions.Read;
            AllowThirdPartyLogin = false;
            ShowSignUpButton = true;
            DefaultUsername = string.Empty;
            ViewportScale = 0;
        }

        /// <summary>
        /// Generates the proper authorization url to which the user should be directed.
        /// </summary>
        /// <returns></returns>
        public string GetAuthorizationUrl()
        {
            var allowThirdPartyLogin = AllowThirdPartyLogin ? 1 : 0;

            return $"{UserAuthorizationUrl}" +
                $"?Access={AccessLevel}&Permissions={PermissionsLevel}" +
                $"&allowThirdPartyLogin={allowThirdPartyLogin}&showSignUpButton={ShowSignUpButton}&username={DefaultUsername}&viewportScale={ViewportScale}";
        }

        /// <summary>
        /// Makes a request to the API using the ApiKey signed with the ApiSecret
        /// to get a request token to initiate the OAuth process.
        /// </summary>
        /// <returns>A valid OAuth token.</returns>
        public async Task<string> GetRequestToken()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new ArgumentException("ApiKey must be set.");
            }

            if (string.IsNullOrWhiteSpace(OAuthToken))
            {                                   
                var parameters = OAuth.OAuthBase.GetOAuthParameters(ApiKey, CallbackUrl);
                var url = OAuth.OAuthBase.CalculateOAuthSignedUrl(parameters, null, Authentication.RequestTokenUrl, ApiSecret, false);

                var response = await OAuth.OAuthBase.GetResponseFromWeb(url);

                ParseRequestToken(response);
            }

            return OAuthToken;
        }

        /// <summary>
        /// Creates authentication headers based on the current OAuth state.
        /// </summary>
        /// <param name="url">The URL to generate headers (and signature) to.</param>
        /// <returns>Key-value pairs representing the oauth pairs in the Authorization header.</returns>
        public Dictionary<string, string> GetAuthHeaders(string method, string url)
        {
            var parameters = OAuth.OAuthBase.GetOAuthParameters(ApiKey);
            parameters.Add("oauth_token", OAuthToken);

            var signature = OAuth.OAuthBase.GetSignature(
                method,
                url,
                string.Join("&", parameters.OrderBy(h => h.Key).Select(h => $"{h.Key}={h.Value}")),
                ApiSecret,
                OAuthTokenSecret);
            parameters.Add("oauth_signature", signature);

            return parameters;
        }

        /// <summary>
        /// Parses the request token response and splits out the token and secret.
        /// </summary>
        /// <param name="response">The response from the API.</param>
        private void ParseRequestToken(string response)
        {
            var keyValPairs = response.Split('&');
            for (int i = 0; i < keyValPairs.Length; i++)
            {
                var splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        {
                            OAuthToken = splits[1];
                            break;
                        }
                    case "oauth_token_secret":
                        {
                            OAuthTokenSecret = splits[1];
                            break;
                        }
                }
            }
        }
    }
}
