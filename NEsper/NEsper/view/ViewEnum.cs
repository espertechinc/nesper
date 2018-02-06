///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.rowregex;
using com.espertech.esper.view.internals;
using com.espertech.esper.view.std;
using com.espertech.esper.view.ext;
using com.espertech.esper.view.window;
using com.espertech.esper.view.stat;

namespace com.espertech.esper.view
{
    public enum ViewEnum
    {
        /// <summary>Length window.</summary>
        LENGTH,
        /// <summary> Length first window.</summary>
        FIRST_LENGTH_WINDOW,
        /// <summary>Length batch window.</summary>
        LENGTH_BATCH,
        /// <summary>Time window.</summary>
        TIME_WINDOW,
        /// <summary> Time first window.</summary>
        FIRST_TIME_WINDOW,
        /// <summary>Time batch.</summary>
        TIME_BATCH,
        /// <summary>Time length batch.</summary>
        TIME_LENGTH_BATCH,
        /// <summary>Time accumulating view.</summary>
        TIME_ACCUM,
        /// <summary>Externally timed window.</summary>
        EXT_TIMED_WINDOW,
        /// <summary>Externally timed window.</summary>
        EXT_TIMED_BATCH,
        /// <summary>Keep-all data window.</summary>
        KEEPALL_WINDOW,
        /// <summary>Count view.</summary>
        SIZE,
        /// <summary>Last event.</summary>
        LAST_EVENT,
        /// <summary> First event.</summary>
        FIRST_EVENT,
        /// <summary>Unique.</summary>
        UNIQUE_BY_PROPERTY,
        /// <summary> Unique.</summary>
        UNIQUE_FIRST_BY_PROPERTY,
        /// <summary>Group-by merge.</summary>
        GROUP_MERGE,
        /// <summary>Group-by.</summary>
        GROUP_PROPERTY,
        /// <summary>Univariate statistics.</summary>
        UNIVARIATE_STATISTICS,
        /// <summary>Weighted avg.</summary>
        WEIGHTED_AVERAGE,
        /// <summary>CorrelationStatistics.</summary>
        CORRELATION,
        /// <summary>Linest.</summary>
        REGRESSION_LINEST,
        /// <summary>Sorted window.</summary>
        SORT_WINDOW,
        /// <summary>Rank window.</summary>
        RANK_WINDOW,
        /// <summary>Time order event window.</summary>
        TIME_ORDER,
        /// <summary>Time order event window </summary>
        TIME_TO_LIVE,
        /// <summary>Prior event view.</summary>
        PRIOR_EVENT_VIEW,
        /// <summary>For retain-union policy.</summary>
        INTERNAL_UNION,
        /// <summary>For retain-intersection policy.</summary>
        INTERNAL_INTERSECT,
        /// <summary>Match-recognize.</summary>
        INTERNAL_MATCH_RECOG,
        /// <summary>Length window.</summary>
        EXPRESSION_WINDOW,
        /// <summary>Expression batch window.</summary>
        EXPRESSION_BATCH_WINDOW,
        /// <summary>No-op window.</summary>
        NOOP_WINDOW
    }

