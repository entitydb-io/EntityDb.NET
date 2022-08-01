using EntityDb.Common.Envelopes;
using EntityDb.Common.TypeResolvers;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.TypeResolvers;

public class MemberInfoNameTypeResolverTests
{
    [Fact]
    public void GivenMemberInfoNameTypeResolverKnowsExpectedType_WhenResolvingType_ThenReturnExpectedType()
    {
        // ARRANGE

        var expectedType = typeof(string);

        var envelopeHeaders = new EnvelopeHeaders(new Dictionary<string, string>
        {
            [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform,
            [EnvelopeHelper.MemberInfoName] = expectedType.Name
        });

        var typeResolver = new MemberInfoNamePartialTypeResolver(new[] { expectedType });

        // ACT

        var resolved = typeResolver.TryResolveType(envelopeHeaders, out var actualType);

        // ASSERT

        resolved.ShouldBeTrue();
        actualType.ShouldBe(expectedType);
    }

    [Fact]
    public void GivenNonEmptyMemberInfoNameTypeResolver_WhenResolvingTypeWithNoInformation_ThenReturnNull()
    {
        // ARRANGE

        var typeResolver = new MemberInfoNamePartialTypeResolver(new[] { typeof(string) });

        var headers = new EnvelopeHeaders(new Dictionary<string, string>());

        // ACT

        var resolved = typeResolver.TryResolveType(headers, out var actualType);

        // ASSERT

        resolved.ShouldBeFalse();
        actualType.ShouldBeNull();
    }

    [Fact]
    public void GivenEmptyMemberInfoNameTypeResolver_WhenResolvingType_ThenReturnNull()
    {
        // ARRANGE

        var typeResolver = new MemberInfoNamePartialTypeResolver(Array.Empty<Type>());

        var envelopeHeaders = new EnvelopeHeaders(new Dictionary<string, string>
        {
            [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform,
            [EnvelopeHelper.MemberInfoName] = ""
        });

        // ACT

        var resolved = typeResolver.TryResolveType(envelopeHeaders, out var actualType);

        // ASSERT

        resolved.ShouldBeFalse();
        actualType.ShouldBeNull();
    }
}