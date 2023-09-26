///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.util;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class EnumMethodEnumParams
    {
        public static readonly DotMethodFP[] NOOP_REVERSE = {
            new DotMethodFP(DotMethodFPInputEnum.ANY)
        };

        public static readonly DotMethodFP[] COUNTOF_FIRST_LAST = {
            new DotMethodFP(DotMethodFPInputEnum.ANY),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "predicate", EPLExpressionParamType.BOOLEAN)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(predicate, index)", EPLExpressionParamType.BOOLEAN)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(predicate, index, size)", EPLExpressionParamType.BOOLEAN))
        };

        public static readonly DotMethodFP[] TAKELAST = {
            new DotMethodFP(DotMethodFPInputEnum.ANY, new DotMethodFPParam(0, "count", EPLExpressionParamType.NUMERIC))
        };

        public static readonly DotMethodFP[] TAKE = {
            new DotMethodFP(DotMethodFPInputEnum.ANY, new DotMethodFPParam(0, "count", EPLExpressionParamType.NUMERIC))
        };

        public static readonly DotMethodFP[] AGGREGATE_FP = {
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(0, "initialization-value", EPLExpressionParamType.ANY),
                new DotMethodFPParam(2, "(result, next)", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(0, "initialization-value", EPLExpressionParamType.ANY),
                new DotMethodFPParam(3, "(result, next, index)", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(0, "initialization-value", EPLExpressionParamType.ANY),
                new DotMethodFPParam(4, "(result, next, index, size)", EPLExpressionParamType.ANY))
        };

        public static readonly DotMethodFP[] ALLOF_ANYOF = {
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "predicate", EPLExpressionParamType.BOOLEAN)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(predicate, index)", EPLExpressionParamType.BOOLEAN)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(predicate, index, size)", EPLExpressionParamType.BOOLEAN))
        };

        public static readonly DotMethodFP[] ORDERBY_DISTINCT_ARRAYOF_MOSTLEAST_MINMAX = {
            new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "value-selector", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(value-selector, index)", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(value-selector, index, size)", EPLExpressionParamType.ANY))
        };

        public static readonly DotMethodFP[] SELECTFROM_MINMAXBY = {
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "value-selector", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(value-selector, index)", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(value-selector, index, size)", EPLExpressionParamType.ANY))
        };

        public static readonly DotMethodFP[] AVERAGE_SUMOF = {
            new DotMethodFP(DotMethodFPInputEnum.SCALAR_NUMERIC),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "value-selector", EPLExpressionParamType.NUMERIC)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(value-selector, index)", EPLExpressionParamType.NUMERIC)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(value-selector, index, size)", EPLExpressionParamType.NUMERIC))
        };

        public static readonly DotMethodFP[] TOMAP = {
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "key-selector", EPLExpressionParamType.ANY),
                new DotMethodFPParam(1, "value-selector", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(key-selector, index)", EPLExpressionParamType.ANY),
                new DotMethodFPParam(2, "(value-selector, index)", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(key-selector, index, size)", EPLExpressionParamType.ANY),
                new DotMethodFPParam(3, "(value-selector, index, size)", EPLExpressionParamType.ANY))
        };

        public static readonly DotMethodFP[] GROUP = {
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "key-selector", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(key-selector, index)", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(key-selector, index, size)", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "key-selector", EPLExpressionParamType.ANY),
                new DotMethodFPParam(1, "value-selector", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(key-selector, index)", EPLExpressionParamType.ANY),
                new DotMethodFPParam(2, "(value-selector, index)", EPLExpressionParamType.ANY)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(key-selector, index, size)", EPLExpressionParamType.ANY),
                new DotMethodFPParam(3, "(value-selector, index, size)", EPLExpressionParamType.ANY))
        };

        public static readonly DotMethodFP[] WHERE_FP = {
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(1, "predicate", EPLExpressionParamType.BOOLEAN)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(2, "(predicate, index)", EPLExpressionParamType.BOOLEAN)),
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam(3, "(predicate, index, size)", EPLExpressionParamType.BOOLEAN))
        };

        public static readonly DotMethodFP[] SET_LOGIC_FP = {
            new DotMethodFP(DotMethodFPInputEnum.ANY, new DotMethodFPParam(0, "collection", EPLExpressionParamType.ANY))
        };

        public static readonly DotMethodFP[] SEQ_EQUALS_FP = {
            new DotMethodFP(
                DotMethodFPInputEnum.SCALAR_ANY,
                new DotMethodFPParam(0, "sequence", EPLExpressionParamType.ANY))
        };
    }
} // end of namespace