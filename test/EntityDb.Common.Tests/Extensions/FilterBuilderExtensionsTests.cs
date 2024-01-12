﻿using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Common.Sources.Queries.FilterBuilders;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Extensions;

public sealed class FilterBuilderExtensionsTests
{
    private static IDataFilterBuilder<bool> GetFilterBuilder()
    {
        var filterBuilderMock = new Mock<IDataFilterBuilder<bool>>(MockBehavior.Strict);

        filterBuilderMock
            .Setup(builder => builder.Not(It.IsAny<bool>()))
            .Returns((bool filter) => !filter);

        filterBuilderMock
            .Setup(builder => builder.And(It.IsAny<bool[]>()))
            .Returns((bool[] filters) => filters.All(filter => filter));

        filterBuilderMock
            .Setup(builder => builder.Or(It.IsAny<bool[]>()))
            .Returns((bool[] filters) => filters.Any(filter => filter));

        return filterBuilderMock.Object;
    }

    [Fact]
    public void NotTruthTable()
    {
        // ARRANGE

        var truthTable = new SingleInput[] { new(true, false), new(false, true) };

        var filterBuilder = GetFilterBuilder();

        foreach (var (input1, expectedOutput) in truthTable)
        {
            // ACT

            var actualOutput = filterBuilder.Not(input1);

            // ASSERT

            actualOutput.ShouldBe(expectedOutput);
        }
    }

    [Fact]
    public void AndTruthTable()
    {
        // ARRANGE

        var truthTable = new DoubleInput[]
        {
            new(false, false, false), new(false, true, false), new(true, false, false), new(true, true, true),
        };

        var filterBuilder = GetFilterBuilder();

        foreach (var (input1, input2, expectedOutput) in truthTable)
        {
            // ACT

            var actualOutput = filterBuilder.And(input1, input2);

            // ASSERT

            actualOutput.ShouldBe(expectedOutput);
        }
    }


    [Fact]
    public void OrTruthTable()
    {
        // ARRANGE

        var truthTable = new DoubleInput[]
        {
            new(false, false, false), new(false, true, true), new(true, false, true), new(true, true, true),
        };

        var filterBuilder = GetFilterBuilder();

        foreach (var (input1, input2, expectedOutput) in truthTable)
        {
            // ACT

            var actualOutput = filterBuilder.Or(input1, input2);

            // ASSERT

            actualOutput.ShouldBe(expectedOutput);
        }
    }

    [Fact]
    public void NandTruthTable()
    {
        // ARRANGE

        var truthTable = new DoubleInput[]
        {
            new(false, false, true), new(false, true, true), new(true, false, true), new(true, true, false),
        };

        var filterBuilder = GetFilterBuilder();

        foreach (var (input1, input2, expectedOutput) in truthTable)
        {
            // ACT

            var actualOutput = filterBuilder.Nand(input1, input2);

            // ASSERT

            actualOutput.ShouldBe(expectedOutput);
        }
    }

    [Fact]
    public void NorTruthTable()
    {
        // ARRANGE

        var truthTable = new DoubleInput[]
        {
            new(false, false, true), new(false, true, false), new(true, false, false), new(true, true, false),
        };

        var filterBuilder = GetFilterBuilder();

        foreach (var (input1, input2, expectedOutput) in truthTable)
        {
            // ACT

            var actualOutput = filterBuilder.Nor(input1, input2);

            // ASSERT

            actualOutput.ShouldBe(expectedOutput);
        }
    }


    [Fact]
    public void XorTruthTable()
    {
        // ARRANGE

        var truthTable = new DoubleInput[]
        {
            new(false, false, false), new(false, true, true), new(true, false, true), new(true, true, false),
        };

        var filterBuilder = GetFilterBuilder();

        foreach (var (input1, input2, expectedOutput) in truthTable)
        {
            // ACT

            var actualOutput = filterBuilder.Xor(input1, input2);

            // ASSERT

            actualOutput.ShouldBe(expectedOutput);
        }
    }

    [Fact]
    public void XnorTruthTable()
    {
        // ARRANGE

        var truthTable = new DoubleInput[]
        {
            new(false, false, true), new(false, true, false), new(true, false, false), new(true, true, true),
        };

        var filterBuilder = GetFilterBuilder();

        foreach (var (input1, input2, expectedOutput) in truthTable)
        {
            // ACT

            var actualOutput = filterBuilder.Xnor(input1, input2);

            // ASSERT

            actualOutput.ShouldBe(expectedOutput);
        }
    }

    private sealed record SingleInput(bool Input1, bool ExpectedOutput);

    private sealed record DoubleInput(bool Input1, bool Input2, bool ExpectedOutput);
}