    public static class ViewEnumExtensions
    {
        /// <summary> Returns namespace that the object belongs to.</summary>
        /// <returns> namespace
        /// </returns>
        public static string GetNamespace(this ViewEnum viewEnum)
        {
            switch (viewEnum)
            {
                case ViewEnum.LENGTH:
                case ViewEnum.FIRST_LENGTH_WINDOW:
                case ViewEnum.LENGTH_BATCH:
                case ViewEnum.TIME_WINDOW:
                case ViewEnum.FIRST_TIME_WINDOW:
                case ViewEnum.TIME_BATCH:
                case ViewEnum.TIME_LENGTH_BATCH:
                case ViewEnum.TIME_ACCUM:
                case ViewEnum.EXT_TIMED_WINDOW:
                case ViewEnum.EXT_TIMED_BATCH:
                case ViewEnum.KEEPALL_WINDOW:
                    return "win";
                case ViewEnum.SIZE:
                case ViewEnum.LAST_EVENT:
                case ViewEnum.FIRST_EVENT:
                case ViewEnum.UNIQUE_BY_PROPERTY:
                case ViewEnum.UNIQUE_FIRST_BY_PROPERTY:
                case ViewEnum.GROUP_MERGE:
                case ViewEnum.GROUP_PROPERTY:
                    return "std";
                case ViewEnum.UNIVARIATE_STATISTICS:
                case ViewEnum.WEIGHTED_AVERAGE:
                case ViewEnum.CORRELATION:
                case ViewEnum.REGRESSION_LINEST:
                    return "stat";
                case ViewEnum.SORT_WINDOW:
                case ViewEnum.RANK_WINDOW:
                case ViewEnum.TIME_ORDER:
                case ViewEnum.TIME_TO_LIVE:
                    return "ext";
                case ViewEnum.PRIOR_EVENT_VIEW:
                    return "int";
                case ViewEnum.INTERNAL_UNION:
                case ViewEnum.INTERNAL_INTERSECT:
                case ViewEnum.INTERNAL_MATCH_RECOG:
                case ViewEnum.NOOP_WINDOW:
                    return "internal";
                case ViewEnum.EXPRESSION_WINDOW:
                case ViewEnum.EXPRESSION_BATCH_WINDOW:
                    return "win";
            }

            throw new ArgumentException("invalid value", nameof(viewEnum));
        }

        /// <summary> Returns name of the view that can be used to reference the view in a view expression.</summary>
        /// <returns> short name of view
        /// </returns>
        public static string GetName(this ViewEnum viewEnum)
        {
            switch (viewEnum)
            {
                case ViewEnum.LENGTH:
                    return "length";
                case ViewEnum.FIRST_LENGTH_WINDOW:
                    return "firstlength";
                case ViewEnum.LENGTH_BATCH:
                    return "length_batch";
                case ViewEnum.TIME_WINDOW:
                    return "time";
                case ViewEnum.FIRST_TIME_WINDOW:
                    return "firsttime";
                case ViewEnum.TIME_BATCH:
                    return "time_batch";
                case ViewEnum.TIME_LENGTH_BATCH:
                    return "time_length_batch";
                case ViewEnum.TIME_ACCUM:
                    return "time_accum";
                case ViewEnum.EXT_TIMED_WINDOW:
                    return "ext_timed";
                case ViewEnum.EXT_TIMED_BATCH:
                    return "ext_timed_batch";
                case ViewEnum.KEEPALL_WINDOW:
                    return "keepall";
                case ViewEnum.SIZE:
                    return "size";
                case ViewEnum.LAST_EVENT:
                    return "lastevent";
                case ViewEnum.FIRST_EVENT:
                    return "firstevent";
                case ViewEnum.UNIQUE_BY_PROPERTY:
                    return "unique";
                case ViewEnum.UNIQUE_FIRST_BY_PROPERTY:
                    return "firstunique";
                case ViewEnum.GROUP_MERGE:
                    return "merge";
                case ViewEnum.GROUP_PROPERTY:
                    return "groupwin";
                case ViewEnum.UNIVARIATE_STATISTICS:
                    return "uni";
                case ViewEnum.WEIGHTED_AVERAGE:
                    return "weighted_avg";
                case ViewEnum.CORRELATION:
                    return "correl";
                case ViewEnum.REGRESSION_LINEST:
                    return "linest";
                case ViewEnum.SORT_WINDOW:
                    return "sort";
                case ViewEnum.RANK_WINDOW:
                    return "rank";
                case ViewEnum.TIME_ORDER:
                    return "time_order";
                case ViewEnum.TIME_TO_LIVE:
                    return "timetolive";
                case ViewEnum.PRIOR_EVENT_VIEW:
                    return "prioreventinternal";
                case ViewEnum.INTERNAL_UNION:
                    return "union";
                case ViewEnum.INTERNAL_INTERSECT:
                    return "intersect";
                case ViewEnum.INTERNAL_MATCH_RECOG:
                    return "match_recognize";
                case ViewEnum.EXPRESSION_WINDOW:
                    return "expr";
                case ViewEnum.EXPRESSION_BATCH_WINDOW:
                    return "expr_batch";
                case ViewEnum.NOOP_WINDOW:
                    return "noop";
            }

            throw new ArgumentException("invalid value", nameof(viewEnum));
        }

