using EntityDb.MongoDb.Provisioner.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner
{
    public class MongoDbAtlasClient : IDisposable
    {
        private readonly Settings _mongoDbAtlasSettings;
        private readonly HttpClient _httpClient;

        private DigestChallengeRequest? DigestChallengeRequest { get; set; }

        public MongoDbAtlasClient(Settings mongoDbAtlasSettings)
        {
            _mongoDbAtlasSettings = mongoDbAtlasSettings;

            _httpClient = new HttpClient();
        }

        private async Task<HttpResponseMessage> GetResponse(Func<HttpRequestMessage> createRequest)
        {
            var request = createRequest.Invoke();

            if (DigestChallengeRequest != null && DigestChallengeRequest.IsUsable)
            {
                request.Headers.Authorization = DigestChallengeRequest.GetResponseHeader
                (
                    _mongoDbAtlasSettings.PublicKey,
                    _mongoDbAtlasSettings.PrivateKey,
                    request.Method.Method,
                    request.RequestUri!.AbsolutePath
                );

                DigestChallengeRequest = DigestChallengeRequest.Refresh();
            }

            return await _httpClient.SendAsync(request);
        }

        private async Task<HttpResponseMessage> Send(Func<HttpRequestMessage> createRequest)
        {
            var firstResponse = await GetResponse(createRequest);

            if (firstResponse.StatusCode != HttpStatusCode.Unauthorized)
            {
                return firstResponse;
            }

            var wwwAuthenticateHeader = AuthenticationHeaderValue.Parse(firstResponse.Headers.WwwAuthenticate.ToString());

            if (DigestChallengeRequest.TryParse(wwwAuthenticateHeader, out var digestChallengeRequest) == false)
            {
                return firstResponse;
            }

            DigestChallengeRequest = digestChallengeRequest;

            return await GetResponse(createRequest);
        }

        public async Task<bool> UserExists(string databaseName, string username)
        {
            var getUserResponse = await Send(() => new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_mongoDbAtlasSettings.BaseUrl}/api/atlas/v1.0/groups/{_mongoDbAtlasSettings.GroupId}/databaseUsers/{databaseName}/{username}?pretty=true"),
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
                RequestUri = new Uri($"{_mongoDbAtlasSettings.BaseUrl}/api/atlas/v1.0/groups/{_mongoDbAtlasSettings.GroupId}/customDBRoles/roles/{role}?pretty=true"),
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
            var jsonPayload = JsonSerializer.Serialize(new
            {
                actions,
                roleName,
            });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var createRoleResponse = await Send(() => new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_mongoDbAtlasSettings.BaseUrl}/api/atlas/v1.0/groups/{_mongoDbAtlasSettings.GroupId}/customDBRoles/roles"),
                Content = content,
            });

            if (createRoleResponse.IsSuccessStatusCode)
            {
                return true;
            }

            throw new InvalidOperationException();
        }

        public async Task<bool> CreateUser(string databaseName, string username, string password, MongoDbAtlastUserRole[] roles)
        {
            var jsonPayload = JsonSerializer.Serialize(new
            {
                databaseName,
                password,
                roles,
                username,
            });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var createUserResponse = await Send(() => new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_mongoDbAtlasSettings.BaseUrl}/api/atlas/v1.0/groups/{_mongoDbAtlasSettings.GroupId}/databaseUsers"),
                Content = content,
            });

            if (createUserResponse.IsSuccessStatusCode)
            {
                return true;
            }

            throw new InvalidOperationException();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
