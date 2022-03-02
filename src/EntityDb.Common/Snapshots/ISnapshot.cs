namespace EntityDb.Common.Snapshots;

/// <summary>
///     
/// </summary>
public interface ISnapshot<in TSnapshot>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="previousSnapshot"></param>
    /// <returns></returns>
    bool ShouldReplace(TSnapshot? previousSnapshot);
}
