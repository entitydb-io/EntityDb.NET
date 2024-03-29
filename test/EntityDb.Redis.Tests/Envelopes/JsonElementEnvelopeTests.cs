﻿using System.Text;
using EntityDb.Common.Tests.Envelopes;

namespace EntityDb.Redis.Tests.Envelopes;

public class JsonElementEnvelopeTests : EnvelopeTestsBase<Startup, byte[]>
{
    public JsonElementEnvelopeTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    protected override byte[] GenerateCorruptedSerializedData()
    {
        const string invalidJson = "I AM A STRING VALUE, NOT JSON!";

        return Encoding.UTF8.GetBytes(invalidJson);
    }
}