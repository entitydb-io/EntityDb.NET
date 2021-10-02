using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Queries.Filtered;
using EntityDb.Common.Queries.Modified;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    /// Extensions for queries.
    /// </summary>
    public static class IQueryExtensions
    {
        /// <summary>
        /// Returns a new <see cref="ISourceQuery"/> that "ands" a source filter with the filter of a source query. All other properties of the query are inherited.
        /// </summary>
        /// <param name="sourceQuery">The source query.</param>
        /// <param name="sourceFilter">The source filter.</param>
        /// <returns>A new <see cref="ISourceQuery"/> that "ands" <paramref name="sourceFilter"/> with the filter of <paramref name="sourceQuery"/>.</returns>
        public static ISourceQuery Filter(this ISourceQuery sourceQuery, ISourceFilter sourceFilter)
        {
            return new FilteredSourceQuery(sourceQuery, sourceFilter);
        }

        /// <summary>
        /// Returns a new <see cref="ICommandQuery"/> that "ands" a command filter with the filter of a command query. All other properties of the query are inherited.
        /// </summary>
        /// <param name="commandQuery">The command query.</param>
        /// <param name="commandFilter">The command filter.</param>
        /// <returns>A new <see cref="ICommandQuery"/> that "ands" <paramref name="commandFilter"/> with the filter of <paramref name="commandQuery"/>.</returns>
        public static ICommandQuery Filter(this ICommandQuery commandQuery, ICommandFilter commandFilter)
        {
            return new FilteredCommandQuery(commandQuery, commandFilter);
        }

        /// <summary>
        /// Returns a new <see cref="IFactQuery"/> that "ands" a fact filter with the filter of a fact query. All other properties of the query are inherited.
        /// </summary>
        /// <param name="factQuery">The fact query.</param>
        /// <param name="factFilter">The fact filter.</param>
        /// <returns>A new <see cref="IFactQuery"/> that "ands" <paramref name="factFilter"/> with the filter of <paramref name="factQuery"/>.</returns>
        public static IFactQuery Filter(this IFactQuery factQuery, IFactFilter factFilter)
        {
            return new FilteredFactQuery(factQuery, factFilter);
        }

        /// <summary>
        /// Returns a new <see cref="ILeaseQuery"/> that "ands" a lease filter with the filter of a lease query. All other properties of the query are inherited.
        /// </summary>
        /// <param name="leaseQuery">The lease query.</param>
        /// <param name="leaseFilter">The lease filter.</param>
        /// <returns>A new <see cref="ILeaseQuery"/> that "ands" <paramref name="leaseFilter"/> with the filter of <paramref name="leaseQuery"/>.</returns>
        public static ILeaseQuery Filter(this ILeaseQuery leaseQuery, ILeaseFilter leaseFilter)
        {
            return new FilteredLeaseQuery(leaseQuery, leaseFilter);
        }

        /// <summary>
        /// Returns a new <see cref="ITagQuery"/> that "ands" a tag filter with the filter of a tag query. All other properties of the query are inherited.
        /// </summary>
        /// <param name="tagQuery">The tag query.</param>
        /// <param name="tagFilter">The tag filter.</param>
        /// <returns>A new <see cref="ITagQuery"/> that "ands" <paramref name="tagFilter"/> with the filter of <paramref name="tagQuery"/>.</returns>
        public static ITagQuery Filter(this ITagQuery tagQuery, ITagFilter tagFilter)
        {
            return new FilteredTagQuery(tagQuery, tagFilter);
        }

        /// <summary>
        /// Returns a new, modified <see cref="ISourceQuery"/>. The way in which it is modified depends on the parameters of this extension method.
        /// </summary>
        /// <param name="sourceQuery">The source query.</param>
        /// <param name="invertFilter">If <c>true</c>, then the new <see cref="ISourceQuery"/> will return the value of <see cref="IFilterBuilder{TFilter}.Not(TFilter)"/> applied to the filter of <paramref name="sourceQuery"/>. Otherwise, the new <see cref="ISourceQuery"/> will just return the same filter as <paramref name="sourceQuery"/>.</param>
        /// <param name="reverseSort">If <c>true</c>, then the new <see cref="ISourceQuery"/> will pass the opposite values of <c>ascending</c> to the <see cref="ISourceSortBuilder{TSort}"/> methods (i.e., <c>true</c> becomes <c>false</c> and vice versa). Otherwise, the new <see cref="ISourceQuery"/> will just return the same sort as <paramref name="sourceQuery"/>. Note that <see cref="ISortBuilder{TSort}.Combine"/> is unaffected in either case.</param>
        /// <param name="replaceSkip">The new <see cref="ISourceQuery"/> will null-coalesce this value with the skip of <paramref name="sourceQuery"/>.</param>
        /// <param name="replaceTake">The new <see cref="ISourceQuery"/> will null-coalesce this value with the take of <paramref name="sourceQuery"/>.</param>
        /// <returns>A new, modified <see cref="ISourceQuery"/>.</returns>
        public static ISourceQuery Modify(this ISourceQuery sourceQuery, bool invertFilter = false, bool reverseSort = false, int? replaceSkip = null, int? replaceTake = null)
        {
            return new ModifiedSourceQuery(sourceQuery, invertFilter, reverseSort, replaceSkip, replaceTake);
        }

        /// <summary>
        /// Returns a new, modified <see cref="ICommandQuery"/>. The way in which it is modified depends on the parameters of this extension method.
        /// </summary>
        /// <param name="commandQuery">The command query.</param>
        /// <param name="invertFilter">If <c>true</c>, then the new <see cref="ICommandQuery"/> will return the value of <see cref="IFilterBuilder{TFilter}.Not(TFilter)"/> applied to the filter of <paramref name="commandQuery"/>. Otherwise, the new <see cref="ICommandQuery"/> will just return the same filter as <paramref name="commandQuery"/>.</param>
        /// <param name="reverseSort">If <c>true</c>, then the new <see cref="ICommandQuery"/> will pass the opposite values of <c>ascending</c> to the <see cref="ICommandSortBuilder{TSort}"/> methods (i.e., <c>true</c> becomes <c>false</c> and vice versa). Otherwise, the new <see cref="ICommandQuery"/> will just return the same sort as <paramref name="commandQuery"/>. Note that <see cref="ISortBuilder{TSort}.Combine"/> is unaffected in either case.</param>
        /// <param name="replaceSkip">The new <see cref="ICommandQuery"/> will null-coalesce this value with the skip of <paramref name="commandQuery"/>.</param>
        /// <param name="replaceTake">The new <see cref="ICommandQuery"/> will null-coalesce this value with the take of <paramref name="commandQuery"/>.</param>
        /// <returns>A new, modified <see cref="ICommandQuery"/>.</returns>
        public static ICommandQuery Modify(this ICommandQuery commandQuery, bool invertFilter = false, bool reverseSort = false, int? replaceSkip = null, int? replaceTake = null)
        {
            return new ModifiedCommandQuery(commandQuery, invertFilter, reverseSort, replaceSkip, replaceTake);
        }

        /// <summary>
        /// Returns a new, modified <see cref="IFactQuery"/>. The way in which it is modified depends on the parameters of this extension method.
        /// </summary>
        /// <param name="factQuery">The fact query.</param>
        /// <param name="invertFilter">If <c>true</c>, then the new <see cref="IFactQuery"/> will return the value of <see cref="IFilterBuilder{TFilter}.Not(TFilter)"/> applied to the filter of <paramref name="factQuery"/>. Otherwise, the new <see cref="IFactQuery"/> will just return the same filter as <paramref name="factQuery"/>.</param>
        /// <param name="reverseSort">If <c>true</c>, then the new <see cref="IFactQuery"/> will pass the opposite values of <c>ascending</c> to the <see cref="IFactSortBuilder{TSort}"/> methods (i.e., <c>true</c> becomes <c>false</c> and vice versa). Otherwise, the new <see cref="IFactQuery"/> will just return the same sort as <paramref name="factQuery"/>. Note that <see cref="ISortBuilder{TSort}.Combine"/> is unaffected in either case.</param>
        /// <param name="replaceSkip">The new <see cref="IFactQuery"/> will null-coalesce this value with the skip of <paramref name="factQuery"/>.</param>
        /// <param name="replaceTake">The new <see cref="IFactQuery"/> will null-coalesce this value with the take of <paramref name="factQuery"/>.</param>
        /// <returns>A new, modified <see cref="IFactQuery"/>.</returns>
        public static IFactQuery Modify(this IFactQuery factQuery, bool invertFilter = false, bool reverseSort = false, int? replaceSkip = null, int? replaceTake = null)
        {
            return new ModifiedFactQuery(factQuery, invertFilter, reverseSort, replaceSkip, replaceTake);
        }

        /// <summary>
        /// Returns a new, modified <see cref="ILeaseQuery"/>. The way in which it is modified depends on the parameters of this extension method.
        /// </summary>
        /// <param name="leaseQuery">The lease query.</param>
        /// <param name="invertFilter">If <c>true</c>, then the new <see cref="ILeaseQuery"/> will return the value of <see cref="IFilterBuilder{TFilter}.Not(TFilter)"/> applied to the filter of <paramref name="leaseQuery"/>. Otherwise, the new <see cref="ILeaseQuery"/> will just return the same filter as <paramref name="leaseQuery"/>.</param>
        /// <param name="reverseSort">If <c>true</c>, then the new <see cref="ILeaseQuery"/> will pass the opposite values of <c>ascending</c> to the <see cref="ILeaseSortBuilder{TSort}"/> methods (i.e., <c>true</c> becomes <c>false</c> and vice versa). Otherwise, the new <see cref="ILeaseQuery"/> will just return the same sort as <paramref name="leaseQuery"/>. Note that <see cref="ISortBuilder{TSort}.Combine"/> is unaffected in either case.</param>
        /// <param name="replaceSkip">The new <see cref="ILeaseQuery"/> will null-coalesce this value with the skip of <paramref name="leaseQuery"/>.</param>
        /// <param name="replaceTake">The new <see cref="ILeaseQuery"/> will null-coalesce this value with the take of <paramref name="leaseQuery"/>.</param>
        /// <returns>A new, modified <see cref="ILeaseQuery"/>.</returns>
        public static ILeaseQuery Modify(this ILeaseQuery leaseQuery, bool invertFilter = false, bool reverseSort = false, int? replaceSkip = null, int? replaceTake = null)
        {
            return new ModifiedLeaseQuery(leaseQuery, invertFilter, reverseSort, replaceSkip, replaceTake);
        }

        /// <summary>
        /// Returns a new, modified <see cref="ITagQuery"/>. The way in which it is modified depends on the parameters of this extension method.
        /// </summary>
        /// <param name="tagQuery">The tag query.</param>
        /// <param name="invertFilter">If <c>true</c>, then the new <see cref="ITagQuery"/> will return the value of <see cref="IFilterBuilder{TFilter}.Not(TFilter)"/> applied to the filter of <paramref name="tagQuery"/>. Otherwise, the new <see cref="ITagQuery"/> will just return the same filter as <paramref name="tagQuery"/>.</param>
        /// <param name="reverseSort">If <c>true</c>, then the new <see cref="ITagQuery"/> will pass the opposite values of <c>ascending</c> to the <see cref="ITagSortBuilder{TSort}"/> methods (i.e., <c>true</c> becomes <c>false</c> and vice versa). Otherwise, the new <see cref="ITagQuery"/> will just return the same sort as <paramref name="tagQuery"/>. Note that <see cref="ISortBuilder{TSort}.Combine"/> is unaffected in either case.</param>
        /// <param name="replaceSkip">The new <see cref="ITagQuery"/> will null-coalesce this value with the skip of <paramref name="tagQuery"/>.</param>
        /// <param name="replaceTake">The new <see cref="ITagQuery"/> will null-coalesce this value with the take of <paramref name="tagQuery"/>.</param>
        /// <returns>A new, modified <see cref="ITagQuery"/>.</returns>
        public static ITagQuery Modify(this ITagQuery tagQuery, bool invertFilter = false, bool reverseSort = false, int? replaceSkip = null, int? replaceTake = null)
        {
            return new ModifiedTagQuery(tagQuery, invertFilter, reverseSort, replaceSkip, replaceTake);
        }
    }
}
