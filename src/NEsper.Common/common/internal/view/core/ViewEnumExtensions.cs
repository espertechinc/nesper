using System;

using com.espertech.esper.common.@internal.view.derived;
using com.espertech.esper.common.@internal.view.expression;
using com.espertech.esper.common.@internal.view.exttimedbatch;
using com.espertech.esper.common.@internal.view.exttimedwin;
using com.espertech.esper.common.@internal.view.firstevent;
using com.espertech.esper.common.@internal.view.firstlength;
using com.espertech.esper.common.@internal.view.firsttime;
using com.espertech.esper.common.@internal.view.firstunique;
using com.espertech.esper.common.@internal.view.groupwin;
using com.espertech.esper.common.@internal.view.keepall;
using com.espertech.esper.common.@internal.view.lastevent;
using com.espertech.esper.common.@internal.view.length;
using com.espertech.esper.common.@internal.view.lengthbatch;
using com.espertech.esper.common.@internal.view.rank;
using com.espertech.esper.common.@internal.view.sort;
using com.espertech.esper.common.@internal.view.time_accum;
using com.espertech.esper.common.@internal.view.timebatch;
using com.espertech.esper.common.@internal.view.timelengthbatch;
using com.espertech.esper.common.@internal.view.timetolive;
using com.espertech.esper.common.@internal.view.timewin;
using com.espertech.esper.common.@internal.view.unique;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.core
{
    public static class ViewEnumExtensions
    {
        /// <summary>
        ///     Returns namespace that the object belongs to.
        /// </summary>
        /// <returns>namespace</returns>
        public static string GetNamespace(this ViewEnum value)
        {
            switch (value) {
                case ViewEnum.LENGTH_WINDOW:
                    return "win";

                case ViewEnum.TIME_WINDOW:
                    return "win";

                case ViewEnum.KEEPALL_WINDOW:
                    return "win";

                case ViewEnum.TIME_BATCH:
                    return "win";

                case ViewEnum.TIME_LENGTH_BATCH:
                    return "win";

                case ViewEnum.LENGTH_BATCH:
                    return "win";

                case ViewEnum.SORT_WINDOW:
                    return "ext";

                case ViewEnum.RANK_WINDOW:
                    return "ext";

                case ViewEnum.TIME_ACCUM:
                    return "win";

                case ViewEnum.UNIQUE_BY_PROPERTY:
                    return "std";

                case ViewEnum.UNIQUE_FIRST_BY_PROPERTY:
                    return "std";

                case ViewEnum.FIRST_TIME_WINDOW:
                    return "win";

                case ViewEnum.TIME_ORDER:
                    return "ext";

                case ViewEnum.TIMETOLIVE:
                    return "ext";

                case ViewEnum.EXT_TIMED_BATCH:
                    return "win";

                case ViewEnum.EXT_TIMED_WINDOW:
                    return "win";

                case ViewEnum.LAST_EVENT:
                    return "std";

                case ViewEnum.FIRST_EVENT:
                    return "std";

                case ViewEnum.FIRST_LENGTH_WINDOW:
                    return "win";

                case ViewEnum.SIZE:
                    return "std";

                case ViewEnum.UNIVARIATE_STATISTICS:
                    return "stat";

                case ViewEnum.WEIGHTED_AVERAGE:
                    return "stat";

                case ViewEnum.REGRESSION_LINEST:
                    return "stat";

                case ViewEnum.CORRELATION:
                    return "stat";

                case ViewEnum.GROUP_MERGE:
                    return "std";

                case ViewEnum.GROUP_PROPERTY:
                    return "std";

                case ViewEnum.EXPRESSION_BATCH_WINDOW:
                    return "win";

                case ViewEnum.EXPRESSION_WINDOW:
                    return "win";
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        /// <summary>
        ///     Returns name of the view that can be used to reference the view in a view expression.
        /// </summary>
        /// <returns>short name of view</returns>
        public static string GetViewName(this ViewEnum value)
        {
            switch (value) {
                case ViewEnum.LENGTH_WINDOW:
                    return "length";

                case ViewEnum.TIME_WINDOW:
                    return "time";

                case ViewEnum.KEEPALL_WINDOW:
                    return "keepall";

                case ViewEnum.TIME_BATCH:
                    return "time_batch";

                case ViewEnum.TIME_LENGTH_BATCH:
                    return "time_length_batch";

                case ViewEnum.LENGTH_BATCH:
                    return "length_batch";

                case ViewEnum.SORT_WINDOW:
                    return "sort";

                case ViewEnum.RANK_WINDOW:
                    return "rank";

                case ViewEnum.TIME_ACCUM:
                    return "time_accum";

                case ViewEnum.UNIQUE_BY_PROPERTY:
                    return "unique";

                case ViewEnum.UNIQUE_FIRST_BY_PROPERTY:
                    return "firstunique";

                case ViewEnum.FIRST_TIME_WINDOW:
                    return "firsttime";

                case ViewEnum.TIME_ORDER:
                    return "time_order";

                case ViewEnum.TIMETOLIVE:
                    return "timetolive";

                case ViewEnum.EXT_TIMED_BATCH:
                    return "ext_timed_batch";

                case ViewEnum.EXT_TIMED_WINDOW:
                    return "ext_timed";

                case ViewEnum.LAST_EVENT:
                    return "lastevent";

                case ViewEnum.FIRST_EVENT:
                    return "firstevent";

                case ViewEnum.FIRST_LENGTH_WINDOW:
                    return "firstlength";

                case ViewEnum.SIZE:
                    return "size";

                case ViewEnum.UNIVARIATE_STATISTICS:
                    return "uni";

                case ViewEnum.WEIGHTED_AVERAGE:
                    return "weighted_avg";

                case ViewEnum.REGRESSION_LINEST:
                    return "linest";

                case ViewEnum.CORRELATION:
                    return "correl";

                case ViewEnum.GROUP_MERGE:
                    return "merge";

                case ViewEnum.GROUP_PROPERTY:
                    return "groupwin";

                case ViewEnum.EXPRESSION_BATCH_WINDOW:
                    return "expr_batch";

                case ViewEnum.EXPRESSION_WINDOW:
                    return "expr";
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        /// <summary>
        ///     Returns the enumeration value of the view for merging the data generated by another view.
        /// </summary>
        /// <returns>view enum for the merge view</returns>
        public static ViewEnum? GetMergeView(this ViewEnum value)
        {
            switch (value) {
                case ViewEnum.LENGTH_WINDOW:
                case ViewEnum.TIME_WINDOW:
                case ViewEnum.KEEPALL_WINDOW:
                case ViewEnum.TIME_BATCH:
                case ViewEnum.TIME_LENGTH_BATCH:
                case ViewEnum.LENGTH_BATCH:
                case ViewEnum.SORT_WINDOW:
                case ViewEnum.RANK_WINDOW:
                case ViewEnum.TIME_ACCUM:
                case ViewEnum.UNIQUE_BY_PROPERTY:
                case ViewEnum.UNIQUE_FIRST_BY_PROPERTY:
                case ViewEnum.FIRST_TIME_WINDOW:
                case ViewEnum.TIME_ORDER:
                case ViewEnum.TIMETOLIVE:
                case ViewEnum.EXT_TIMED_BATCH:
                case ViewEnum.EXT_TIMED_WINDOW:
                case ViewEnum.LAST_EVENT:
                case ViewEnum.FIRST_EVENT:
                case ViewEnum.FIRST_LENGTH_WINDOW:
                case ViewEnum.SIZE:
                case ViewEnum.UNIVARIATE_STATISTICS:
                case ViewEnum.WEIGHTED_AVERAGE:
                case ViewEnum.REGRESSION_LINEST:
                case ViewEnum.CORRELATION:
                case ViewEnum.GROUP_MERGE:
                case ViewEnum.EXPRESSION_BATCH_WINDOW:
                case ViewEnum.EXPRESSION_WINDOW:
                    return null;

                case ViewEnum.GROUP_PROPERTY:
                    return ViewEnum.GROUP_MERGE;
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        /// <summary>
        ///     Returns a view's factory class.
        /// </summary>
        /// <returns>class of view factory</returns>
        public static Type GetFactoryClass(this ViewEnum value)
        {
            switch (value) {
                case ViewEnum.LENGTH_WINDOW:
                    return typeof(LengthWindowViewForge);

                case ViewEnum.TIME_WINDOW:
                    return typeof(TimeWindowViewForge);

                case ViewEnum.KEEPALL_WINDOW:
                    return typeof(KeepAllViewForge);

                case ViewEnum.TIME_BATCH:
                    return typeof(TimeBatchViewForge);

                case ViewEnum.TIME_LENGTH_BATCH:
                    return typeof(TimeLengthBatchViewForge);

                case ViewEnum.LENGTH_BATCH:
                    return typeof(LengthBatchViewForge);

                case ViewEnum.SORT_WINDOW:
                    return typeof(SortWindowViewForge);

                case ViewEnum.RANK_WINDOW:
                    return typeof(RankWindowViewForge);

                case ViewEnum.TIME_ACCUM:
                    return typeof(TimeAccumViewForge);

                case ViewEnum.UNIQUE_BY_PROPERTY:
                    return typeof(UniqueByPropertyViewForge);

                case ViewEnum.UNIQUE_FIRST_BY_PROPERTY:
                    return typeof(FirstUniqueByPropertyViewForge);

                case ViewEnum.FIRST_TIME_WINDOW:
                    return typeof(FirstTimeViewForge);

                case ViewEnum.TIME_ORDER:
                    return typeof(TimeOrderViewForge);

                case ViewEnum.TIMETOLIVE:
                    return typeof(TimeToLiveViewForge);

                case ViewEnum.EXT_TIMED_BATCH:
                    return typeof(ExternallyTimedBatchViewForge);

                case ViewEnum.EXT_TIMED_WINDOW:
                    return typeof(ExternallyTimedWindowViewForge);

                case ViewEnum.LAST_EVENT:
                    return typeof(LastEventViewForge);

                case ViewEnum.FIRST_EVENT:
                    return typeof(FirstEventViewForge);

                case ViewEnum.FIRST_LENGTH_WINDOW:
                    return typeof(FirstLengthWindowViewForge);

                case ViewEnum.SIZE:
                    return typeof(SizeViewForge);

                case ViewEnum.UNIVARIATE_STATISTICS:
                    return typeof(UnivariateStatisticsViewForge);

                case ViewEnum.WEIGHTED_AVERAGE:
                    return typeof(WeightedAverageViewForge);

                case ViewEnum.REGRESSION_LINEST:
                    return typeof(RegressionLinestViewForge);

                case ViewEnum.CORRELATION:
                    return typeof(CorrelationViewForge);

                case ViewEnum.GROUP_MERGE:
                    return typeof(MergeViewFactoryForge);

                case ViewEnum.GROUP_PROPERTY:
                    return typeof(GroupByViewFactoryForge);

                case ViewEnum.EXPRESSION_BATCH_WINDOW:
                    return typeof(ExpressionBatchViewForge);

                case ViewEnum.EXPRESSION_WINDOW:
                    return typeof(ExpressionWindowViewForge);
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        /// <summary>
        ///     Returns the view enumeration value given the name of the view.
        /// </summary>
        /// <param name="namespace">is the namespace name of the view</param>
        /// <param name="name">is the short name of the view as used in view expressions</param>
        /// <returns>view enumeration value, or null if no such view name is among the enumerated values</returns>
        public static ViewEnum? ForName(
            string @namespace,
            string name)
        {
            if (@namespace != null) {
                foreach (var viewEnum in EnumHelper.GetValues<ViewEnum>()) {
                    if (viewEnum.GetNamespace().Equals(@namespace) && viewEnum.GetViewName().Equals(name)) {
                        return viewEnum;
                    }
                }
            }
            else {
                foreach (var viewEnum in EnumHelper.GetValues<ViewEnum>()) {
                    if (viewEnum.GetViewName().Equals(name)) {
                        return viewEnum;
                    }
                }
            }

            return null;
        }
    }
}