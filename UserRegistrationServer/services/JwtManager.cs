using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UserRegistrationServer.services
{
    public static class JwtManager
    {
        private static string _secretKey = string.Empty;

        public static void Init(IConfiguration config)
        {
            _secretKey = config["Jwt:SecretKey"]
                ?? throw new Exception("JWT secret key not set in configuration!");
        }

        public static string SecretKey => _secretKey!;

        public static string GenerateToken(string email, int expiresMinutes = 60)
        {
            var header = Base64UrlEncode(JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" }));
            var payload = Base64UrlEncode(JsonSerializer.Serialize(new
            {
                email,
                exp = DateTimeOffset.UtcNow.AddMinutes(expiresMinutes).ToUnixTimeSeconds()
            }));

            var signature = HMACSHA256($"{header}.{payload}", _secretKey);
            return $"{header}.{payload}.{signature}";
        }

        public static string? ValidateToken(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return null;

                var header = parts[0];
                var payload = parts[1];
                var signature = parts[2];

                var computedSig = HMACSHA256($"{header}.{payload}", _secretKey);
                if (signature != computedSig) return null;

                var jsonPayload = Encoding.UTF8.GetString(Base64UrlDecode(payload));
                var doc = JsonDocument.Parse(jsonPayload);
                var exp = doc.RootElement.GetProperty("exp").GetInt64();
                var email = doc.RootElement.GetProperty("email").GetString();

                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp) return null;

                return email;
            }
            catch
            {
                return null;
            }
        }

        private static string HMACSHA256(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using var hmac = new HMACSHA256(keyBytes);
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = hmac.ComputeHash(bytes);
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(object input)
        {
            byte[] bytes = input is string s ? Encoding.UTF8.GetBytes(s) : (byte[])input;
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            input = input.Replace('-', '+').Replace('_', '/');
            switch (input.Length % 4)
            {
                case 2: input += "=="; break;
                case 3: input += "="; break;
            }
            return Convert.FromBase64String(input);
        }
    }
}
