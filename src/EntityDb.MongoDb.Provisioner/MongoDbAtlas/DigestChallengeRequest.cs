using System;
using System.Collections.Generic;
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

        private static string Hash(string input)
        {
            return BitConverter.ToString(MD5.ComputeHash(Encoding.ASCII.GetBytes(input))).Replace("-", "")
                .ToLowerInvariant();
        }

        private static string NewClientNonce()
        {
            return new Random().Next(0x100000, 0xFFFFFF).ToString("X6");
        }

        public DigestChallengeRequest Refresh()
        {
            return this with { NonceCount = NonceCount + 1, ClientNonce = NewClientNonce() };
        }

        public AuthenticationHeaderValue GetResponseHeader(string username, string password, string method,
            string digestUri)
        {
            string? hash1 = Hash($"{username}:{Realm}:{password}");
            string? hash2 = Hash($"{method}:{digestUri}");

            string? response = Hash($"{hash1}:{Nonce}:{NonceCount:D8}:{ClientNonce}:{Qop}:{hash2}");

            return AuthenticationHeaderValue.Parse(
                $"Digest username=\"{username}\", realm=\"{Realm}\", nonce=\"{Nonce}\", uri=\"{digestUri}\", algorithm={Algorithm}, qop={Qop}, nc={NonceCount:D8}, cnonce=\"{ClientNonce}\", response=\"{response}\"");
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
                Dictionary<string, string?>? parts = DigestChallengeRequestRegex
                    .Matches(authorizationHeaderValue.Parameter)
                    .ToDictionary
                    (
                        match => match.Groups["key"].Value,
                        match =>
                        {
                            string? value = match.Groups["value"].Value;

                            if (value.StartsWith("\""))
                            {
                                return JsonSerializer.Deserialize<string>(value);
                            }

                            return value;
                        }
                    );

                string? realm = parts["realm"];
                string? domain = parts["domain"];
                string? nonce = parts["nonce"];
                string? algorithm = parts["algorithm"];
                string? qop = parts["qop"];
                string? stale = parts["stale"];

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
