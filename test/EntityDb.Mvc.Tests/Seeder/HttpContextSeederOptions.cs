namespace EntityDb.Mvc.Tests.Seeder;

public class HttpContextSeederOptions
{
    // Request
    public Dictionary<string, string[]> Headers { get; init; } = new();
    public Dictionary<string, string[]> QueryStringParams { get; init; } = new();
    public string Method { get; init; } = "GET";
    public string Scheme { get; init; } = "https";
    public string Host { get; init; } = "localhost";
    public string Path { get; init; } = "/";
    public string Protocol { get; init; } = "HTTP/1.1";

    // Connection
    public bool HasIpAddress { get; init; }
}