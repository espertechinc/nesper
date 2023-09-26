///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>
    ///     Enumeration of hint values. Since hints may be a comma-separate list in a single
    ///     @Hint annotation they are listed as enumeration values here.
    /// </summary>
    public enum HintEnum
    {
        /// <summary>
        ///     For use with match_recognize, iterate-only matching.
        /// </summary>
        ITERATE_ONLY,

        /// <summary>
        ///     For use with group-by, disabled reclaim groups.
        /// </summary>
        DISABLE_RECLAIM_GROUP,

        /// <summary>
        ///     For use with group-by and std:groupwin, reclaim groups for unbound streams based on time.
        ///     The number of seconds after which a groups is reclaimed if inactive.
        /// </summary>
        RECLAIM_GROUP_AGED,

        /// <summary>
        ///     For use with group-by and std:groupwin, reclaim groups for unbound streams based on time,
        ///     this number is the frequency in seconds at which a sweep occurs for aged groups, if not
        ///     provided then the sweep frequency is the same number as the age.
        /// </summary>MAX_FILTER_WIDTH
        RECLAIM_GROUP_FREQ,

        /// <summary>
        ///     For use with create-named-window statements only, to indicate that statements that subquery
        ///     the named window use named window data structures (unless the subquery statement specifies
        ///     below DISBABLE hint and as listed below).
        ///     <para>
        ///         By default and if this hint is not specified or subqueries specify a stream filter on a
        ///         named window, subqueries use statement-local data structures representing named window
        ///         contents (table, index). Such data structure is maintained by consuming the named window
        ///         insert and remove stream.
        ///     </para>
        /// </summary>
        ENABLE_WINDOW_SUBQUERY_INDEXSHARE,

        /// <summary>
        ///     If ENABLE_WINDOW_SUBQUERY_INDEXSHARE is not specified for a named window (the default)
        ///     then this instruction is ignored.
        ///     <para>
        ///         For use with statements that subquery a named window and that benefit from a statement-local
        ///         data structure representing named window contents (table, index), maintained through consuming
        ///         the named window insert and remove stream.
        ///     </para>
        /// </summary>
        DISABLE_WINDOW_SUBQUERY_INDEXSHARE,

        /// <summary>
        ///     For use with subqueries and on-select, on-merge, on-Update and on-delete to specify the
        ///     query engine neither build an implicit index nor use an existing index, always performing
        ///     a full table scan.
        /// </summary>
        SET_NOINDEX,

        /// <summary>
        ///     For use with join query plans to force a nested iteration plan.
        /// </summary>
        FORCE_NESTED_ITER,

        /// <summary>
        ///     For use with join query plans to indicate preferance of the merge-join query plan.
        /// </summary>
        PREFER_MERGE_JOIN,

        /// <summary>
        ///     For use everywhere where indexes are used (subquery, joins, fire-and-forget, on-select etc.), index hint.
        /// </summary>
        INDEX,

        /// <summary>
        ///     For use where query planning applies.
        /// </summary>
        EXCLUDE_PLAN,

        /// <summary>
        ///     For use everywhere where unique data window are used
        /// </summary>
        DISABLE_UNIQUE_IMPLICIT_IDX,

        /// <summary>
        ///     For use when filter expression optimization may widen the filter expression
        /// </summary>
        MAX_FILTER_WIDTH,

        /// <summary>
        /// For use when filter expression optimization may filter index composite lookupable expressions (typically LHS, i.e. left hand side).
        /// Such as "select * from MyEvent(a+b=0)" wherein "a+b" is a composite lookupable expression, i.e. provides lookup values for filter index
        /// lookup.
        /// </summary>
        FILTERINDEX,

        /// <summary>
        ///     For use everywhere where unique data window are used
        /// </summary>
        DISABLE_WHEREEXPR_MOVETO_FILTER,

        /// <summary>
        ///     For use with output rate limiting.
        /// </summary>
        ENABLE_OUTPUTLIMIT_OPT,

        /// <summary>
        ///     For use with output rate limiting.
        /// </summary>
        DISABLE_OUTPUTLIMIT_OPT,

        /// <summary>
        /// For use with named window to silent-delete.
        /// </summary>
        SILENT_DELETE
    }
}