namespace EntityDb.Common.TypeResolvers;

/// <summary>
///     Options for the default partial type resolver
/// </summary>
public sealed class DefaultPartialTypeResolverOptions
{
    /// <summary>
    ///     If you rename your assemblies, you will want to update the
    ///     name of the assembly for type resolving purposes, at least
    ///     until you update the data header.
    /// </summary>
    public Dictionary<string, string> UpdateNames { get; set; } = new();

    /// <summary>
    ///     If you version your assemblies, you may want to ignore the
    ///     version of the assembly for type resolving purposes.
    /// </summary>
    public bool IgnoreVersion { get; set; }
}
