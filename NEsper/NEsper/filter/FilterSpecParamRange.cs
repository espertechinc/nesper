///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class represents a range filter parameter in an <seealso cref="FilterSpecCompiled"/> 
    /// filter specification.
    /// </summary>
    public sealed class FilterSpecParamRange : FilterSpecParam
    {
        private readonly FilterSpecParamFilterForEval _min;
        private readonly FilterSpecParamFilterForEval _max;

        /// <summary>Constructor. </summary>
        /// <param name="lookupable">is the lookupable</param>
        /// <param name="filterOperator">is the type of range operator</param>
        /// <param name="min">is the begin point of the range</param>
        /// <param name="max">is the end point of the range</param>
        /// <throws>ArgumentException if an operator was supplied that does not take a double range value</throws>
        public FilterSpecParamRange(FilterSpecLookupable lookupable, FilterOperator filterOperator, FilterSpecParamFilterForEval min, FilterSpecParamFilterForEval max)
            : base(lookupable, filterOperator)
        {
            _min = min;
            _max = max;

            if (!(filterOperator.IsRangeOperator()) && (!filterOperator.IsInvertedRangeOperator()))
            {
                throw new ArgumentException("Illegal filter operator " + filterOperator + " supplied to " +
                        "range filter parameter");
            }
        }

        public override object GetFilterValue(MatchedEventMap matchedEvents, AgentInstanceContext agentInstanceContext)
        {
            if (Lookupable.ReturnType == typeof(String))
            {
                return new StringRange((String)_min.GetFilterValue(matchedEvents, agentInstanceContext), (String)_max.GetFilterValue(matchedEvents, agentInstanceContext));
            }

            var begin = _min.GetFilterValue(matchedEvents, agentInstanceContext).AsBoxedDouble();
            var end = _max.GetFilterValue(matchedEvents, agentInstanceContext).AsBoxedDouble();
            return new DoubleRange(begin, end);
        }

        /// <summary>Returns the lower endpoint. </summary>
        /// <value>lower endpoint</value>
        public FilterSpecParamFilterForEval Min => _min;

        /// <summary>Returns the upper endpoint. </summary>
        /// <value>upper endpoint</value>
        public FilterSpecParamFilterForEval Max => _max;

        public override String ToString()
        {
            return base.ToString() + "  range=(min=" + _min + ",max=" + _max + ')';
        }

        public bool Equals(FilterSpecParamRange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(other._min, _min) && Equals(other._max, _max);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as FilterSpecParamRange);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (_min != null ? _min.GetHashCode() : 0);
                result = (result * 397) ^ (_max != null ? _max.GetHashCode() : 0);
                return result;
            }
        }
    }
}
