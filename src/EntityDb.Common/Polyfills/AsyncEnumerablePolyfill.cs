using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Polyfills;

internal static class AsyncEnumerablePolyfill
{
    public static IAsyncEnumerable<T> FromResult<T>(IEnumerable<T> enumerable)
    {
        var enumerator = enumerable.GetEnumerator();
        var current = default(T);

        var asyncEnumerator = AsyncEnumerator.Create<T>
        (
            () =>
            {
                var moveNext = enumerator.MoveNext();

                current = moveNext
                    ? enumerator.Current
                    : default;

                return ValueTask.FromResult(moveNext);
            },
            () => current!,
            () =>
            {
                enumerator.Dispose();

                return ValueTask.CompletedTask;
            }
        );

        return AsyncEnumerable.Create(_ => asyncEnumerator);
    }
}
