using System;
using System.Threading;

namespace EntityDb.Common.Tests.Implementations.Snapshots;

public interface ISnapshotWithShouldReplaceLogic<TSnapshot>
{
    static abstract AsyncLocal<Func<TSnapshot, TSnapshot?, bool>?> ShouldReplaceLogic { get; }
}