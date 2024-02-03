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
    public class SupportFilterSpecParamConstant : FilterSpecParam
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="lookupable">is the lookupable</param>
        /// <param name="filterOperator">is the type of compare</param>
        /// <param name="filterConstant">contains the value to match against the event's property value</param>
        /// <throws>IllegalArgumentException if an operator was supplied that does not take a single constant value</throws>
        public SupportFilterSpecParamConstant(
            ExprFilterSpecLookupable lookupable,
            FilterOperator filterOperator,
            object filterConstant)
            : base(lookupable, filterOperator)
        {
            FilterConstant = filterConstant;

            if (filterOperator.IsRangeOperator())
            {
                throw new ArgumentException(
                    "Illegal filter operator " +
                    filterOperator +
                    " supplied to " +
                    "constant filter parameter");
            }
        }

        /// <summary>
        ///     Returns the constant value.
        /// </summary>
        /// <returns>constant value</returns>
        public object FilterConstant { get; }

        public override FilterValueSetParam GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            ExprFilterSpecLookupable lookupable = this.Lkupable.Make(matchedEvents, exprEvaluatorContext);
            return new FilterValueSetParamImpl(lookupable, FilterOperator, FilterConstant);
        }

        public override string ToString()
        {
            return base.ToString() + " filterConstant=" + FilterConstant;
        }

        protected bool Equals(SupportFilterSpecParamConstant other)
        {
            return Equals(FilterConstant, other.FilterConstant);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((SupportFilterSpecParamConstant) obj);
        }

        public override int GetHashCode()
        {
            return (FilterConstant != null ? FilterConstant.GetHashCode() : 0);
        }
    }
} // end of namespace
