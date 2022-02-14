using System;

namespace EntityDb.Common.Projections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TProjection"></typeparam>
    public interface IProjection<TProjection>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectionId"></param>
        /// <returns></returns>
        abstract static TProjection Construct(Guid projectionId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ulong GetEntityVersionNumber(Guid entityId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TProjection SkipEntityVersionNumber(Guid entityId);
    }
}
