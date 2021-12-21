using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas
{
    public record DigestChallengeRequest(string? Realm, string? Domain, string? Nonce, string? Algorithm, string? Qop,
        string? Stale, uint NonceCount, string ClientNonce, DateTime ExpiresAt)
    {
        private static readonly MD5 MD5 = MD5.Create();

        private static readonly Regex DigestChallengeRequestRegex =
            new("(?<key>\\w+)[:=](?<value>[\\s\"]?(([^\",]|(\\\"))+))\"?", RegexOptions.IgnoreCase);

        public bool IsUsable => ExpiresAt > DateTime.UtcNow;

        public const int MinClientNonce = 0x100000;
        public const int MaxClientNonce = 0xFFFFFF;

        private static string Hash(string input)
        {
            return BitConverter.ToString(MD5.ComputeHash(Encoding.ASCII.GetBytes(input))).Replace("-", "")
                .ToLowerInvariant();
        }

        private static string NewClientNonce()
        {
            return new Random().Next(MinClientNonce, MaxClientNonce).ToString("X6");
        }

        public DigestChallengeRequest Refresh()
        {
            return this with { NonceCount = NonceCount + 1, ClientNonce = NewClientNonce() };
        }

        public AuthenticationHeaderValue GetResponseHeader(string username, string password, string method,
            string digestUri)
        {
            var hash1 = Hash($"{username}:{Realm}:{password}");
            var hash2 = Hash($"{method}:{digestUri}");

            var response = Hash($"{hash1}:{Nonce}:{NonceCount:D8}:{ClientNonce}:{Qop}:{hash2}");

            return AuthenticationHeaderValue.Parse(
                $"Digest username=\"{username}\", " +
                $"realm=\"{Realm}\", " +
                $"nonce=\"{Nonce}\", " +
                $"uri=\"{digestUri}\", " +
                $"algorithm={Algorithm}, " +
                $"qop={Qop}, " +
                $"nc={NonceCount:D8}, " +
                $"cnonce=\"{ClientNonce}\", " +
                $"response=\"{response}\"");
        }

        public static bool TryParse(AuthenticationHeaderValue authorizationHeaderValue,
            out DigestChallengeRequest digestChallengeRequest)
        {
            digestChallengeRequest = default!;

            if (authorizationHeaderValue.Scheme != "Digest" || authorizationHeaderValue.Parameter == null)
            {
                return false;
            }

            try
            {
                var parts = DigestChallengeRequestRegex
                    .Matches(authorizationHeaderValue.Parameter)
                    .ToDictionary
                    (
                        match => match.Groups["key"].Value,
                        match =>
                        {
                            var value = match.Groups["value"].Value;

                            if (value.StartsWith("\""))
                            {
                                return JsonSerializer.Deserialize<string>(value);
                            }

                            return value;
                        }
                    );

                var realm = parts["realm"];
                var domain = parts["domain"];
                var nonce = parts["nonce"];
                var algorithm = parts["algorithm"];
                var qop = parts["qop"];
                var stale = parts["stale"];

                digestChallengeRequest = new DigestChallengeRequest(realm, domain, nonce, algorithm, qop, stale, 1,
                    NewClientNonce(), DateTime.UtcNow.AddHours(1));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
