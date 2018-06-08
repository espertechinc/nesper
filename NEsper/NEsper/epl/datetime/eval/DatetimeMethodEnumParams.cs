///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.util;
using com.espertech.esper.epl.methodbase;
using com.espertech.esper.epl.util;

namespace com.espertech.esper.epl.datetime.eval
{
    public class DatetimeMethodEnumParams {
    
        public static readonly DotMethodFP[] WITHTIME = new DotMethodFP[] {
                    new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam("an integer-type hour", EPLExpressionParamType.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type minute", EPLExpressionParamType.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type second", EPLExpressionParamType.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type millis", EPLExpressionParamType.SPECIFIC, typeof(int)))
                };
    
        public static readonly DotMethodFP[] WITHDATE = new DotMethodFP[] {
                    new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam("an integer-type year", EPLExpressionParamType.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type month", EPLExpressionParamType.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type day", EPLExpressionParamType.SPECIFIC, typeof(int)))
                };
    
        public static readonly DotMethodFP[] PLUSMINUS = new DotMethodFP[] {
                    new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(0, "a numeric-type millisecond", EPLExpressionParamType.NUMERIC)),
                    new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam("a time period", EPLExpressionParamType.SPECIFIC, typeof(TimePeriod)))
                };
    
        public static readonly DotMethodFP[] CALFIELD = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam("a string-type calendar field name", EPLExpressionParamType.SPECIFIC, typeof(String))),
                };
    
        public static readonly DotMethodFP[] CALFIELD_PLUS_INT = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam("a string-type calendar field name", EPLExpressionParamType.SPECIFIC, typeof(String)),
                                new DotMethodFPParam("an integer-type value", EPLExpressionParamType.SPECIFIC, typeof(int))),
                };
    
        public static readonly DotMethodFP[] NOPARAM = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY)
                };
    
        public static readonly DotMethodFP[] BETWEEN = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam("a date-time type", EPLExpressionParamType.DATETIME, null),
                                new DotMethodFPParam("a date-time type", EPLExpressionParamType.DATETIME, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam("a date-time type", EPLExpressionParamType.DATETIME, null),
                                new DotMethodFPParam("a date-time type", EPLExpressionParamType.DATETIME, null),
                                new DotMethodFPParam("bool", EPLExpressionParamType.BOOLEAN, null),
                                new DotMethodFPParam("bool", EPLExpressionParamType.BOOLEAN, null)),
                };
    
        /// <summary>Interval. </summary>
    
        public static readonly String INPUT_INTERVAL = "timestamp or timestamped-event";
        public static readonly String INPUT_INTERVAL_START = "interval start value";
        public static readonly String INPUT_INTERVAL_FINISHES = "interval finishes value";
    
        public static readonly DotMethodFP[] INTERVAL_BEFORE_AFTER = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                            new DotMethodFPParam(INPUT_INTERVAL_START, EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                            new DotMethodFPParam(INPUT_INTERVAL_START, EPLExpressionParamType.TIME_PERIOD_OR_SEC, null),
                            new DotMethodFPParam(INPUT_INTERVAL_FINISHES, EPLExpressionParamType.TIME_PERIOD_OR_SEC, null))
                        };
    
        public static readonly DotMethodFP[] INTERVAL_COINCIDES = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                            new DotMethodFPParam("threshold for start and end value", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                            new DotMethodFPParam("threshold for start value", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null),
                            new DotMethodFPParam("threshold for end value", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null))
                        };
    
        public static readonly DotMethodFP[] INTERVAL_DURING_INCLUDES = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                                new DotMethodFPParam("maximum distance interval both start and end", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                                new DotMethodFPParam("minimum distance interval both start and end", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("maximum distance interval both start and end", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                                new DotMethodFPParam("minimum distance start", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("maximum distance start", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("minimum distance end", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("maximum distance end", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        };
    
        public static readonly DotMethodFP[] INTERVAL_DURING_OVERLAPS_OVERLAPBY = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                                new DotMethodFPParam("maximum distance interval both start and end", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                                new DotMethodFPParam("minimum distance interval both start and end", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("maximum distance interval both start and end", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        };
    
        public static readonly DotMethodFP[] INTERVAL_FINISHES_FINISHEDBY = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                                new DotMethodFPParam("maximum distance between end timestamps", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        };
    
        public static readonly DotMethodFP[] INTERVAL_STARTS_STARTEDBY = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                                new DotMethodFPParam("maximum distance between start timestamps", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        };
    
        public static readonly DotMethodFP[] INTERVAL_MEETS_METBY = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, EPLExpressionParamType.ANY, null),
                                new DotMethodFPParam("maximum distance between start and end timestamps", EPLExpressionParamType.TIME_PERIOD_OR_SEC, null)),
                        };

        public static readonly DotMethodFP[] FORMAT = new DotMethodFP[]{
            new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY),
            new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam("a string-type format", EPLExpressionParamType.SPECIFIC, typeof(string)))
        };
    }
}
