using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Extensions;

internal static class PointerExtensions
{
    public static Pointer Previous(this Pointer pointer)
    {
        return pointer.Id + pointer.VersionNumber.Previous();
    }
}
