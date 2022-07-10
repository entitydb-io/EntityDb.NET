using EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas;

internal class MongoDbAtlasClient : IDisposable
{
    private static readonly HttpClient HttpClient = new();

    private readonly string _groupId;
    private readonly string _privateKey;
    private readonly string _publicKey;

    private MongoDbAtlasClient(string groupId, string publicKey, string privateKey)
    {
        _groupId = groupId;
        _publicKey = publicKey;
        _privateKey = privateKey;
    }

    private static DigestChallengeRequest? DigestChallengeRequest { get; set; }

    public void Dispose()
    {
        HttpClient.Dispose();
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

        if (DigestChallengeRequest is not { IsUsable: true })
        {
            return await HttpClient.SendAsync(request);
        }

        request.Headers.Authorization = DigestChallengeRequest.GetResponseHeader
        (
            publicKey,
            privateKey,
            request.Method.Method,
            request.RequestUri!.AbsolutePath
        );

        DigestChallengeRequest = DigestChallengeRequest.Refresh();

        return await HttpClient.SendAsync(request);
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

        return getUserResponse.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,
            _ => throw new InvalidOperationException()
        };
    }

    public async Task<bool> RoleExists(string role)
    {
        var getRoleResponse = await Send(() => new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = GetUri($"groups/{_groupId}/customDBRoles/roles/{role}")
        });

        return getRoleResponse.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,
            _ => throw new InvalidOperationException()
        };
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
        MongoDbAtlasUserRole[] roles)
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

    public async Task<ServerlessInstance?> GetServerlessInstance(string instanceName)
    {
        var getServerlessInstanceResponse = await Send(() => new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = GetUri($"groups/{_groupId}/serverless/{instanceName}")
        });

        if (!getServerlessInstanceResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var responseStream = await getServerlessInstanceResponse.Content.ReadAsStreamAsync();

        return await JsonSerializer.DeserializeAsync<ServerlessInstance>(responseStream);
    }

    public async Task<Cluster?> GetCluster(string clusterName)
    {
        var getClusterResponse = await Send(() => new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = GetUri($"groups/{_groupId}/clusters/{clusterName}")
        });

        if (!getClusterResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var responseStream = await getClusterResponse.Content.ReadAsStreamAsync();

        return await JsonSerializer.DeserializeAsync<Cluster>(responseStream);
    }

    public static async Task<MongoDbAtlasClient> Create(string groupName, string publicKey, string privateKey)
    {
        var getGroupsResponse = await Send(publicKey, privateKey,
            () => new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = GetUri("groups") });

        if (!getGroupsResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var responseStream = await getGroupsResponse.Content.ReadAsStreamAsync();

        var listOfGroups = await JsonSerializer.DeserializeAsync<ListOf<Group>>(responseStream) ??
                           throw new InvalidOperationException();

        var group = listOfGroups
            .Results
            .Single(group => group.Name == groupName);

        return new MongoDbAtlasClient(group.Id, publicKey, privateKey);
    }
}
