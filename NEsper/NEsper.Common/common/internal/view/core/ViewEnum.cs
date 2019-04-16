///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
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
using com.espertech.esper.common.@internal.view.timebatch;
using com.espertech.esper.common.@internal.view.timelengthbatch;
using com.espertech.esper.common.@internal.view.timetolive;
using com.espertech.esper.common.@internal.view.timewin;
using com.espertech.esper.common.@internal.view.time_accum;
using com.espertech.esper.common.@internal.view.unique;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Enum for all build-in views.
    /// </summary>
    public class ViewEnum
    {
        /// <summary>
        ///     Length window.
        /// </summary>
        public static readonly ViewEnum LENGTH_WINDOW =
            new ViewEnum("win", "length", typeof(LengthWindowViewForge), null);

        /// <summary>
        ///     Time window.
        /// </summary>
        public static readonly ViewEnum TIME_WINDOW =
            new ViewEnum("win", "time", typeof(TimeWindowViewForge), null);

        /// <summary>
        ///     Keep-all data window.
        /// </summary>
        public static readonly ViewEnum KEEPALL_WINDOW =
            new ViewEnum("win", "keepall", typeof(KeepAllViewForge), null);

        /// <summary>
        ///     Time batch.
        /// </summary>
        public static readonly ViewEnum TIME_BATCH =
            new ViewEnum("win", "time_batch", typeof(TimeBatchViewForge), null);

        /// <summary>
        ///     Time length batch.
        /// </summary>
        public static readonly ViewEnum TIME_LENGTH_BATCH =
            new ViewEnum("win", "time_length_batch", typeof(TimeLengthBatchViewForge), null);

        /// <summary>
        ///     Length batch window.
        /// </summary>
        public static readonly ViewEnum LENGTH_BATCH =
            new ViewEnum("win", "length_batch", typeof(LengthBatchViewForge), null);

        /// <summary>
        ///     Sorted window.
        /// </summary>
        public static readonly ViewEnum SORT_WINDOW =
            new ViewEnum("ext", "sort", typeof(SortWindowViewForge), null);

        /// <summary>
        ///     Rank window.
        /// </summary>
        public static readonly ViewEnum RANK_WINDOW =
            new ViewEnum("ext", "rank", typeof(RankWindowViewForge), null);

        /// <summary>
        ///     Time accumulating view.
        /// </summary>
        public static readonly ViewEnum TIME_ACCUM =
            new ViewEnum("win", "time_accum", typeof(TimeAccumViewForge), null);

        /// <summary>
        ///     Unique.
        /// </summary>
        public static readonly ViewEnum UNIQUE_BY_PROPERTY =
            new ViewEnum("std", "unique", typeof(UniqueByPropertyViewForge), null);

        /// <summary>
        ///     First-Unique.
        /// </summary>
        public static readonly ViewEnum UNIQUE_FIRST_BY_PROPERTY =
            new ViewEnum("std", "firstunique", typeof(FirstUniqueByPropertyViewForge), null);

        /// <summary>
        ///     Time first window.
        /// </summary>
        public static readonly ViewEnum FIRST_TIME_WINDOW =
            new ViewEnum("win", "firsttime", typeof(FirstTimeViewForge), null);

        /// <summary>
        ///     Time order event window.
        /// </summary>
        public static readonly ViewEnum TIME_ORDER =
            new ViewEnum("ext", "time_order", typeof(TimeOrderViewForge), null);

        /// <summary>
        ///     Time order event window.
        /// </summary>
        public static readonly ViewEnum TIMETOLIVE =
            new ViewEnum("ext", "timetolive", typeof(TimeToLiveViewForge), null);

        /// <summary>
        ///     Externally timed batch.
        /// </summary>
        public static readonly ViewEnum EXT_TIMED_BATCH =
            new ViewEnum("win", "ext_timed_batch", typeof(ExternallyTimedBatchViewForge), null);

        /// <summary>
        ///     Externally timed window.
        /// </summary>
        public static readonly ViewEnum EXT_TIMED_WINDOW =
            new ViewEnum("win", "ext_timed", typeof(ExternallyTimedWindowViewForge), null);

        /// <summary>
        ///     Last event.
        /// </summary>
        public static readonly ViewEnum LAST_EVENT =
            new ViewEnum("std", "lastevent", typeof(LastEventViewForge), null);

        /// <summary>
        ///     First event.
        /// </summary>
        public static readonly ViewEnum FIRST_EVENT =
            new ViewEnum("std", "firstevent", typeof(FirstEventViewForge), null);

        /// <summary>
        ///     Length first window.
        /// </summary>
        public static readonly ViewEnum FIRST_LENGTH_WINDOW =
            new ViewEnum("win", "firstlength", typeof(FirstLengthWindowViewForge), null);

        /// <summary>
        ///     Size view.
        /// </summary>
        public static readonly ViewEnum SIZE =
            new ViewEnum("std", "size", typeof(SizeViewForge), null);

        /// <summary>
        ///     Univariate statistics.
        /// </summary>
        public static readonly ViewEnum UNIVARIATE_STATISTICS =
            new ViewEnum("stat", "uni", typeof(UnivariateStatisticsViewForge), null);

        /// <summary>
        ///     Weighted avg.
        /// </summary>
        public static readonly ViewEnum WEIGHTED_AVERAGE =
            new ViewEnum("stat", "weighted_avg", typeof(WeightedAverageViewForge), null);

        /// <summary>
        ///     Linest.
        /// </summary>
        public static readonly ViewEnum REGRESSION_LINEST =
            new ViewEnum("stat", "linest", typeof(RegressionLinestViewForge), null);

        /// <summary>
        ///     Correlation.
        /// </summary>
        public static readonly ViewEnum CORRELATION =
            new ViewEnum("stat", "correl", typeof(CorrelationViewForge), null);

        /// <summary>
        ///     Group-by merge.
        /// </summary>
        public static readonly ViewEnum GROUP_MERGE =
            new ViewEnum("std", "merge", typeof(MergeViewFactoryForge), null);

        /// <summary>
        ///     Group-by.
        /// </summary>
        public static readonly ViewEnum GROUP_PROPERTY =
            new ViewEnum("std", "groupwin", typeof(GroupByViewFactoryForge), GROUP_MERGE);

        /// <summary>
        ///     Expression batch window.
        /// </summary>
        public static readonly ViewEnum EXPRESSION_BATCH_WINDOW =
            new ViewEnum("win", "expr_batch", typeof(ExpressionBatchViewForge), null);

        /// <summary>
        ///     Expression window.
        /// </summary>
        public static readonly ViewEnum EXPRESSION_WINDOW =
            new ViewEnum("win", "expr", typeof(ExpressionWindowViewForge), null);

        public static readonly ISet<ViewEnum> Values = new HashSet<ViewEnum>();

        private ViewEnum(
            string @namespace,
            string name,
            Type factoryClass,
            ViewEnum mergeView)
        {
            Namespace = @namespace;
            Name = name;
            FactoryClass = factoryClass;
            MergeView = mergeView;
            Values.Add(this);
        }

        /// <summary>
        ///     Returns namespace that the object belongs to.
        /// </summary>
        /// <returns>namespace</returns>
        public string Namespace { get; }

        /// <summary>
        ///     Returns name of the view that can be used to reference the view in a view expression.
        /// </summary>
        /// <returns>short name of view</returns>
        public string Name { get; }

        /// <summary>
        ///     Returns the enumeration value of the view for merging the data generated by another view.
        /// </summary>
        /// <returns>view enum for the merge view</returns>
        public ViewEnum MergeView { get; }

        /// <summary>
        ///     Returns a view's factory class.
        /// </summary>
        /// <returns>class of view factory</returns>
        public Type FactoryClass { get; }

        /// <summary>
        ///     Returns the view enumeration value given the name of the view.
        /// </summary>
        /// <param name="namespace">is the namespace name of the view</param>
        /// <param name="name">is the short name of the view as used in view expressions</param>
        /// <returns>view enumeration value, or null if no such view name is among the enumerated values</returns>
        public static ViewEnum ForName(
            string @namespace,
            string name)
        {
            if (@namespace != null) {
                foreach (var viewEnum in Values) {
                    if (viewEnum.Namespace.Equals(@namespace) && viewEnum.Name.Equals(name)) {
                        return viewEnum;
                    }
                }
            }
            else {
                foreach (var viewEnum in Values) {
                    if (viewEnum.Name.Equals(name)) {
                        return viewEnum;
                    }
                }
            }

            return null;
        }
    }
} // end of namespace