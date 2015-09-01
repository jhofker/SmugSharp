using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmugSharp
{
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

        public enum Access { Full, Public }
        public enum Permissions { Read, Add, Modify }

        public Access AccessLevel { get; private set; }
        public Permissions PermissionsLevel { get; private set; }

        public bool AllowThirdPartyLogin { get; private set; }
        public bool ShowSignUpButton { get; private set; }
        public string DefaultUsername { get; private set; }

        private double viewportScale;
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

        public Dictionary<string, string> GetAuthHeaders(string url)
        {
            var parameters = OAuth.OAuthBase.GetOAuthParameters(ApiKey);
            parameters.Add("oauth_token", OAuthToken);

            var signature = OAuth.OAuthBase.GetSignature(
                url,
                string.Join("&", parameters.OrderBy(h => h.Key).Select(h => $"{h.Key}={h.Value}")),
                ApiSecret,
                OAuthTokenSecret);
            parameters.Add("oauth_signature", signature);

            return parameters;
        }

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
