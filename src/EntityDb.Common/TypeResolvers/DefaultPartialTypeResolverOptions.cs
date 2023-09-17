namespace EntityDb.Common.TypeResolvers;

/// <summary>
///     Options for the default partial type resolver
/// </summary>
public sealed class DefaultPartialTypeResolverOptions
{
    /// <summary>
    ///     If you version your assemblies, you may want to ignore the
    ///     version of the assembly for type resolving purposes.
    /// </summary>
    public bool IgnoreVersion { get; set; }
}
