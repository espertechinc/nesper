///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Enum for all build-in views.
    /// </summary>
    public enum ViewEnum
    {
        /// <summary>
        ///     Length window.
        /// </summary>
        LENGTH_WINDOW,

        /// <summary>
        ///     Time window.
        /// </summary>
        TIME_WINDOW,

        /// <summary>
        ///     Keep-all data window.
        /// </summary>
        KEEPALL_WINDOW,

        /// <summary>
        ///     Time batch.
        /// </summary>
        TIME_BATCH,

        /// <summary>
        ///     Time length batch.
        /// </summary>
        TIME_LENGTH_BATCH,

        /// <summary>
        ///     Length batch window.
        /// </summary>
        LENGTH_BATCH,

        /// <summary>
        ///     Sorted window.
        /// </summary>
        SORT_WINDOW,

        /// <summary>
        ///     Rank window.
        /// </summary>
        RANK_WINDOW,

        /// <summary>
        ///     Time accumulating view.
        /// </summary>
        TIME_ACCUM,

        /// <summary>
        ///     Unique.
        /// </summary>
        UNIQUE_BY_PROPERTY,

        /// <summary>
        ///     First-Unique.
        /// </summary>
        UNIQUE_FIRST_BY_PROPERTY,

        /// <summary>
        ///     Time first window.
        /// </summary>
        FIRST_TIME_WINDOW,

        /// <summary>
        ///     Time order event window.
        /// </summary>
        TIME_ORDER,

        /// <summary>
        ///     Time order event window.
        /// </summary>
        TIMETOLIVE,

        /// <summary>
        ///     Externally timed batch.
        /// </summary>
        EXT_TIMED_BATCH,

        /// <summary>
        ///     Externally timed window.
        /// </summary>
        EXT_TIMED_WINDOW,

        /// <summary>
        ///     Last event.
        /// </summary>
        LAST_EVENT,

        /// <summary>
        ///     First event.
        /// </summary>
        FIRST_EVENT,

        /// <summary>
        ///     Length first window.
        /// </summary>
        FIRST_LENGTH_WINDOW,

        /// <summary>
        ///     Size view.
        /// </summary>
        SIZE,

        /// <summary>
        ///     Univariate statistics.
        /// </summary>
        UNIVARIATE_STATISTICS,

        /// <summary>
        ///     Weighted avg.
        /// </summary>
        WEIGHTED_AVERAGE,

        /// <summary>
        ///     Linest.
        /// </summary>
        REGRESSION_LINEST,

        /// <summary>
        ///     Correlation.
        /// </summary>
        CORRELATION,

        /// <summary>
        ///     Group-by merge.
        /// </summary>
        GROUP_MERGE,

        /// <summary>
        ///     Group-by.
        /// </summary>
        GROUP_PROPERTY,

        /// <summary>
        ///     Expression batch window.
        /// </summary>
        EXPRESSION_BATCH_WINDOW,

        /// <summary>
        ///     Expression window.
        /// </summary>
        EXPRESSION_WINDOW
    }
} // end of namespace