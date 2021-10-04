using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Common.Extensions;
using Moq;
using Shouldly;
using System.Linq;
using Xunit;

namespace EntityDb.Common.Tests.Extensions
{
    public class IFilterBuilderExtensionsTests
    {
        private IFilterBuilder<bool> GetFilterBuilder()
        {
            Mock<IFilterBuilder<bool>>? filterBuilderMock = new Mock<IFilterBuilder<bool>>(MockBehavior.Strict);

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

            SingleInput[]? truthTable = new SingleInput[] { new(true, false), new(false, true) };

            IFilterBuilder<bool>? filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                bool actualOutput = filterBuilder.Not(entry.Input1);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        [Fact]
        public void AndTruthTable()
        {
            // ARRANGE

            DoubleInput[]? truthTable = new DoubleInput[]
            {
                new(false, false, false), new(false, true, false), new(true, false, false), new(true, true, true)
            };

            IFilterBuilder<bool>? filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                bool actualOutput = filterBuilder.And(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }


        [Fact]
        public void OrTruthTable()
        {
            // ARRANGE

            DoubleInput[]? truthTable = new DoubleInput[]
            {
                new(false, false, false), new(false, true, true), new(true, false, true), new(true, true, true)
            };

            IFilterBuilder<bool>? filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                bool actualOutput = filterBuilder.Or(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        [Fact]
        public void NandTruthTable()
        {
            // ARRANGE

            DoubleInput[]? truthTable = new DoubleInput[]
            {
                new(false, false, true), new(false, true, true), new(true, false, true), new(true, true, false)
            };

            IFilterBuilder<bool>? filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                bool actualOutput = filterBuilder.Nand(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        [Fact]
        public void NorTruthTable()
        {
            // ARRANGE

            DoubleInput[]? truthTable = new DoubleInput[]
            {
                new(false, false, true), new(false, true, false), new(true, false, false), new(true, true, false)
            };

            IFilterBuilder<bool>? filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                bool actualOutput = filterBuilder.Nor(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }


        [Fact]
        public void XorTruthTable()
        {
            // ARRANGE

            DoubleInput[]? truthTable = new DoubleInput[]
            {
                new(false, false, false), new(false, true, true), new(true, false, true), new(true, true, false)
            };

            IFilterBuilder<bool>? filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                bool actualOutput = filterBuilder.Xor(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        [Fact]
        public void XnorTruthTable()
        {
            // ARRANGE

            DoubleInput[]? truthTable = new DoubleInput[]
            {
                new(false, false, true), new(false, true, false), new(true, false, false), new(true, true, true)
            };

            IFilterBuilder<bool>? filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                bool actualOutput = filterBuilder.Xnor(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        private record SingleInput(bool Input1, bool ExpectedOutput);

        private record DoubleInput(bool Input1, bool Input2, bool ExpectedOutput);
    }
}
