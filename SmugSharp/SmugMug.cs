using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAuth;
using SmugSharp.Models;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

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
    /// Documentation: https://api.smugmug.com/api/v2
    /// </summary>
    public class SmugMug
    {
        public static SmugMug Instance { get; private set; }

        /// <summary>
        /// The base url of the SmugMug v2 api.
        /// </summary>
        public const string BaseApiUrl = "https://api.smugmug.com/api/v2";
        public const string BaseUrl = "https://api.smugmug.com";

        public const string RequestTokenUrl = "https://secure.smugmug.com/services/oauth/1.0a/getRequestToken";
        public const string UserAuthorizationUrl = "https://secure.smugmug.com/services/oauth/1.0a/authorize";
        public const string AccessTokenUrl = " https://secure.smugmug.com/services/oauth/1.0a/getAccessToken";

        /// <summary>
        /// The consuming application's api key.
        /// </summary>
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public string CallbackUrl { get; set; }

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

        private string RequestToken;
        private string RequestTokenSecret;

        public User CurrentUser { get; set; }

        /// <summary>
        /// The ctor to use in testing or situations where you already have the auth token and secret.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenSecret"></param>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="callbackUrl"></param>
        public SmugMug(string token, string tokenSecret, string consumerKey, string consumerSecret, string callbackUrl)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            AccessToken = token;
            AccessTokenSecret = tokenSecret;
            CallbackUrl = callbackUrl;
            InitializeDefaults();

            Instance = this;
        }

        /// <summary>
        /// The ctor to use when the user will need to authenticate.
        /// </summary>
        /// <param name="ConsumerKey"></param>
        /// <param name="apiSecret"></param>
        /// <param name="callbackUrl"></param>
        public SmugMug(string consumerKey, string consumerSecret, string callbackUrl)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            CallbackUrl = callbackUrl;
            InitializeDefaults();

            Instance = this;
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
        /// Makes a request to the API using the ConsumerKey signed with the ApiSecret
        /// to get a request token to initiate the OAuth process.
        /// </summary>
        /// <returns>A valid OAuth token.</returns>
        public async Task<string> GetRequestToken()
        {
            if (string.IsNullOrWhiteSpace(ConsumerKey))
            {
                throw new ArgumentException("ConsumerKey must be set.");
            }

            if (string.IsNullOrWhiteSpace(RequestToken))
            {
                var client = OAuthRequest.ForRequestToken(Instance.ConsumerKey, Instance.ConsumerSecret);
                client.CallbackUrl = Instance.CallbackUrl;
                client.RequestUrl = RequestTokenUrl;

                var auth = client.GetAuthorizationQuery();
                var url = $"{client.RequestUrl}?{auth}";

                var response = await GetResponseForRequest(url);

                ParseTokenResult(response);
            }

            return RequestToken;
        }

        public async void GetAccessToken(string authorizationResult)
        {
            var verifier = ParseAuthorizationResult(authorizationResult);

            if (!string.IsNullOrWhiteSpace(verifier))
            {
                var client = OAuthRequest.ForAccessToken(Instance.ConsumerKey, Instance.ConsumerSecret, Instance.RequestToken, Instance.RequestTokenSecret);
                client.Verifier = verifier;
                client.RequestUrl = AccessTokenUrl;

                var auth = client.GetAuthorizationQuery();
                var url = $"{client.RequestUrl}?{auth}";

                var extraHeader = new Dictionary<string, string>();
                var verifierKvp = new KeyValuePair<string, string>("oauth_verifier", verifier);
                extraHeader.Add(verifierKvp.Key, verifierKvp.Value);

                var response = await GetResponseForProtectedRequest(url, "POST", extraHeader, $"{verifierKvp.Key}={verifierKvp.Value}");

                ParseTokenResult(response, isAccessToken: true);
            }
        }

        /// <summary>
        /// Parses the request token response and splits out the token and secret.
        /// </summary>
        /// <param name="response">The response from the API.</param>
        private void ParseTokenResult(string response, bool isAccessToken = false)
        {
            var keyValPairs = response.Split('&');
            for (int i = 0; i < keyValPairs.Length; i++)
            {
                var splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        {
                            if (isAccessToken)
                            {
                                AccessToken = splits[1];
                            }
                            else
                            {
                                RequestToken = splits[1];
                            }
                            break;
                        }
                    case "oauth_token_secret":
                        {

                            if (isAccessToken)
                            {
                                AccessTokenSecret = splits[1];
                            }
                            else
                            {
                                RequestTokenSecret = splits[1];
                            }
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Generates the proper authorization url to which the user should be directed.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAuthorizationUrl()
        {
            var allowThirdPartyLogin = AllowThirdPartyLogin ? 1 : 0;
            var oauthToken = await GetRequestToken();

            return $"{UserAuthorizationUrl}" +
                $"?oauth_token={oauthToken}" +
                $"&Access={AccessLevel}&Permissions={PermissionsLevel}" +
                $"&allowThirdPartyLogin={allowThirdPartyLogin}&showSignUpButton={ShowSignUpButton}&username={DefaultUsername}&viewportScale={ViewportScale}";
        }

        public async Task<User> GetCurrentUser()
        {
            if (CurrentUser == null)
            {
                var authUserUrl = $"{BaseApiUrl}!authuser";
                var response = await GetResponseForProtectedRequest(authUserUrl);

                CurrentUser = User.FromJson(response);
            }

            return CurrentUser;
        }

        /// <summary>
        /// Performs an authenticated request to a given url.
        /// </summary>
        /// <param name="url">The destination url</param>
        /// <returns>The response from the request</returns>
        /// <remarks>Will likely go away in the future.</remarks>
        public static async Task<string> GetResponseForProtectedRequest(string url, string method = "GET", Dictionary<string, string> extraHeaders = null, string postContent = null, Dictionary<string, string> extraParams = null)
        {
            var client = OAuthRequest.ForProtectedResource(method, Instance.ConsumerKey, Instance.ConsumerSecret, Instance.AccessToken, Instance.AccessTokenSecret);
            client.RequestUrl = url;

            var request = new HttpClient();
            if (extraParams != null)
            {
                var authedUrl = client.GetAuthorizationQuery(extraParams);
                client.RequestUrl = $"{url}?{authedUrl}";
            } 

            var headers = client.GetAuthorizationHeader();  
            request.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "OAuth",
                    headers);    

            request.DefaultRequestHeaders.Add("Accept", "application/json");
            if (method == "POST")
            {
                request.DefaultRequestHeaders.Add("Content-Type", "application/json");
            }

            var response = method == "GET" ? await request.GetAsync(client.RequestUrl) :
                           method == "POST" ? await request.PostAsync(client.RequestUrl, new StringContent(postContent ?? string.Empty)) : null;

            string httpResponse = null;
            if (response != null)
            {
                httpResponse = await response.Content.ReadAsStringAsync();
            }
            return httpResponse;
        }

        /// <summary>
        /// Does a POST to a url and returns the response.
        /// </summary>
        /// <param name="url">The destination url.</param>
        /// <returns>The response from the endpoint.</returns>
        /// <remarks>Will likely go away in future releases.</remarks>
        private async static Task<string> GetResponseForRequest(string url, string method = "GET")
        {
            var Request = (HttpWebRequest)WebRequest.Create(url);
            Request.Method = method;

            var response = (HttpWebResponse)await Request.GetResponseAsync();

            string httpResponse = null;
            if (response != null)
            {
                var data = new StreamReader(response.GetResponseStream());
                httpResponse = await data.ReadToEndAsync();
            }
            return httpResponse;
        }

        private string ParseAuthorizationResult(string authData)
        {
            if (string.IsNullOrWhiteSpace(authData))
            {
                return null;
            }

            var identifier = "oauth_verifier=";
            var index = authData.IndexOf(identifier) + identifier.Length;
            var verifier = authData.Substring(index, 6);

            return verifier;
        }
    }
}
