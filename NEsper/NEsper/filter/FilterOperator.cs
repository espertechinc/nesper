///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Defines the different operator types available for event filters.
    /// <para>
    /// Mathematical notation for defining ranges of floating point numbers is used as defined below:
    /// <list>
    /// <item>[a,b] a closed range from value a to value b with the end-points a and b included in the range</item>
    /// <item>(a,b) an open range from value a to value b with the end-points a and b not included in the range</item>
    /// <item>[a,b) a half-open range from value a to value b with the end-point a included and end-point b not included in the range</item>
    /// <item>(a,b] a half-open range from value a to value b with the end-point a not included and end-point b included in the range</item>
    /// </list>
    /// </para>
    /// </summary>
    public enum FilterOperator
    {
        /// <summary> Exact matches (=).</summary>
        EQUAL,
        /// <summary> Exact not matches (!=).</summary>
        NOT_EQUAL,
        /// <summary>Exact matches allowing null (is).</summary>
        IS,
        /// <summary>Exact not matches allowing null (is not)</summary>
        IS_NOT,
        /// <summary> Less (&lt;).</summary>
        LESS,
        /// <summary> Less or equal (&lt;=).</summary>
        LESS_OR_EQUAL,
        /// <summary> Greater or equal (&gt;=).</summary>
        GREATER_OR_EQUAL,
        /// <summary> Greater (&gt;).</summary>
        GREATER,
        /// <summary> Range contains neither endpoint, i.e. (a,b)</summary>
        RANGE_OPEN,
        /// <summary> Range contains low and high endpoint, i.e. [a,b]</summary>
        RANGE_CLOSED,
        /// <summary> Range includes low endpoint but not high endpoint, i.e. [a,b)</summary>
        RANGE_HALF_OPEN,
        /// <summary> Range includes high endpoint but not low endpoint, i.e. (a,b]</summary>
        RANGE_HALF_CLOSED,
        /// <summary> Inverted-Range contains neither endpoint, i.e. (a,b)</summary>
        NOT_RANGE_OPEN,
        /// <summary> Inverted-Range contains low and high endpoint, i.e. [a,b]</summary>
        NOT_RANGE_CLOSED,
        /// <summary> Inverted-Range includes low endpoint but not high endpoint, i.e. [a,b)</summary>
        NOT_RANGE_HALF_OPEN,
        /// <summary> Inverted-Range includes high endpoint but not low endpoint, i.e. (a,b]</summary>
        NOT_RANGE_HALF_CLOSED,
        /// <summary> List of values using the 'in' operator</summary>
        IN_LIST_OF_VALUES,
        /// <summary> Not-in list of values using the 'not in' operator</summary>
        NOT_IN_LIST_OF_VALUES,
        /// <summary> Advanced-index</summary>
        ADVANCED_INDEX,
        /// <summary> Boolean expression filter operator</summary>
        BOOLEAN_EXPRESSION
    };

    /// <summary>
    /// Contains static methods useful for help with FilterOperators.
    /// </summary>

    public static class FilterOperatorExtensions
    {
        /// <summary> Returns true for all range operators, false if not a range operator.</summary>
        /// <returns> true for ranges, false for anyting else
        /// </returns>
        public static bool IsRangeOperator(this FilterOperator op)
        {
            if ((op == FilterOperator.RANGE_CLOSED) ||
                (op == FilterOperator.RANGE_OPEN) ||
                (op == FilterOperator.RANGE_HALF_OPEN) ||
                (op == FilterOperator.RANGE_HALF_CLOSED))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true for inverted range operators, false if not an inverted range operator.
        /// </summary>
        /// <returns>true for inverted ranges, false for anyting else</returns>
        public static bool IsInvertedRangeOperator(this FilterOperator op)
        {
            if ((op == FilterOperator.NOT_RANGE_CLOSED) ||
                (op == FilterOperator.NOT_RANGE_OPEN) ||
                (op == FilterOperator.NOT_RANGE_HALF_OPEN) ||
                (op == FilterOperator.NOT_RANGE_HALF_CLOSED))
            {
                return true;
            }
            return false;
        }

        /// <summary> Returns true for relational comparison operators which excludes the = equals operator, else returns false.</summary>
        /// <returns> true for lesser or greater -type operators, false for anyting else
        /// </returns>
        public static bool IsComparisonOperator(this FilterOperator op)
        {
            if ((op == FilterOperator.LESS) ||
                (op == FilterOperator.LESS_OR_EQUAL) ||
                (op == FilterOperator.GREATER) ||
                (op == FilterOperator.GREATER_OR_EQUAL))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse the range operator from booleans describing whether the Start or end values are exclusive.
        /// </summary>
        /// <param name="isInclusiveFirst">true if low endpoint is inclusive, false if not</param>
        /// <param name="isInclusiveLast">true if high endpoint is inclusive, false if not</param>
        /// <param name="isNot">if set to <c>true</c> [is not].</param>
        /// <returns>
        /// FilterOperator for the combination inclusive or exclusive
        /// </returns>
        public static FilterOperator ParseRangeOperator(bool isInclusiveFirst, bool isInclusiveLast, bool isNot)
        {
            if (isInclusiveFirst && isInclusiveLast)
            {
                return isNot ? FilterOperator.NOT_RANGE_CLOSED : FilterOperator.RANGE_CLOSED;
            }
            if (isInclusiveFirst)
            {
                return isNot ? FilterOperator.NOT_RANGE_HALF_OPEN : FilterOperator.RANGE_HALF_OPEN;
            }
            if (isInclusiveLast)
            {
                return isNot ? FilterOperator.NOT_RANGE_HALF_CLOSED : FilterOperator.RANGE_HALF_CLOSED;
            }
            return isNot ? FilterOperator.NOT_RANGE_OPEN : FilterOperator.RANGE_OPEN;
        }

        public static string GetTextualOp(this FilterOperator op)
        {
            switch (op)
            {
                case FilterOperator.EQUAL:
                    return ("=");
                case FilterOperator.NOT_EQUAL:
                    return ("!=");
                case FilterOperator.IS:
                    return ("is");
                case FilterOperator.IS_NOT:
                    return ("is not");
                case FilterOperator.LESS:
                    return ("<");
                case FilterOperator.LESS_OR_EQUAL:
                    return ("<=");
                case FilterOperator.GREATER_OR_EQUAL:
                    return (">=");
                case FilterOperator.GREATER:
                    return (">");
                case FilterOperator.RANGE_OPEN:
                    return (": return(,)");
                case FilterOperator.RANGE_CLOSED:
                    return ("[,]");
                case FilterOperator.RANGE_HALF_OPEN:
                    return ("[,)");
                case FilterOperator.RANGE_HALF_CLOSED:
                    return (": return(,]");
                case FilterOperator.NOT_RANGE_OPEN:
                    return ("-: return(,)");
                case FilterOperator.NOT_RANGE_CLOSED:
                    return ("-[,]");
                case FilterOperator.NOT_RANGE_HALF_OPEN:
                    return ("-[,)");
                case FilterOperator.NOT_RANGE_HALF_CLOSED:
                    return ("-: return(,]");
                case FilterOperator.IN_LIST_OF_VALUES:
                    return ("in");
                case FilterOperator.NOT_IN_LIST_OF_VALUES:
                    return ("!in");
                case FilterOperator.ADVANCED_INDEX:
                    return ("ai");
                case FilterOperator.BOOLEAN_EXPRESSION:
                    return ("boolean_expr");
            }

            throw new ArgumentException();
        }

        public static FilterOperator ReversedRelationalOp(this FilterOperator op)
        {
            switch (op)
            {
                case FilterOperator.LESS:
                    return FilterOperator.GREATER;
                case FilterOperator.LESS_OR_EQUAL:
                    return FilterOperator.GREATER_OR_EQUAL;
                case FilterOperator.GREATER:
                    return FilterOperator.LESS;
                case FilterOperator.GREATER_OR_EQUAL:
                    return FilterOperator.LESS_OR_EQUAL;
            }
            throw new ArgumentException("Not a relational operator: " + op);
        }
    }
}
