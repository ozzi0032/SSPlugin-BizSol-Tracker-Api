using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BizSol.Tracker.Api.Providers
{
    public class AppAuthProvider
    {
        public static string GetAuthorizationToken(string publicKey, string secretKey, string contentMd5Hash, string method,
            string url)
        {
            string accept = "application/json, text/javascript, */*";
            var timestamp = DateTime.UtcNow.ToString("o");
            var uri = new Uri(url);     // decode url

            if (uri.Query != null && uri.Query.Length > 0)
            {
                url = string.Concat(uri.GetLeftPart(UriPartial.Path), HttpUtility.UrlDecode(uri.Query));
            }


            var messageRepresentation = string.Join("\n",
                method.ToLower(),
                contentMd5Hash ?? "",
                accept.ToLower(),
                url.ToLower(),
                timestamp,
                publicKey.ToLower()
            );
            var signature = CreateSignature(secretKey, messageRepresentation);
            return signature;
        }

        public static string CreateSignature(string secretKey, string messageRepresentation)
        {
            if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(messageRepresentation))
                return "";

            string signature;
            var secretBytes = Encoding.UTF8.GetBytes(secretKey);
            var valueBytes = Encoding.UTF8.GetBytes(messageRepresentation);

            using (var hmac = new HMACSHA256(secretBytes))
            {
                var hash = hmac.ComputeHash(valueBytes);
                signature = Convert.ToBase64String(hash);
            }
            return signature;
        }

        public static string CreateContentMd5Hash(byte[] content)
        {
            string result = "";
            if (content != null && content.Length > 0)
            {
                using (var md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(content);
                    result = Convert.ToBase64String(hash);
                }
            }
            return result;
        }
    }
}