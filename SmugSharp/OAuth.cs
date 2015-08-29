using System;
using System.Security.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OAuth
{
    public class OAuthBase
    {

        public static Dictionary<string, string> GetOAuthParameters(string apikey, string callbackUrl = null)
        {
            var random = new Random();
            var epochDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timespan = DateTime.UtcNow - epochDate;
            var oauthTimestamp = Math.Round(timespan.TotalSeconds).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            var oauthNonce = random.Next(100000000, 999999999).ToString();

            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(callbackUrl))
            {
                parameters.Add("oauth_callback", callbackUrl);
            }
            parameters.Add("oauth_consumer_key", apikey);
            parameters.Add("oauth_nonce", oauthNonce);
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            parameters.Add("oauth_timestamp", oauthTimestamp);
            parameters.Add("oauth_version", "1.0");

            return parameters;
        }

        public static string CalculateOAuthSignedUrl(Dictionary<string, string> parameters, string oauthTokenSecret, string url, string secretKey, bool exchangeStep)
        {
            var baseString = new StringBuilder();
            var sortedParams = new SortedDictionary<string, string>();

            foreach (var param in parameters.OrderBy(p => p.Key))
            {
                sortedParams.Add(param.Key, param.Value);
            }

            foreach (var param in sortedParams)
            {
                baseString.Append(param.Key);
                baseString.Append("=");
                baseString.Append(Uri.EscapeDataString(param.Value));
                baseString.Append("&");
            }

            //removing the extra ampersand 
            baseString.Remove(baseString.Length - 1, 1);
            var baseStringForSig = "POST&" + Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(baseString.ToString());

            //calculating the signature 
            var HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");

            IBuffer keyMaterial;
            if (exchangeStep)
            {
                keyMaterial = CryptographicBuffer.ConvertStringToBinary(secretKey + "&" + oauthTokenSecret, BinaryStringEncoding.Utf8);
            }
            else
            {
                keyMaterial = CryptographicBuffer.ConvertStringToBinary(secretKey + "&", BinaryStringEncoding.Utf8);
            }

            var cryptoKey = HmacSha1Provider.CreateKey(keyMaterial);
            var dataString = CryptographicBuffer.ConvertStringToBinary(baseStringForSig, BinaryStringEncoding.Utf8);

            return url + "?" + baseString.ToString() + "&oauth_signature=" +
                Uri.EscapeDataString(CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Sign(cryptoKey, dataString)));
        }

        public static string GetSignature(string url, string data, string secretKey)
        {
            var HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);

            var keyMaterial = CryptographicBuffer.ConvertStringToBinary(secretKey + "&", BinaryStringEncoding.Utf8);
            var cryptoKey = HmacSha1Provider.CreateKey(keyMaterial);

            var baseStringForSig = "GET&" + Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(data);
            var dataString = CryptographicBuffer.ConvertStringToBinary(baseStringForSig, BinaryStringEncoding.Utf8);

            return Uri.EscapeDataString(CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Sign(cryptoKey, dataString)));
        }

        public async static Task<string> GetResponseFromWeb(string url)
        {
            var Request = (HttpWebRequest)WebRequest.Create(url);
            Request.Method = "POST";

            var response = (HttpWebResponse)await Request.GetResponseAsync();

            string httpResponse = null;
            if (response != null)
            {
                var data = new StreamReader(response.GetResponseStream());
                httpResponse = await data.ReadToEndAsync();
            }
            return httpResponse;
        }
    }

}