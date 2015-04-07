///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.filter;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.join.plan
{
    public enum QueryGraphRangeEnum
    {
        /// <summary>Less (&lt;). </summary>
        LESS,

        /// <summary>Less or equal (&lt;=). </summary>
        LESS_OR_EQUAL,

        /// <summary>Greater or equal (&gt;=). </summary>
        GREATER_OR_EQUAL,

        /// <summary>Greater (&gt;). </summary>
        GREATER,

        /// <summary>Range contains neither endpoint, i.e. (a,b) </summary>
        RANGE_OPEN,

        /// <summary>Range contains low and high endpoint, i.e. [a,b] </summary>
        RANGE_CLOSED,

        /// <summary>Range includes low endpoint but not high endpoint, i.e. [a,b) </summary>
        RANGE_HALF_OPEN,

        /// <summary>Range includes high endpoint but not low endpoint, i.e. (a,b] </summary>
        RANGE_HALF_CLOSED,

        /// <summary>Inverted-Range contains neither endpoint, i.e. (a,b) </summary>
        NOT_RANGE_OPEN,

        /// <summary>Inverted-Range contains low and high endpoint, i.e. [a,b] </summary>
        NOT_RANGE_CLOSED,

        /// <summary>Inverted-Range includes low endpoint but not high endpoint, i.e. [a,b) </summary>
        NOT_RANGE_HALF_OPEN,

        /// <summary>Inverted-Range includes high endpoint but not low endpoint, i.e. (a,b] </summary>
        NOT_RANGE_HALF_CLOSED
    }

    public static class QueryGraphRangeEnumExtensions
    {
        public static QueryGraphRangeEnum MapFrom(this FilterOperator op)
        {
            switch (op)
            {
                case FilterOperator.GREATER:
                    return QueryGraphRangeEnum.GREATER;
                case FilterOperator.GREATER_OR_EQUAL:
                    return QueryGraphRangeEnum.GREATER_OR_EQUAL;
                case FilterOperator.LESS:
                    return QueryGraphRangeEnum.LESS;
                case FilterOperator.LESS_OR_EQUAL:
                    return QueryGraphRangeEnum.LESS_OR_EQUAL;
                case FilterOperator.RANGE_OPEN:
                    return QueryGraphRangeEnum.RANGE_OPEN;
                case FilterOperator.RANGE_HALF_CLOSED:
                    return QueryGraphRangeEnum.RANGE_HALF_CLOSED;
                case FilterOperator.RANGE_HALF_OPEN:
                    return QueryGraphRangeEnum.RANGE_HALF_OPEN;
                case FilterOperator.RANGE_CLOSED:
                    return QueryGraphRangeEnum.RANGE_CLOSED;
                case FilterOperator.NOT_RANGE_OPEN:
                    return QueryGraphRangeEnum.NOT_RANGE_OPEN;
                case FilterOperator.NOT_RANGE_HALF_CLOSED:
                    return QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED;
                case FilterOperator.NOT_RANGE_HALF_OPEN:
                    return QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN;
                case FilterOperator.NOT_RANGE_CLOSED:
                    return QueryGraphRangeEnum.NOT_RANGE_CLOSED;
                default:
                    throw new ArgumentException("invalid filter", "op");
            }
        }

        public static QueryGraphRangeEnum MapFrom(RelationalOpEnum relationalOpEnum)
        {
            if (relationalOpEnum == RelationalOpEnum.GE)
            {
                return QueryGraphRangeEnum.GREATER_OR_EQUAL;
            }
            if (relationalOpEnum == RelationalOpEnum.GT)
            {
                return QueryGraphRangeEnum.GREATER;
            }
            if (relationalOpEnum == RelationalOpEnum.LT)
            {
                return QueryGraphRangeEnum.LESS;
            }
            if (relationalOpEnum == RelationalOpEnum.LE)
            {
                return QueryGraphRangeEnum.LESS_OR_EQUAL;
            }

            throw new ArgumentException("Failed to map code " + relationalOpEnum);
        }

        public static bool IsRange(this QueryGraphRangeEnum @enum)
        {
            switch (@enum)
            {
                case QueryGraphRangeEnum.LESS:
                    return false;
                case QueryGraphRangeEnum.LESS_OR_EQUAL:
                    return false;
                case QueryGraphRangeEnum.GREATER_OR_EQUAL:
                    return false;
                case QueryGraphRangeEnum.GREATER:
                    return false;
                case QueryGraphRangeEnum.RANGE_OPEN:
                    return true;
                case QueryGraphRangeEnum.RANGE_CLOSED:
                    return true;
                case QueryGraphRangeEnum.RANGE_HALF_OPEN:
                    return true;
                case QueryGraphRangeEnum.RANGE_HALF_CLOSED:
                    return true;
                case QueryGraphRangeEnum.NOT_RANGE_OPEN:
                    return true;
                case QueryGraphRangeEnum.NOT_RANGE_CLOSED:
                    return true;
                case QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN:
                    return true;
                case QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED:
                    return true;
                default:
                    throw new ArgumentException("invalid value", "enum");
            }
        }

        public static bool IsIncludeStart(this QueryGraphRangeEnum @enum)
        {
            if (!@enum.IsRange())
            {
                throw new UnsupportedOperationException("Cannot determine endpoint-start included for op " + @enum);
            }
            return @enum == QueryGraphRangeEnum.RANGE_HALF_OPEN ||
                   @enum == QueryGraphRangeEnum.RANGE_CLOSED ||
                   @enum == QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN ||
                   @enum == QueryGraphRangeEnum.NOT_RANGE_CLOSED;
        }

        public static bool IsIncludeEnd(this QueryGraphRangeEnum @enum)
        {
            if (!@enum.IsRange())
            {
                throw new UnsupportedOperationException("Cannot determine endpoint-end included for op " + @enum);
            }
            return @enum == QueryGraphRangeEnum.RANGE_HALF_CLOSED ||
                   @enum == QueryGraphRangeEnum.RANGE_CLOSED ||
                   @enum == QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED ||
                   @enum == QueryGraphRangeEnum.NOT_RANGE_CLOSED;
        }

        public static QueryGraphRangeEnum GetRangeOp(bool includeStart, bool includeEnd, bool isInverted)
        {
            if (!isInverted)
            {
                if (includeStart)
                {
                    return includeEnd
                        ? QueryGraphRangeEnum.RANGE_CLOSED
                        : QueryGraphRangeEnum.RANGE_HALF_OPEN;
                }

                return includeEnd
                    ? QueryGraphRangeEnum.RANGE_HALF_CLOSED
                    : QueryGraphRangeEnum.RANGE_OPEN;
            }

            if (includeStart)
            {
                return includeEnd
                    ? QueryGraphRangeEnum.NOT_RANGE_CLOSED
                    : QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN;
            }

            return includeEnd ?
                QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED :
                QueryGraphRangeEnum.NOT_RANGE_OPEN;
        }

        public static bool IsRangeInverted(this QueryGraphRangeEnum @enum)
        {
            return IsRange(@enum) &&
                   (@enum == QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED ||
                    @enum == QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN ||
                    @enum == QueryGraphRangeEnum.NOT_RANGE_OPEN ||
                    @enum == QueryGraphRangeEnum.NOT_RANGE_CLOSED);
        }

        public static string GetStringOp(this QueryGraphRangeEnum @enum)
        {
            switch (@enum)
            {
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
                default:
                    throw new ArgumentException("invalid value", "enum");
            }
        }
    }
}
