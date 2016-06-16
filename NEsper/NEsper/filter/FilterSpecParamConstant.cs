///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class represents a single, constant value filter parameter in an <seealso cref="FilterSpecCompiled"/> filter specification.
    /// </summary>
    public sealed class FilterSpecParamConstant : FilterSpecParam
    {
        private readonly Object _filterConstant;

        /// <summary>Constructor. </summary>
        /// <param name="lookupable">is the lookupable</param>
        /// <param name="filterOperator">is the type of compare</param>
        /// <param name="filterConstant">contains the value to match against the event's property value</param>
        /// <throws>ArgumentException if an operator was supplied that does not take a single constant value</throws>
        public FilterSpecParamConstant(FilterSpecLookupable lookupable, FilterOperator filterOperator, Object filterConstant)
            : base(lookupable, filterOperator)
        {
            _filterConstant = filterConstant;

            if (filterOperator.IsRangeOperator())
            {
                throw new ArgumentException("Illegal filter operator " + filterOperator + " supplied to " +
                        "constant filter parameter");
            }
        }

        /// <summary>
        /// Return the filter parameter constant to filter for.
        /// </summary>
        /// <param name="matchedEvents">is the prior results that can be used to determine filter parameters</param>
        /// <param name="agentInstanceContext"></param>
        /// <returns>filter parameter constant's value</returns>
        public override object GetFilterValue(MatchedEventMap matchedEvents, AgentInstanceContext agentInstanceContext)
        {
            return _filterConstant;
        }

        /// <summary>Returns the constant value. </summary>
        /// <value>constant value</value>
        public object FilterConstant
        {
            get { return _filterConstant; }
        }

        public override String ToString()
        {
            return base.ToString() + " filterConstant=" + _filterConstant;
        }

        public bool Equals(FilterSpecParamConstant other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(other._filterConstant, _filterConstant);
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
            return Equals(obj as FilterSpecParamConstant);
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
                return (base.GetHashCode()*397) ^ (_filterConstant != null ? _filterConstant.GetHashCode() : 0);
            }
        }
    }
}
