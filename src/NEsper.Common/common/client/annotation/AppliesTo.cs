///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>Annotation to target certain constructs.</summary>
    public enum AppliesTo
    {
        /// <summary>
        /// For use with annotations as a default value, not used otherwise (internal use only)
        /// </summary>
        UNDEFINED,

        /// <summary>
        /// Group-by for aggregations
        /// </summary>
        AGGREGATION_GROUPBY,

        /// <summary>
        /// Group-by for aggregations
        /// </summary>
        AGGREGATION_UNGROUPED,

        /// <summary>
        /// Local-Group-by for aggregations
        /// </summary>
        AGGREGATION_LOCAL,

        /// <summary>
        /// Rollup-Group-by for aggregations
        /// </summary>
        AGGREGATION_ROLLUP,

        /// <summary>
        /// Context partition id management
        /// </summary>
        CONTEXT_PARTITIONID,

        /// <summary>
        /// Contexts - Category Context
        /// </summary>
        CONTEXT_CATEGORY,

        /// <summary>
        /// Contexts - Hash Context
        /// </summary>
        CONTEXT_HASH,

        /// <summary>
        /// Contexts - Non-overlapping and overlapping
        /// </summary>
        CONTEXT_INITTERM,

        /// <summary>
        /// Contexts - Distinct for overlapping contexts
        /// </summary>
        CONTEXT_INITTERM_DISTINCT,

        /// <summary>
        /// Contexts - Keyed Context
        /// </summary>
        CONTEXT_KEYED,

        /// <summary>
        /// Contexts - Keyed Context termination
        /// </summary>
        CONTEXT_KEYED_TERM,

        /// <summary>
        /// Index hashed
        /// </summary>
        INDEX_HASH,

        /// <summary>
        /// Index in-set-of-values
        /// </summary>
        INDEX_IN,

        /// <summary>
        /// Index btree
        /// </summary>
        INDEX_SORTED,

        /// <summary>
        /// Index composite of hash and btree
        /// </summary>
        INDEX_COMPOSITE,

        /// <summary>
        /// Index unindexed
        /// </summary>
        INDEX_UNINDEXED,

        /// <summary>
        /// Index spatial or other
        /// </summary>
        INDEX_OTHER,

        /// <summary>
        /// Prior
        /// </summary>
        PRIOR,

        /// <summary>
        /// Rank window
        /// </summary>
        WINDOW_RANK,

        /// <summary>
        /// Pattern every-distinct
        /// </summary>
        PATTERN_EVERYDISTINCT,

        /// <summary>
        /// Pattern followed-by
        /// </summary>
        PATTERN_FOLLOWEDBY,

        /// <summary>
        /// Match-recognize partitioned state
        /// </summary>
        ROWRECOG_PARTITIONED,

        /// <summary>
        /// Match-recognize unpartitioned state
        /// </summary>
        ROWRECOG_UNPARTITIONED,

        /// <summary>
        /// Match-recognize schedule state
        /// </summary>
        ROWRECOG_SCHEDULE,

        /// <summary>
        /// Pattern-Root node (internal use only)
        /// </summary>
        PATTERN_ROOT,

        /// <summary>
        /// Pattern-And node
        /// </summary>
        PATTERN_AND,

        /// <summary>
        /// Pattern-Or node
        /// </summary>
        PATTERN_OR,

        /// <summary>
        /// Pattern-Guard node
        /// </summary>
        PATTERN_GUARD,

        /// <summary>
        /// Pattern-Match-Until node
        /// </summary>
        PATTERN_MATCHUNTIL,

        /// <summary>
        /// Pattern-Filter node
        /// </summary>
        PATTERN_FILTER,

        /// <summary>
        /// Pattern-Observer node
        /// </summary>
        PATTERN_OBSERVER,

        /// <summary>
        /// Pattern-Not node
        /// </summary>
        PATTERN_NOT,

        /// <summary>
        /// Pattern-Every node
        /// </summary>
        PATTERN_EVERY,

        /// <summary>
        /// Previous-function
        /// </summary>
        PREVIOUS,

        /// <summary>
        /// Result Set Aggregate-Grouped Output Limit Helper
        /// </summary>
        RESULTSET_AGGREGATEGROUPED_OUTPUTFIRST,

        /// <summary>
        /// Result Set Row-Per-Group Output Limit Helper
        /// </summary>
        RESULTSET_ROWPERGROUP_OUTPUTFIRST,

        /// <summary>
        /// Output rate limiting
        /// </summary>
        RESULTSET_OUTPUTLIMIT,

        /// <summary>
        /// Result Set Rollup Output Limit Helper
        /// </summary>
        RESULTSET_ROLLUP_OUTPUTSNAPSHOT,

        /// <summary>
        /// Result Set Rollup Output Limit Helper
        /// </summary>
        RESULTSET_ROLLUP_OUTPUTALL,

        /// <summary>
        /// Result Set Rollup Output Limit Helper
        /// </summary>
        RESULTSET_ROLLUP_OUTPUTFIRST,

        /// <summary>
        /// Result Set Rollup Output Limit Helper
        /// </summary>
        RESULTSET_ROLLUP_OUTPUTLAST,

        /// <summary>
        /// Result Set Fully-Aggregated Output All
        /// </summary>
        RESULTSET_FULLYAGGREGATED_OUTPUTALL,

        /// <summary>
        /// Result Set Fully-Aggregated Output Last
        /// </summary>
        RESULTSET_FULLYAGGREGATED_OUTPUTLAST,

        /// <summary>
        /// Result Set Simple Output All
        /// </summary>
        RESULTSET_SIMPLE_OUTPUTALL,

        /// <summary>
        /// Result Set Simple Output Last
        /// </summary>
        RESULTSET_SIMPLE_OUTPUTLAST,

        /// <summary>
        /// Result Set Simple Row-Per-Event Output All
        /// </summary>
        RESULTSET_ROWPEREVENT_OUTPUTALL,

        /// <summary>
        /// Result Set Simple Row-Per-Event Output Last
        /// </summary>
        RESULTSET_ROWPEREVENT_OUTPUTLAST,

        /// <summary>
        /// Result Set Row-Per-Group Output All
        /// </summary>
        RESULTSET_ROWPERGROUP_OUTPUTALL,

        /// <summary>
        /// Result Set Row-Per-Group Output All with Option
        /// </summary>
        RESULTSET_ROWPERGROUP_OUTPUTALL_OPT,

        /// <summary>
        /// Result Set Row-Per-Group Output All with Option
        /// </summary>
        RESULTSET_ROWPERGROUP_OUTPUTLAST_OPT,

        /// <summary>
        /// Result Set Row-Per-Group Unbound Helper
        /// </summary>
        RESULTSET_ROWPERGROUP_UNBOUND,

        /// <summary>
        /// Result Set Aggregate-Grouped Output All
        /// </summary>
        RESULTSET_AGGREGATEGROUPED_OUTPUTALL,

        /// <summary>
        /// Result Set Aggregate-Grouped Output All with Options
        /// </summary>
        RESULTSET_AGGREGATEGROUPED_OUTPUTALL_OPT,

        /// <summary>
        /// Result Set Aggregate-Grouped Output Last with Options
        /// </summary>
        RESULTSET_AGGREGATEGROUPED_OUTPUTLAST_OPT,

        /// <summary>
        /// Unique-window
        /// </summary>
        WINDOW_UNIQUE,

        /// <summary>
        /// Time-accumulative window
        /// </summary>
        WINDOW_TIMEACCUM,

        /// <summary>
        /// Time-batch window
        /// </summary>
        WINDOW_TIMEBATCH,

        /// <summary>
        /// Length-batch window
        /// </summary>
        WINDOW_TIMELENGTHBATCH,

        /// <summary>
        /// Grouped window
        /// </summary>
        WINDOW_GROUP,

        /// <summary>
        /// Length window
        /// </summary>
        WINDOW_LENGTH,

        /// <summary>
        /// Time window
        /// </summary>
        WINDOW_TIME,

        /// <summary>
        /// Length-batch window
        /// </summary>
        WINDOW_LENGTHBATCH,

        /// <summary>
        /// Expression window
        /// </summary>
        WINDOW_EXPRESSION,

        /// <summary>
        /// Expression batch window
        /// </summary>
        WINDOW_EXPRESSIONBATCH,

        /// <summary>
        /// First-length window
        /// </summary>
        WINDOW_FIRSTLENGTH,

        /// <summary>
        /// First-time window
        /// </summary>
        WINDOW_FIRSTTIME,

        /// <summary>
        /// First-unique window
        /// </summary>
        WINDOW_FIRSTUNIQUE,

        /// <summary>
        /// First-event window
        /// </summary>
        WINDOW_FIRSTEVENT,

        /// <summary>
        /// Externally-timed window
        /// </summary>
        WINDOW_EXTTIMED,

        /// <summary>
        /// Externally-timed batch window
        /// </summary>
        WINDOW_EXTTIMEDBATCH,

        /// <summary>
        /// Univariate stat view
        /// </summary>
        WINDOW_UNIVARIATESTAT,

        /// <summary>
        /// Correlation stat view
        /// </summary>
        WINDOW_CORRELATION,

        /// <summary>
        /// Size stat view
        /// </summary>
        WINDOW_SIZE,

        /// <summary>
        /// Weighted average stat view
        /// </summary>
        WINDOW_WEIGHTEDAVG,

        /// <summary>
        /// Regression lineest stat view
        /// </summary>
        WINDOW_REGRESSIONLINEST,

        /// <summary>
        /// Union view
        /// </summary>
        WINDOW_UNION,

        /// <summary>
        /// Intersect view
        /// </summary>
        WINDOW_INTERSECT,

        /// <summary>
        /// Last-event window
        /// </summary>
        WINDOW_LASTEVENT,

        /// <summary>
        /// Sorted window
        /// </summary>
        WINDOW_SORTED,

        /// <summary>
        /// Time order window
        /// </summary>
        WINDOW_TIMEORDER,

        /// <summary>
        /// Time-to-live window
        /// </summary>
        WINDOW_TIMETOLIVE,

        /// <summary>
        /// Keep-all window
        /// </summary>
        WINDOW_KEEPALL,

        /// <summary>
        /// Match-recognize view (internal use only)
        /// </summary>
        WINDOW_ROWRECOG
    }
} // end of namespace