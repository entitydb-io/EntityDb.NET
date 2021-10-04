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
            var filterBuilderMock = new Mock<IFilterBuilder<bool>>(MockBehavior.Strict);

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

            foreach (var entry in truthTable)
            {
                // ACT

                var actualOutput = filterBuilder.Not(entry.Input1);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        [Fact]
        public void AndTruthTable()
        {
            // ARRANGE

            var truthTable = new DoubleInput[]
            {
                new(false, false, false), new(false, true, false), new(true, false, false), new(true, true, true)
            };

            var filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                var actualOutput = filterBuilder.And(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }


        [Fact]
        public void OrTruthTable()
        {
            // ARRANGE

            var truthTable = new DoubleInput[]
            {
                new(false, false, false), new(false, true, true), new(true, false, true), new(true, true, true)
            };

            var filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                var actualOutput = filterBuilder.Or(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        [Fact]
        public void NandTruthTable()
        {
            // ARRANGE

            var truthTable = new DoubleInput[]
            {
                new(false, false, true), new(false, true, true), new(true, false, true), new(true, true, false)
            };

            var filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                var actualOutput = filterBuilder.Nand(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        [Fact]
        public void NorTruthTable()
        {
            // ARRANGE

            var truthTable = new DoubleInput[]
            {
                new(false, false, true), new(false, true, false), new(true, false, false), new(true, true, false)
            };

            var filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                var actualOutput = filterBuilder.Nor(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }


        [Fact]
        public void XorTruthTable()
        {
            // ARRANGE

            var truthTable = new DoubleInput[]
            {
                new(false, false, false), new(false, true, true), new(true, false, true), new(true, true, false)
            };

            var filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                var actualOutput = filterBuilder.Xor(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        [Fact]
        public void XnorTruthTable()
        {
            // ARRANGE

            var truthTable = new DoubleInput[]
            {
                new(false, false, true), new(false, true, false), new(true, false, false), new(true, true, true)
            };

            var filterBuilder = GetFilterBuilder();

            foreach (var entry in truthTable)
            {
                // ACT

                var actualOutput = filterBuilder.Xnor(entry.Input1, entry.Input2);

                // ASSERT

                actualOutput.ShouldBe(entry.ExpectedOutput);
            }
        }

        private record SingleInput(bool Input1, bool ExpectedOutput);

        private record DoubleInput(bool Input1, bool Input2, bool ExpectedOutput);
    }
}