        /// <summary> Gets the view's factory class.</summary>
        /// <returns> view's factory class
        /// </returns>
        public static Type GetFactoryType(this ViewEnum viewEnum)
        {
            switch (viewEnum)
            {
                case ViewEnum.LENGTH:
                    return typeof(LengthWindowViewFactory);
                case ViewEnum.FIRST_LENGTH_WINDOW:
                    return typeof(FirstLengthWindowViewFactory);
                case ViewEnum.LENGTH_BATCH:
                    return typeof(LengthBatchViewFactory);
                case ViewEnum.TIME_WINDOW:
                    return typeof(TimeWindowViewFactory);
                case ViewEnum.FIRST_TIME_WINDOW:
                    return typeof(FirstTimeViewFactory);
                case ViewEnum.TIME_BATCH:
                    return typeof(TimeBatchViewFactory);
                case ViewEnum.TIME_LENGTH_BATCH:
                    return typeof(TimeLengthBatchViewFactory);
                case ViewEnum.TIME_ACCUM:
                    return typeof(TimeAccumViewFactory);
                case ViewEnum.EXT_TIMED_WINDOW:
                    return typeof(ExternallyTimedWindowViewFactory);
                case ViewEnum.EXT_TIMED_BATCH:
                    return typeof(ExternallyTimedBatchViewFactory);
                case ViewEnum.KEEPALL_WINDOW:
                    return typeof(KeepAllViewFactory);
                case ViewEnum.SIZE:
                    return typeof(SizeViewFactory);
                case ViewEnum.LAST_EVENT:
                    return typeof(LastElementViewFactory);
                case ViewEnum.FIRST_EVENT:
                    return typeof(FirstElementViewFactory);
                case ViewEnum.UNIQUE_BY_PROPERTY:
                    return typeof(UniqueByPropertyViewFactory);
                case ViewEnum.UNIQUE_FIRST_BY_PROPERTY:
                    return typeof(FirstUniqueByPropertyViewFactory);
                case ViewEnum.GROUP_MERGE:
                    return typeof(MergeViewFactory);
                case ViewEnum.GROUP_PROPERTY:
                    return typeof(GroupByViewFactory);
                case ViewEnum.UNIVARIATE_STATISTICS:
                    return typeof(UnivariateStatisticsViewFactory);
                case ViewEnum.WEIGHTED_AVERAGE:
                    return typeof(WeightedAverageViewFactory);
                case ViewEnum.CORRELATION:
                    return typeof(CorrelationViewFactory);
                case ViewEnum.REGRESSION_LINEST:
                    return typeof(RegressionLinestViewFactory);
                case ViewEnum.SORT_WINDOW:
                    return typeof(SortWindowViewFactory);
                case ViewEnum.RANK_WINDOW:
                    return typeof(RankWindowViewFactory);
                case ViewEnum.TIME_ORDER:
                    return typeof(TimeOrderViewFactory);
                case ViewEnum.TIME_TO_LIVE:
                    return typeof(TimeToLiveViewFactory);
                case ViewEnum.PRIOR_EVENT_VIEW:
                    return typeof(PriorEventViewFactory);
                case ViewEnum.INTERNAL_UNION:
                    return typeof(UnionViewFactory);
                case ViewEnum.INTERNAL_INTERSECT:
                    return typeof(IntersectViewFactory);
                case ViewEnum.INTERNAL_MATCH_RECOG:
                    return typeof(EventRowRegexNFAViewFactory);
                case ViewEnum.EXPRESSION_WINDOW:
                    return typeof(ExpressionWindowViewFactory);
                case ViewEnum.EXPRESSION_BATCH_WINDOW:
                    return typeof(ExpressionBatchViewFactory);
                case ViewEnum.NOOP_WINDOW:
                    return typeof(NoopViewFactory);
            }

            throw new ArgumentException("invalid value", nameof(viewEnum));
        }

