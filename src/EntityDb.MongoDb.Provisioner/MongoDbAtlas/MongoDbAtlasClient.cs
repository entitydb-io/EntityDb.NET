using EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas
{
    internal class MongoDbAtlasClient : IDisposable
    {
        private static readonly HttpClient _httpClient = new();

        private readonly string _groupId;
        private readonly string _privateKey;
        private readonly string _publicKey;

        public MongoDbAtlasClient(string groupId, string publicKey, string privateKey)
        {
            _groupId = groupId;
            _publicKey = publicKey;
            _privateKey = privateKey;
        }

        private static DigestChallengeRequest? DigestChallengeRequest { get; set; }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private static Uri GetUri(string route)
        {
            const string baseUrl = "https://cloud.mongodb.com";

            return new Uri($"{baseUrl}/api/atlas/v1.0/{route}");
        }

        private static async Task<HttpResponseMessage> GetResponse(string publicKey, string privateKey,
            Func<HttpRequestMessage> createRequest)
        {
            var request = createRequest.Invoke();

            if (DigestChallengeRequest != null && DigestChallengeRequest.IsUsable)
            {
                request.Headers.Authorization = DigestChallengeRequest.GetResponseHeader
                (
                    publicKey,
                    privateKey,
                    request.Method.Method,
                    request.RequestUri!.AbsolutePath
                );

                DigestChallengeRequest = DigestChallengeRequest.Refresh();
            }

            return await _httpClient.SendAsync(request);
        }

        private static async Task<HttpResponseMessage> Send(string publicKey, string privateKey,
            Func<HttpRequestMessage> createRequest)
        {
            var firstResponse = await GetResponse(publicKey, privateKey, createRequest);

            if (firstResponse.StatusCode != HttpStatusCode.Unauthorized)
            {
                return firstResponse;
            }

            var wwwAuthenticateHeader =
                AuthenticationHeaderValue.Parse(firstResponse.Headers.WwwAuthenticate.ToString());

            if (!DigestChallengeRequest.TryParse(wwwAuthenticateHeader, out var digestChallengeRequest))
            {
                return firstResponse;
            }

            DigestChallengeRequest = digestChallengeRequest;

            return await GetResponse(publicKey, privateKey, createRequest);
        }

        private Task<HttpResponseMessage> Send(Func<HttpRequestMessage> createRequest)
        {
            return Send(_publicKey, _privateKey, createRequest);
        }

        public async Task<bool> UserExists(string databaseName, string username)
        {
            var getUserResponse = await Send(() => new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = GetUri($"groups/{_groupId}/databaseUsers/{databaseName}/{username}")
            });

            if (getUserResponse.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            if (getUserResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            throw new InvalidOperationException();
        }

        public async Task<bool> RoleExists(string role)
        {
            var getRoleResponse = await Send(() => new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = GetUri($"groups/{_groupId}/customDBRoles/roles/{role}")
            });

            if (getRoleResponse.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            if (getRoleResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            throw new InvalidOperationException();
        }

        public async Task<bool> CreateRole(string roleName, MongoDbAtlasRoleAction[] actions)
        {
            var jsonPayload = JsonSerializer.Serialize(new { actions, roleName });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var createRoleResponse = await Send(() => new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = GetUri($"groups/{_groupId}/customDBRoles/roles"),
                Content = content
            });

            if (createRoleResponse.IsSuccessStatusCode)
            {
                return true;
            }

            throw new InvalidOperationException();
        }

        public async Task<bool> CreateUser(string databaseName, string username, string password,
            MongoDbAtlastUserRole[] roles)
        {
            var jsonPayload = JsonSerializer.Serialize(new { databaseName, password, roles, username });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var createUserResponse = await Send(() => new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = GetUri($"groups/{_groupId}/databaseUsers"),
                Content = content
            });

            if (createUserResponse.IsSuccessStatusCode)
            {
                return true;
            }

            throw new InvalidOperationException();
        }

        public async Task<Cluster?> GetCluster(string clusterName)
        {
            var getClusterResponse = await Send(() => new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = GetUri($"groups/{_groupId}/clusters/{clusterName}")
            });

            if (getClusterResponse.IsSuccessStatusCode)
            {
                var responseStream = await getClusterResponse.Content.ReadAsStreamAsync();

                return await JsonSerializer.DeserializeAsync<Cluster>(responseStream);
            }

            throw new InvalidOperationException();
        }

        public static async Task<MongoDbAtlasClient> Create(string groupName, string publicKey, string privateKey)
        {
            var getGroupsResponse = await Send(publicKey, privateKey,
                () => new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = GetUri("groups") });

            if (getGroupsResponse.IsSuccessStatusCode)
            {
                var responseStream = await getGroupsResponse.Content.ReadAsStreamAsync();

                var listOfGroups = await JsonSerializer.DeserializeAsync<ListOf<Group>>(responseStream) ??
                                   throw new InvalidOperationException();

                var group = listOfGroups
                    .Results
                    .Single(group => group.Name == groupName);

                return new MongoDbAtlasClient(group.Id, publicKey, privateKey);
            }

            throw new InvalidOperationException();
        }
    }
}
