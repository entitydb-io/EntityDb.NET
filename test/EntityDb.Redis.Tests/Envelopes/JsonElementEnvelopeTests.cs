using System;
using System.Text;
using System.Text.Json;
using EntityDb.Common.Tests.Envelopes;

namespace EntityDb.Redis.Tests.Envelopes;

public class JsonElementEnvelopeTests : EnvelopeTestsBase<Startup, JsonElement>
{
    public JsonElementEnvelopeTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    protected override byte[] GenerateCorruptedBytes()
    {
        const string invalidJson = "I AM A STRING VALUE, NOT JSON!";

        return Encoding.UTF8.GetBytes(invalidJson);
    }
}