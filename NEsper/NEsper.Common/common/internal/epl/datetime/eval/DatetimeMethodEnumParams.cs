///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeMethodEnumParams
    {
        /// <summary>
        ///     Interval.
        /// </summary>
        internal const string INPUT_INTERVAL = "timestamp or timestamped-event";

        internal const string INPUT_INTERVAL_START = "interval start value";
        internal const string INPUT_INTERVAL_FINISHES = "interval finishes value";

        protected internal static readonly DotMethodFP[] WITHTIME = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam("an integer-type hour", EPLExpressionParamType.SPECIFIC, typeof(int)),
                new DotMethodFPParam("an integer-type minute", EPLExpressionParamType.SPECIFIC, typeof(int)),
                new DotMethodFPParam("an integer-type second", EPLExpressionParamType.SPECIFIC, typeof(int)),
                new DotMethodFPParam("an integer-type millis", EPLExpressionParamType.SPECIFIC, typeof(int)))
        };

        protected internal static readonly DotMethodFP[] WITHDATE = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam("an integer-type year", EPLExpressionParamType.SPECIFIC, typeof(int)),
                new DotMethodFPParam("an integer-type month", EPLExpressionParamType.SPECIFIC, typeof(int)),
                new DotMethodFPParam("an integer-type day", EPLExpressionParamType.SPECIFIC, typeof(int)))
        };

        protected internal static readonly DotMethodFP[] PLUSMINUS = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(0, "a numeric-type millisecond", EPLExpressionParamType.NUMERIC)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam("a time period", EPLExpressionParamType.SPECIFIC, typeof(TimePeriod)))
        };

        protected internal static readonly DotMethodFP[] CALFIELD = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(
                    "a string-type calendar field name",
                    EPLExpressionParamType.SPECIFIC,
                    typeof(string)))
        };

        protected internal static readonly DotMethodFP[] CALFIELD_PLUS_INT = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(
                    "a string-type calendar field name",
                    EPLExpressionParamType.SPECIFIC,
                    typeof(string)),
                new DotMethodFPParam("an integer-type value", EPLExpressionParamType.SPECIFIC, typeof(int)))
        };

        protected internal static readonly DotMethodFP[] NOPARAM = {
            new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY)
        };

        protected internal static readonly DotMethodFP[] BETWEEN = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam("a date-time type", EPLExpressionParamType.DATETIME),
                new DotMethodFPParam("a date-time type", EPLExpressionParamType.DATETIME)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam("a date-time type", EPLExpressionParamType.DATETIME),
                new DotMethodFPParam("a date-time type", EPLExpressionParamType.DATETIME),
                new DotMethodFPParam("boolean", EPLExpressionParamType.BOOLEAN),
                new DotMethodFPParam("boolean", EPLExpressionParamType.BOOLEAN))
        };

        protected internal static readonly DotMethodFP[] INTERVAL_BEFORE_AFTER = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(INPUT_INTERVAL_START, EPLExpressionParamType.TIME_PERIOD_OR_SEC)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(INPUT_INTERVAL_START, EPLExpressionParamType.TIME_PERIOD_OR_SEC),
                new DotMethodFPParam(INPUT_INTERVAL_FINISHES, EPLExpressionParamType.TIME_PERIOD_OR_SEC))
        };

        protected internal static readonly DotMethodFP[] INTERVAL_COINCIDES = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam("threshold for start and end value", EPLExpressionParamType.TIME_PERIOD_OR_SEC)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam("threshold for start value", EPLExpressionParamType.TIME_PERIOD_OR_SEC),
                new DotMethodFPParam("threshold for end value", EPLExpressionParamType.TIME_PERIOD_OR_SEC))
        };

        protected internal static readonly DotMethodFP[] INTERVAL_DURING_INCLUDES = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(
                    "maximum distance interval both start and end",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(
                    "minimum distance interval both start and end",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC),
                new DotMethodFPParam(
                    "maximum distance interval both start and end",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam("minimum distance start", EPLExpressionParamType.TIME_PERIOD_OR_SEC),
                new DotMethodFPParam("maximum distance start", EPLExpressionParamType.TIME_PERIOD_OR_SEC),
                new DotMethodFPParam("minimum distance end", EPLExpressionParamType.TIME_PERIOD_OR_SEC),
                new DotMethodFPParam("maximum distance end", EPLExpressionParamType.TIME_PERIOD_OR_SEC))
        };

        protected internal static readonly DotMethodFP[] INTERVAL_DURING_OVERLAPS_OVERLAPBY = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(
                    "maximum distance interval both start and end",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(
                    "minimum distance interval both start and end",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC),
                new DotMethodFPParam(
                    "maximum distance interval both start and end",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC))
        };

        protected internal static readonly DotMethodFP[] INTERVAL_FINISHES_FINISHEDBY = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(
                    "maximum distance between end timestamps",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC))
        };

        protected internal static readonly DotMethodFP[] INTERVAL_STARTS_STARTEDBY = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(
                    "maximum distance between start timestamps",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC))
        };

        protected internal static readonly DotMethodFP[] INTERVAL_MEETS_METBY = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY),
                new DotMethodFPParam(
                    "maximum distance between start and end timestamps",
                    EPLExpressionParamType.TIME_PERIOD_OR_SEC))
        };

        protected internal static readonly DotMethodFP[] FORMAT = {
            new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY),
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(
                    "a string-type format",
                    EPLExpressionParamType.SPECIFIC,
                    typeof(string),
                    typeof(DateFormat),
                    typeof(DateTimeFormat)))
        };
    }
} // end of namespace