///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class SupportFilterSpecParamRange : FilterSpecParam
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="lookupable">is the lookupable</param>
        /// <param name="filterOperator">is the type of range operator</param>
        /// <param name="min">is the begin point of the range</param>
        /// <param name="max">is the end point of the range</param>
        /// <throws>IllegalArgumentException if an operator was supplied that does not take a double range value</throws>
        public SupportFilterSpecParamRange(
            ExprFilterSpecLookupable lookupable,
            FilterOperator filterOperator,
            FilterSpecParamFilterForEval min,
            FilterSpecParamFilterForEval max)
            : base(lookupable, filterOperator)
        {
            Min = min;
            Max = max;

            if (!filterOperator.IsRangeOperator() && !filterOperator.IsInvertedRangeOperator())
            {
                throw new ArgumentException(
                    "Illegal filter operator " +
                    filterOperator +
                    " supplied to " +
                    "range filter parameter");
            }
        }

        /// <summary>
        ///     Returns the lower endpoint.
        /// </summary>
        /// <returns>lower endpoint</returns>
        public FilterSpecParamFilterForEval Min { get; }

        /// <summary>
        ///     Returns the upper endpoint.
        /// </summary>
        /// <returns>upper endpoint</returns>
        public FilterSpecParamFilterForEval Max { get; }

        public override FilterValueSetParam GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            ExprFilterSpecLookupable lookupable = this.Lkupable.Make(matchedEvents, exprEvaluatorContext);
            Object range;
            
            if (lookupable.ReturnType == typeof(string)) {
                var begin = (string) Min.GetFilterValue(matchedEvents, exprEvaluatorContext, filterEvalEnv);
                var end = (string) Max.GetFilterValue(matchedEvents, exprEvaluatorContext, filterEvalEnv);
                range = new StringRange(begin, end);
            }
            else {
                var begin = (double) Min.GetFilterValue(matchedEvents, exprEvaluatorContext, filterEvalEnv);
                var end = (double) Max.GetFilterValue(matchedEvents, exprEvaluatorContext, filterEvalEnv);
                range = new DoubleRange(begin, end);
            }

            return new FilterValueSetParamImpl(lookupable, FilterOperator, range);
        }

        public override string ToString()
        {
            return base.ToString() + "  range=(min=" + Min + ",max=" + Max + ')';
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is SupportFilterSpecParamRange))
            {
                return false;
            }

            var other = (SupportFilterSpecParamRange) obj;
            if (!base.Equals(other))
            {
                return false;
            }

            return Min.Equals(other.Min) &&
                   Max.Equals(other.Max);
        }

        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result = 31 * result + (Min != null ? Min.GetHashCode() : 0);
            result = 31 * result + (Max != null ? Max.GetHashCode() : 0);
            return result;
        }
    }
} // end of namespace
