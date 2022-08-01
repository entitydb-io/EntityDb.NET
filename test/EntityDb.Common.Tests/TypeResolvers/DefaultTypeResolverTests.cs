using EntityDb.Common.Envelopes;
using EntityDb.Common.TypeResolvers;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.TypeResolvers;

public class DefaultTypeResolverTests
{
    [Fact]
    public void GivenEmptyHeaders_WhenLoadingType_ThenReturnNull()
    {
        // ARRANGE

        var headers = new EnvelopeHeaders(new Dictionary<string, string>());

        var typeResolver = new DefaultPartialTypeResolver();

        // ACT

        var resolved = typeResolver.TryResolveType(headers, out var actualType);

        // ASSERT

        resolved.ShouldBeFalse();
        actualType.ShouldBeNull();
    }

    [Fact]
    public void GivenFullNames_WhenLoadingType_ThenReturnType()
    {
        // ARRANGE

        var record = new object();

        var expectedType = record.GetType();

        var headers = EnvelopeHelper.GetEnvelopeHeaders(expectedType, true, false);

        var typeResolver = new DefaultPartialTypeResolver();

        // ACT

        var resolved = typeResolver.TryResolveType(headers, out var actualType);

        // ASSERT

        resolved.ShouldBeTrue();
        actualType.ShouldBe(expectedType);
    }

    [Fact]
    public void GivenMemberInfoName_WhenLoadingType_ThenReturnNull()
    {
        // ARRANGE

        var headers = EnvelopeHelper.GetEnvelopeHeaders(typeof(object), false);

        var typeResolver = new DefaultPartialTypeResolver();

        // ACT

        var resolved = typeResolver.TryResolveType(headers, out var actualType);

        // ASSERT

        resolved.ShouldBeFalse();
        actualType.ShouldBeNull();
    }

    [Fact]
    public void GivenNoTypeInformation_WhenLoadingType_ThenReturnNull()
    {
        // ARRANGE

        var typeResolver = new DefaultPartialTypeResolver();

        var envelopeHeaders = new EnvelopeHeaders(new Dictionary<string, string>
        {
            [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform
        });

        // ACT

        var resolved = typeResolver.TryResolveType(envelopeHeaders, out var actualType);

        // ASSERT

        resolved.ShouldBeFalse();
        actualType.ShouldBeNull();
    }

    [Fact]
    public void GivenGarbageTypeInformation_WhenLoadingType_ThenThrow()
    {
        // ARRANGE

        var typeResolver = new DefaultPartialTypeResolver();

        var envelopeHeaders = new EnvelopeHeaders(new Dictionary<string, string>
        {
            [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform,
            [EnvelopeHelper.AssemblyFullName] = "Garbage",
            [EnvelopeHelper.TypeFullName] = "Garbage",
            [EnvelopeHelper.MemberInfoName] = "Garbage"
        });

        // ASSERT

        Should.Throw<FileNotFoundException>(() => typeResolver.TryResolveType(envelopeHeaders, out _));
    }
}