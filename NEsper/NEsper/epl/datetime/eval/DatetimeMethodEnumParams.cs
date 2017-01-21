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

namespace com.espertech.esper.epl.datetime.eval
{
    public class DatetimeMethodEnumParams {
    
        public static readonly DotMethodFP[] WITHTIME = new DotMethodFP[] {
                    new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam("an integer-type hour", DotMethodFPParamTypeEnum.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type minute", DotMethodFPParamTypeEnum.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type second", DotMethodFPParamTypeEnum.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type millis", DotMethodFPParamTypeEnum.SPECIFIC, typeof(int)))
                };
    
        public static readonly DotMethodFP[] WITHDATE = new DotMethodFP[] {
                    new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam("an integer-type year", DotMethodFPParamTypeEnum.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type month", DotMethodFPParamTypeEnum.SPECIFIC, typeof(int)),
                            new DotMethodFPParam("an integer-type day", DotMethodFPParamTypeEnum.SPECIFIC, typeof(int)))
                };
    
        public static readonly DotMethodFP[] PLUSMINUS = new DotMethodFP[] {
                    new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(0, "a numeric-type millisecond", DotMethodFPParamTypeEnum.NUMERIC)),
                    new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam("a time period", DotMethodFPParamTypeEnum.SPECIFIC, typeof(TimePeriod)))
                };
    
        public static readonly DotMethodFP[] CALFIELD = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam("a string-type calendar field name", DotMethodFPParamTypeEnum.SPECIFIC, typeof(String))),
                };
    
        public static readonly DotMethodFP[] CALFIELD_PLUS_INT = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam("a string-type calendar field name", DotMethodFPParamTypeEnum.SPECIFIC, typeof(String)),
                                new DotMethodFPParam("an integer-type value", DotMethodFPParamTypeEnum.SPECIFIC, typeof(int))),
                };
    
        public static readonly DotMethodFP[] NOPARAM = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY)
                };
    
        public static readonly DotMethodFP[] BETWEEN = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam("a date-time type", DotMethodFPParamTypeEnum.DATETIME, null),
                                new DotMethodFPParam("a date-time type", DotMethodFPParamTypeEnum.DATETIME, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam("a date-time type", DotMethodFPParamTypeEnum.DATETIME, null),
                                new DotMethodFPParam("a date-time type", DotMethodFPParamTypeEnum.DATETIME, null),
                                new DotMethodFPParam("bool", DotMethodFPParamTypeEnum.BOOLEAN, null),
                                new DotMethodFPParam("bool", DotMethodFPParamTypeEnum.BOOLEAN, null)),
                };
    
        /// <summary>Interval. </summary>
    
        public static readonly String INPUT_INTERVAL = "timestamp or timestamped-event";
        public static readonly String INPUT_INTERVAL_START = "interval start value";
        public static readonly String INPUT_INTERVAL_FINISHES = "interval finishes value";
    
        public static readonly DotMethodFP[] INTERVAL_BEFORE_AFTER = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                            new DotMethodFPParam(INPUT_INTERVAL_START, DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                            new DotMethodFPParam(INPUT_INTERVAL_START, DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null),
                            new DotMethodFPParam(INPUT_INTERVAL_FINISHES, DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null))
                        };
    
        public static readonly DotMethodFP[] INTERVAL_COINCIDES = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                            new DotMethodFPParam("threshold for start and end value", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                            new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                            new DotMethodFPParam("threshold for start value", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null),
                            new DotMethodFPParam("threshold for end value", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null))
                        };
    
        public static readonly DotMethodFP[] INTERVAL_DURING_INCLUDES = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                                new DotMethodFPParam("maximum distance interval both start and end", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                                new DotMethodFPParam("minimum distance interval both start and end", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("maximum distance interval both start and end", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                                new DotMethodFPParam("minimum distance start", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("maximum distance start", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("minimum distance end", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("maximum distance end", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        };
    
        public static readonly DotMethodFP[] INTERVAL_DURING_OVERLAPS_OVERLAPBY = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                                new DotMethodFPParam("maximum distance interval both start and end", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                                new DotMethodFPParam("minimum distance interval both start and end", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null),
                                new DotMethodFPParam("maximum distance interval both start and end", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        };
    
        public static readonly DotMethodFP[] INTERVAL_FINISHES_FINISHEDBY = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                                new DotMethodFPParam("maximum distance between end timestamps", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        };
    
        public static readonly DotMethodFP[] INTERVAL_STARTS_STARTEDBY = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                                new DotMethodFPParam("maximum distance between start timestamps", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        };
    
        public static readonly DotMethodFP[] INTERVAL_MEETS_METBY = new DotMethodFP[] {
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null)),
                        new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY,
                                new DotMethodFPParam(INPUT_INTERVAL, DotMethodFPParamTypeEnum.ANY, null),
                                new DotMethodFPParam("maximum distance between start and end timestamps", DotMethodFPParamTypeEnum.TIME_PERIOD_OR_SEC, null)),
                        };
    }
}
