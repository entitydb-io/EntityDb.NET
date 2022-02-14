using EntityDb.Abstractions.Commands;
using EntityDb.Common.Commands;
using EntityDb.Common.Projections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    ///     Extension methods for projections.
    /// </summary>
    public static class ProjectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProjection"></typeparam>
        /// <param name="projection"></param>
        /// <param name="entityId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static TProjection Reduce<TEntity, TProjection>(this TProjection projection, Guid entityId, ICommand<TEntity> command)
            where TProjection : IProjection<TProjection>
        {
            if (command is IProjectionCommand<TProjection> projectionCommand)
            {
                return projectionCommand.Reduce(entityId, projection);
            }

            return projection.SkipEntityVersionNumber(entityId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProjection"></typeparam>
        /// <param name="projection"></param>
        /// <param name="entityId"></param>
        /// <param name="commands"></param>
        /// <returns></returns>
        public static TProjection Reduce<TEntity, TProjection>(this TProjection projection, Guid entityId, IEnumerable<ICommand<TEntity>> commands)
            where TProjection : IProjection<TProjection>
        {
            return commands.Aggregate(projection, (previousProjection, nextCommand) => previousProjection.Reduce(entityId, nextCommand));
        }
    }
}
