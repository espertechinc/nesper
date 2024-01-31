///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public enum QueryGraphRangeEnum
    {
        /// <summary>
        ///     Less (&lt;).
        /// </summary>
        LESS,

        /// <summary>
        ///     Less or equal (&lt;=).
        /// </summary>
        LESS_OR_EQUAL,

        /// <summary>
        ///     Greater or equal (&gt;=).
        /// </summary>
        GREATER_OR_EQUAL,

        /// <summary>
        ///     Greater (&gt;).
        /// </summary>
        GREATER,

        /// <summary>
        ///     Range contains neither endpoint, i.e. (a,b)
        /// </summary>
        RANGE_OPEN,

        /// <summary>
        ///     Range contains low and high endpoint, i.e. [a,b]
        /// </summary>
        RANGE_CLOSED,

        /// <summary>
        ///     Range includes low endpoint but not high endpoint, i.e. [a,b)
        /// </summary>
        RANGE_HALF_OPEN,

        /// <summary>
        ///     Range includes high endpoint but not low endpoint, i.e. (a,b]
        /// </summary>
        RANGE_HALF_CLOSED,

        /// <summary>
        ///     Inverted-Range contains neither endpoint, i.e. (a,b)
        /// </summary>
        NOT_RANGE_OPEN,

        /// <summary>
        ///     Inverted-Range contains low and high endpoint, i.e. [a,b]
        /// </summary>
        NOT_RANGE_CLOSED,

        /// <summary>
        ///     Inverted-Range includes low endpoint but not high endpoint, i.e. [a,b)
        /// </summary>
        NOT_RANGE_HALF_OPEN,

        /// <summary>
        ///     Inverted-Range includes high endpoint but not low endpoint, i.e. (a,b]
        /// </summary>
        NOT_RANGE_HALF_CLOSED
    }

    public static class QueryGraphRangeEnumExtensions
    {
        public static string StringOp(this QueryGraphRangeEnum value)
        {
            switch (value) {
                case QueryGraphRangeEnum.LESS:
                    return "<";

                case QueryGraphRangeEnum.LESS_OR_EQUAL:
                    return "<=";

                case QueryGraphRangeEnum.GREATER_OR_EQUAL:
                    return ">=";

                case QueryGraphRangeEnum.GREATER:
                    return ">";

                case QueryGraphRangeEnum.RANGE_OPEN:
                    return "(,)";

                case QueryGraphRangeEnum.RANGE_CLOSED:
                    return "[,]";

                case QueryGraphRangeEnum.RANGE_HALF_OPEN:
                    return "[,)";

                case QueryGraphRangeEnum.RANGE_HALF_CLOSED:
                    return "(,]";

                case QueryGraphRangeEnum.NOT_RANGE_OPEN:
                    return "-(,)";

                case QueryGraphRangeEnum.NOT_RANGE_CLOSED:
                    return "-[,]";

                case QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN:
                    return "-[,)";

                case QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED:
                    return "-(,])";
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        public static bool IsRange(this QueryGraphRangeEnum value)
        {
            switch (value) {
                case QueryGraphRangeEnum.LESS:
                case QueryGraphRangeEnum.LESS_OR_EQUAL:
                case QueryGraphRangeEnum.GREATER_OR_EQUAL:
                case QueryGraphRangeEnum.GREATER:
                    return false;

                case QueryGraphRangeEnum.RANGE_OPEN:
                case QueryGraphRangeEnum.RANGE_CLOSED:
                case QueryGraphRangeEnum.RANGE_HALF_OPEN:
                case QueryGraphRangeEnum.RANGE_HALF_CLOSED:
                case QueryGraphRangeEnum.NOT_RANGE_OPEN:
                case QueryGraphRangeEnum.NOT_RANGE_CLOSED:
                case QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN:
                case QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED:
                    return true;
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        public static bool IsIncludeStart(this QueryGraphRangeEnum value)
        {
            if (!value.IsRange()) {
                throw new UnsupportedOperationException("Cannot determine endpoint-start included for op " + value);
            }

            return value == QueryGraphRangeEnum.RANGE_HALF_OPEN ||
                   value == QueryGraphRangeEnum.RANGE_CLOSED ||
                   value == QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN ||
                   value == QueryGraphRangeEnum.NOT_RANGE_CLOSED;
        }

        public static bool IsIncludeEnd(this QueryGraphRangeEnum value)
        {
            if (!value.IsRange()) {
                throw new UnsupportedOperationException("Cannot determine endpoint-end included for op " + value);
            }

            return value == QueryGraphRangeEnum.RANGE_HALF_CLOSED ||
                   value == QueryGraphRangeEnum.RANGE_CLOSED ||
                   value == QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED ||
                   value == QueryGraphRangeEnum.NOT_RANGE_CLOSED;
        }

        public static QueryGraphRangeEnum? MapFrom(FilterOperator op)
        {
            if (op == FilterOperator.GREATER) {
                return QueryGraphRangeEnum.GREATER;
            }

            if (op == FilterOperator.GREATER_OR_EQUAL) {
                return QueryGraphRangeEnum.GREATER_OR_EQUAL;
            }

            if (op == FilterOperator.LESS) {
                return QueryGraphRangeEnum.LESS;
            }

            if (op == FilterOperator.LESS_OR_EQUAL) {
                return QueryGraphRangeEnum.LESS_OR_EQUAL;
            }

            if (op == FilterOperator.RANGE_OPEN) {
                return QueryGraphRangeEnum.RANGE_OPEN;
            }

            if (op == FilterOperator.RANGE_HALF_CLOSED) {
                return QueryGraphRangeEnum.RANGE_HALF_CLOSED;
            }

            if (op == FilterOperator.RANGE_HALF_OPEN) {
                return QueryGraphRangeEnum.RANGE_HALF_OPEN;
            }

            if (op == FilterOperator.RANGE_CLOSED) {
                return QueryGraphRangeEnum.RANGE_CLOSED;
            }

            if (op == FilterOperator.NOT_RANGE_OPEN) {
                return QueryGraphRangeEnum.NOT_RANGE_OPEN;
            }

            if (op == FilterOperator.NOT_RANGE_HALF_CLOSED) {
                return QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED;
            }

            if (op == FilterOperator.NOT_RANGE_HALF_OPEN) {
                return QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN;
            }

            if (op == FilterOperator.NOT_RANGE_CLOSED) {
                return QueryGraphRangeEnum.NOT_RANGE_CLOSED;
            }

            //throw new ArgumentException("Failed to map code " + op);
            return null;
        }

        public static QueryGraphRangeEnum MapFrom(RelationalOpEnum relationalOpEnum)
        {
            if (relationalOpEnum == RelationalOpEnum.GE) {
                return QueryGraphRangeEnum.GREATER_OR_EQUAL;
            }

            if (relationalOpEnum == RelationalOpEnum.GT) {
                return QueryGraphRangeEnum.GREATER;
            }

            if (relationalOpEnum == RelationalOpEnum.LT) {
                return QueryGraphRangeEnum.LESS;
            }

            if (relationalOpEnum == RelationalOpEnum.LE) {
                return QueryGraphRangeEnum.LESS_OR_EQUAL;
            }

            throw new ArgumentException("Failed to map code " + relationalOpEnum);
        }

        public static QueryGraphRangeEnum GetRangeOp(
            bool includeStart,
            bool includeEnd,
            bool isInverted)
        {
            if (!isInverted) {
                if (includeStart) {
                    if (includeEnd) {
                        return QueryGraphRangeEnum.RANGE_CLOSED;
                    }

                    return QueryGraphRangeEnum.RANGE_HALF_OPEN;
                }

                if (includeEnd) {
                    return QueryGraphRangeEnum.RANGE_HALF_CLOSED;
                }

                return QueryGraphRangeEnum.RANGE_OPEN;
            }

            if (includeStart) {
                if (includeEnd) {
                    return QueryGraphRangeEnum.NOT_RANGE_CLOSED;
                }

                return QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN;
            }

            if (includeEnd) {
                return QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED;
            }

            return QueryGraphRangeEnum.NOT_RANGE_OPEN;
        }

        public static bool IsRangeInverted(this QueryGraphRangeEnum value)
        {
            return value.IsRange() &&
                   (value == QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED ||
                    value == QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN ||
                    value == QueryGraphRangeEnum.NOT_RANGE_OPEN ||
                    value == QueryGraphRangeEnum.NOT_RANGE_CLOSED);
        }
    }
} // end of namespace