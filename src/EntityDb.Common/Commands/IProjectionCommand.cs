using System;

namespace EntityDb.Common.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TProjection"></typeparam>
    public interface IProjectionCommand<TProjection>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        TProjection Reduce(Guid entityId, TProjection projection);
    }
}
