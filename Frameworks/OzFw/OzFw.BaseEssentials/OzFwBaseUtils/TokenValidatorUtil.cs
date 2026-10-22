using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace OzFwBaseUtils
{
    public class TokenValidator
    {
        private static readonly string STATIC_SECRET_KEY = "nLOdcEqWEsI8cZzD6R03Jq837PpyiyAQ";

        public static bool VerifyToken(string signedToken)
        {
            string[] parts = signedToken.Split('.');
            if (parts.Length != 3) return false;
            string prefix = parts[0];
            string encodedPayload = parts[1];
            string receivedSignature = parts[2];
            string originalToken = $"{prefix}.{encodedPayload}";
            string expectedSignature = Sign(originalToken, STATIC_SECRET_KEY);
            return receivedSignature.Equals(expectedSignature);
        }

        public static Dictionary<string, object> DecodePayload(string signedToken)
        {
            string[] parts = signedToken.Split('.');
            if (parts.Length != 3) throw new ArgumentException("Invalid token format");
            string encodedPayload = parts[1];
            string payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload));
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJson) ?? new Dictionary<string, object>();
        }

        private static string Sign(string data, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(signatureBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static string? GetClientFromUrl(string url)
        {
            string[] partsUrl = url.Split('/');
            string token = partsUrl.Last();
            Dictionary<string, object>? map = null;
            string? clientId = null;
            try
            {
                map = DecodePayload(token);
                if (map != null && map.ContainsKey("client_id"))
                {
                    clientId = map["client_id"]?.ToString();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error decoding payload", e);
            }
            return clientId;
        }


        public bool Durum(long grup) { return true; }

    }
}