        /// <summary> Returns the enumeration value of the view for merging the data generated by another view.</summary>
        /// <returns> view enum for the merge view
        /// </returns>
        public static ViewEnum? GetMergeView(this ViewEnum viewEnum)
        {
            switch (viewEnum)
            {
                case ViewEnum.LENGTH:
                case ViewEnum.FIRST_LENGTH_WINDOW:
                case ViewEnum.LENGTH_BATCH:
                case ViewEnum.TIME_WINDOW:
                case ViewEnum.FIRST_TIME_WINDOW:
                case ViewEnum.TIME_BATCH:
                case ViewEnum.TIME_LENGTH_BATCH:
                case ViewEnum.TIME_ACCUM:
                case ViewEnum.EXT_TIMED_WINDOW:
                case ViewEnum.EXT_TIMED_BATCH:
                case ViewEnum.KEEPALL_WINDOW:
                case ViewEnum.SIZE:
                case ViewEnum.LAST_EVENT:
                case ViewEnum.FIRST_EVENT:
                case ViewEnum.UNIQUE_BY_PROPERTY:
                case ViewEnum.UNIQUE_FIRST_BY_PROPERTY:
                case ViewEnum.GROUP_MERGE:
                    return null;
                case ViewEnum.GROUP_PROPERTY:
                    return ViewEnum.GROUP_MERGE;
                case ViewEnum.UNIVARIATE_STATISTICS:
                case ViewEnum.WEIGHTED_AVERAGE:
                case ViewEnum.CORRELATION:
                case ViewEnum.REGRESSION_LINEST:
                case ViewEnum.SORT_WINDOW:
                case ViewEnum.RANK_WINDOW:
                case ViewEnum.TIME_ORDER:
                case ViewEnum.TIME_TO_LIVE:
                case ViewEnum.PRIOR_EVENT_VIEW:
                case ViewEnum.INTERNAL_UNION:
                case ViewEnum.INTERNAL_INTERSECT:
                case ViewEnum.INTERNAL_MATCH_RECOG:
                case ViewEnum.EXPRESSION_WINDOW:
                case ViewEnum.EXPRESSION_BATCH_WINDOW:
                case ViewEnum.NOOP_WINDOW:
                    return null;
            }

            throw new ArgumentException("invalid value", nameof(viewEnum));
        }

        /// <summary>
        /// Returns the view enumeration value given the name of the view.
        /// </summary>
        /// <param name="nspace">The nspace.</param>
        /// <param name="name">is the short name of the view as used in view expressions</param>
        /// <returns>
        /// view enumeration value, or null if no such view name is among the enumerated values
        /// </returns>

        public static ViewEnum? ForName(String nspace, String name)
        {
            if (string.IsNullOrEmpty(nspace))
            {
                foreach (ViewEnum viewEnum in Enum.GetValues(typeof(ViewEnum)))
                {
                    if (Equals(viewEnum.GetName(), name))
                    {
                        return viewEnum;
                    }
                }
            }
            else
            {
                foreach (ViewEnum viewEnum in Enum.GetValues(typeof(ViewEnum)))
                {
                    if (Equals(viewEnum.GetNamespace(), nspace) &&
                        Equals(viewEnum.GetName(), name))
                    {
                        return viewEnum;
                    }
                }
            }


            return null;
        }
    }
}
