using EntityDb.Abstractions.States;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Common.States;

internal static class StateRepositoryFactoryExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Tests are only meant to run in test mode.")]
    public static IStateRepositoryFactory<TState> UseTestMode<TState>
    (
        this IStateRepositoryFactory<TState> stateRepositoryFactory,
        bool testMode
    )
    {
        return testMode
            ? new TestModeStateRepositoryFactory<TState>(stateRepositoryFactory)
            : stateRepositoryFactory;
    }
}
