using System.Collections.Generic;

namespace EntityDb.Mvc.Tests.Seeder
{
    public class HttpContextSeederOptions
    {
        public Dictionary<string, string[]> Headers { get; set; } = new();
        public Dictionary<string, string[]> Query { get; set; } = new();
        public bool HasIpAddress { get; set; }
        public string? Role { get; set; }
        public string Method { get; set; } = "GET";
        public string Scheme { get; set; } = "https";
        public string Host { get; set; } = "localhost";
        public string Path { get; set; } = "/";
        public string Protocol { get; set; } = "HTTP/1.1";
    }
}
