///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class QueryGraphRangeEnum
    {
        /// <summary>
        ///     Less (&lt;).
        /// </summary>
        public static readonly QueryGraphRangeEnum LESS = new QueryGraphRangeEnum(false, "<");

        /// <summary>
        ///     Less or equal (&lt;=).
        /// </summary>
        public static readonly QueryGraphRangeEnum LESS_OR_EQUAL = new QueryGraphRangeEnum(false, "<=");

        /// <summary>
        ///     Greater or equal (&gt;=).
        /// </summary>
        public static readonly QueryGraphRangeEnum GREATER_OR_EQUAL = new QueryGraphRangeEnum(false, ">=");

        /// <summary>
        ///     Greater (&gt;).
        /// </summary>
        public static readonly QueryGraphRangeEnum GREATER = new QueryGraphRangeEnum(false, ">");

        /// <summary>
        ///     Range contains neither endpoint, i.e. (a,b)
        /// </summary>
        public static readonly QueryGraphRangeEnum RANGE_OPEN = new QueryGraphRangeEnum(true, "(,)");

        /// <summary>
        ///     Range contains low and high endpoint, i.e. [a,b]
        /// </summary>
        public static readonly QueryGraphRangeEnum RANGE_CLOSED = new QueryGraphRangeEnum(true, "[,]");

        /// <summary>
        ///     Range includes low endpoint but not high endpoint, i.e. [a,b)
        /// </summary>
        public static readonly QueryGraphRangeEnum RANGE_HALF_OPEN = new QueryGraphRangeEnum(true, "[,)");

        /// <summary>
        ///     Range includes high endpoint but not low endpoint, i.e. (a,b]
        /// </summary>
        public static readonly QueryGraphRangeEnum RANGE_HALF_CLOSED = new QueryGraphRangeEnum(true, "(,]");

        /// <summary>
        ///     Inverted-Range contains neither endpoint, i.e. (a,b)
        /// </summary>
        public static readonly QueryGraphRangeEnum NOT_RANGE_OPEN = new QueryGraphRangeEnum(true, "-(,)");

        /// <summary>
        ///     Inverted-Range contains low and high endpoint, i.e. [a,b]
        /// </summary>
        public static readonly QueryGraphRangeEnum NOT_RANGE_CLOSED = new QueryGraphRangeEnum(true, "-[,]");

        /// <summary>
        ///     Inverted-Range includes low endpoint but not high endpoint, i.e. [a,b)
        /// </summary>
        public static readonly QueryGraphRangeEnum NOT_RANGE_HALF_OPEN = new QueryGraphRangeEnum(true, "-[,)");

        /// <summary>
        ///     Inverted-Range includes high endpoint but not low endpoint, i.e. (a,b]
        /// </summary>
        public static readonly QueryGraphRangeEnum NOT_RANGE_HALF_CLOSED = new QueryGraphRangeEnum(true, "-(,])");

        private QueryGraphRangeEnum(
            bool range,
            string stringOp)
        {
            IsRange = range;
            StringOp = StringOp;
        }

        public string StringOp { get; }

        public bool IsRange { get; }

        public bool IsIncludeStart {
            get {
                if (!IsRange) {
                    throw new UnsupportedOperationException("Cannot determine endpoint-start included for op " + this);
                }

                return this == RANGE_HALF_OPEN ||
                       this == RANGE_CLOSED ||
                       this == NOT_RANGE_HALF_OPEN ||
                       this == NOT_RANGE_CLOSED;
            }
        }

        public bool IsIncludeEnd {
            get {
                if (!IsRange) {
                    throw new UnsupportedOperationException("Cannot determine endpoint-end included for op " + this);
                }

                return this == RANGE_HALF_CLOSED ||
                       this == RANGE_CLOSED ||
                       this == NOT_RANGE_HALF_CLOSED ||
                       this == NOT_RANGE_CLOSED;
            }
        }

        public static QueryGraphRangeEnum MapFrom(FilterOperator op)
        {
            if (op == FilterOperator.GREATER) {
                return GREATER;
            }

            if (op == FilterOperator.GREATER_OR_EQUAL) {
                return GREATER_OR_EQUAL;
            }

            if (op == FilterOperator.LESS) {
                return LESS;
            }

            if (op == FilterOperator.LESS_OR_EQUAL) {
                return LESS_OR_EQUAL;
            }

            if (op == FilterOperator.RANGE_OPEN) {
                return RANGE_OPEN;
            }

            if (op == FilterOperator.RANGE_HALF_CLOSED) {
                return RANGE_HALF_CLOSED;
            }

            if (op == FilterOperator.RANGE_HALF_OPEN) {
                return RANGE_HALF_OPEN;
            }

            if (op == FilterOperator.RANGE_CLOSED) {
                return RANGE_CLOSED;
            }

            if (op == FilterOperator.NOT_RANGE_OPEN) {
                return NOT_RANGE_OPEN;
            }

            if (op == FilterOperator.NOT_RANGE_HALF_CLOSED) {
                return NOT_RANGE_HALF_CLOSED;
            }

            if (op == FilterOperator.NOT_RANGE_HALF_OPEN) {
                return NOT_RANGE_HALF_OPEN;
            }

            if (op == FilterOperator.NOT_RANGE_CLOSED) {
                return NOT_RANGE_CLOSED;
            }

            return null;
        }

        public static QueryGraphRangeEnum MapFrom(RelationalOpEnum relationalOpEnum)
        {
            if (relationalOpEnum == RelationalOpEnum.GE) {
                return GREATER_OR_EQUAL;
            }

            if (relationalOpEnum == RelationalOpEnum.GT) {
                return GREATER;
            }

            if (relationalOpEnum == RelationalOpEnum.LT) {
                return LESS;
            }

            if (relationalOpEnum == RelationalOpEnum.LE) {
                return LESS_OR_EQUAL;
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
                        return RANGE_CLOSED;
                    }

                    return RANGE_HALF_OPEN;
                }

                if (includeEnd) {
                    return RANGE_HALF_CLOSED;
                }

                return RANGE_OPEN;
            }

            if (includeStart) {
                if (includeEnd) {
                    return NOT_RANGE_CLOSED;
                }

                return NOT_RANGE_HALF_OPEN;
            }

            if (includeEnd) {
                return NOT_RANGE_HALF_CLOSED;
            }

            return NOT_RANGE_OPEN;
        }

        public bool IsRangeInverted()
        {
            return IsRange &&
                   (this == NOT_RANGE_HALF_CLOSED ||
                    this == NOT_RANGE_HALF_OPEN ||
                    this == NOT_RANGE_OPEN ||
                    this == NOT_RANGE_CLOSED);
        }
    }
} // end of namespace