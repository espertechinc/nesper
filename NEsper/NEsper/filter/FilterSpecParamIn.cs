///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class represents a 'in' filter parameter in an <see cref="com.espertech.esper.filter.FilterSpecCompiled"/> filter specification.
    /// <para>
    /// The 'in' checks for a list of values.
    /// </para>
    /// </summary>
    public sealed class FilterSpecParamIn : FilterSpecParam
    {
        private readonly IList<FilterSpecParamInValue> _listOfValues;
        private readonly MultiKeyUntyped _inListConstantsOnly;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupable">is the event property name</param>
        /// <param name="filterOperator">is expected to be the IN-list operator</param>
        /// <param name="listofValues">is a list of constants and event property names</param>
        /// <throws>ArgumentException for illegal args</throws>
        public FilterSpecParamIn(FilterSpecLookupable lookupable, FilterOperator filterOperator, IList<FilterSpecParamInValue> listofValues)
            : base(lookupable, filterOperator)
        {
            _listOfValues = listofValues;

            bool isAllConstants = false;
            foreach (FilterSpecParamInValue value in listofValues)
            {
                if (value is InSetOfValuesEventProp || value is InSetOfValuesContextProp)
                {
                    isAllConstants = false;
                    break;
                }
            }

            if (isAllConstants)
            {
                Object[] constants = new Object[_listOfValues.Count];
                int count = 0;
                foreach (FilterSpecParamInValue valuePlaceholder in _listOfValues)
                {
                    constants[count++] = valuePlaceholder.GetFilterValue(null, null);
                }
                _inListConstantsOnly = new MultiKeyUntyped(constants);
            }

            if ((filterOperator != FilterOperator.IN_LIST_OF_VALUES) && ((filterOperator != FilterOperator.NOT_IN_LIST_OF_VALUES)))
            {
                throw new ArgumentException("Illegal filter operator " + filterOperator + " supplied to " +
                        "in-values filter parameter");
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
            // If the list of values consists of all-constants and no event properties, then use cached version
            if (_inListConstantsOnly != null)
            {
                return _inListConstantsOnly;
            }

            // Determine actual values since the in-list of values contains one or more event properties
            Object[] actualValues = new Object[_listOfValues.Count];
            int count = 0;
            foreach (FilterSpecParamInValue valuePlaceholder in _listOfValues)
            {
                actualValues[count++] = valuePlaceholder.GetFilterValue(matchedEvents, agentInstanceContext);
            }
            return new MultiKeyUntyped(actualValues);
        }

        /// <summary>
        /// Returns the list of values we are asking to match.
        /// </summary>
        /// <returns>list of filter values</returns>
        public IList<FilterSpecParamInValue> ListOfValues
        {
            get { return _listOfValues; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            return base.ToString() + "  in=(listOfValues=" + _listOfValues.ToString() + ')';
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is FilterSpecParamIn))
            {
                return false;
            }

            FilterSpecParamIn other = (FilterSpecParamIn)obj;
            if (!base.Equals(other))
            {
                return false;
            }

            if (_listOfValues.Count != other._listOfValues.Count)
            {
                return false;
            }

            if (!Collections.AreEqual(_listOfValues, other._listOfValues))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result = 31 * result + (_listOfValues != null ? _listOfValues.GetHashCode() : 0);
            return result;
        }
    }
} // End of namespace
