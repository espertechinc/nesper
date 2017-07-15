///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.property;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Contains the filter criteria to sift through events. The filter criteria are the event 
    /// class to look for and a set of parameters (attribute names, operators and constant/range 
    /// values).
    /// </summary>
    public sealed class FilterSpecCompiled
    {
        private static readonly FilterSpecParamComparator COMPARATOR_PARAMETERS = new FilterSpecParamComparator();

        private readonly EventType _filterForEventType;
        private readonly String _filterForEventTypeName;
        private readonly FilterSpecParam[][] _parameters;
        private readonly PropertyEvaluator _optionalPropertyEvaluator;

        /// <summary>Constructor - validates parameter list against event type, throws exception if invalid property names or mismatcing filter operators are found. </summary>
        /// <param name="eventType">is the event type</param>
        /// <param name="filterParameters">is a list of filter parameters</param>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <param name="optionalPropertyEvaluator">optional if evaluating properties returned by filtered events</param>
        /// <throws>ArgumentException if validation invalid</throws>
        public FilterSpecCompiled(EventType eventType, String eventTypeName, IList<FilterSpecParam>[] filterParameters, PropertyEvaluator optionalPropertyEvaluator)
        {
            _filterForEventType = eventType;
            _filterForEventTypeName = eventTypeName;
            _parameters = SortRemoveDups(filterParameters);
            _optionalPropertyEvaluator = optionalPropertyEvaluator;
        }

        /// <summary>
        /// Returns type of event to filter for.
        /// </summary>
        /// <value>event type</value>
        public EventType FilterForEventType
        {
            get { return _filterForEventType; }
        }

        /// <summary>
        /// Returns list of filter parameters.
        /// </summary>
        /// <value>list of filter params</value>
        public FilterSpecParam[][] Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Returns the event type name.
        /// </summary>
        /// <value>event type name</value>
        public string FilterForEventTypeName
        {
            get { return _filterForEventTypeName; }
        }

        /// <summary>
        /// Return the evaluator for property value if any is attached, or none if none attached.
        /// </summary>
        /// <value>property evaluator</value>
        public PropertyEvaluator OptionalPropertyEvaluator
        {
            get { return _optionalPropertyEvaluator; }
        }

        /// <summary>
        /// Returns the result event type of the filter specification.
        /// </summary>
        /// <value>event type</value>
        public EventType ResultEventType
        {
            get
            {
                if (_optionalPropertyEvaluator != null)
                {
                    return _optionalPropertyEvaluator.FragmentEventType;
                }
                else
                {
                    return _filterForEventType;
                }
            }
        }

        /// <summary>
        /// Returns the values for the filter, using the supplied result events to ask filter parameters for the value to filter for.
        /// </summary>
        /// <param name="matchedEvents">contains the result events to use for determining filter values</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="addendum">The addendum.</param>
        /// <returns>
        /// filter values
        /// </returns>
        public FilterValueSet GetValueSet(
            MatchedEventMap matchedEvents,
            AgentInstanceContext agentInstanceContext,
            FilterValueSetParam[][] addendum)
        {
            var valueList = new FilterValueSetParam[Parameters.Length][];
            for (int i = 0; i < Parameters.Length; i++)
            {
                valueList[i] = new FilterValueSetParam[Parameters[i].Length];
                PopulateValueSet(valueList[i], matchedEvents, agentInstanceContext, Parameters[i]);
            }

            if (addendum != null)
            {
                valueList = ContextControllerAddendumUtil.MultiplyAddendum(addendum, valueList);
            } 
            
            return new FilterValueSetImpl(_filterForEventType, valueList);
        }

        /// <summary>
        /// Populates the value set.
        /// </summary>
        /// <param name="valueList">The value list.</param>
        /// <param name="matchedEvents">The matched events.</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="specParams">The spec parameters.</param>
        private static void PopulateValueSet(
            FilterValueSetParam[] valueList,
            MatchedEventMap matchedEvents,
            AgentInstanceContext agentInstanceContext,
            FilterSpecParam[] specParams)
        {
            // Ask each filter specification parameter for the actual value to filter for
            var count = 0;
            foreach (var specParam in specParams)
            {
                var filterForValue = specParam.GetFilterValue(matchedEvents, agentInstanceContext);

                FilterValueSetParam valueParam = new FilterValueSetParamImpl(specParam.Lookupable, specParam.FilterOperator, filterForValue);
                valueList[count] = valueParam;
                count++;
            }
        }

        public override String ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("FilterSpecCompiled type=" + _filterForEventType);
            buffer.Append(" parameters=" + _parameters.Render());
            return buffer.ToString();
        }

        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is FilterSpecCompiled))
            {
                return false;
            }

            var other = (FilterSpecCompiled) obj;
            if (!EqualsTypeAndFilter(other))
            {
                return false;
            }

            if ((_optionalPropertyEvaluator == null) && (other._optionalPropertyEvaluator == null))
            {
                return true;
            }       
            if ((_optionalPropertyEvaluator != null) && (other._optionalPropertyEvaluator == null))
            {
                return false;
            }
            if ((_optionalPropertyEvaluator == null) && (other._optionalPropertyEvaluator != null))
            {
                return false;
            }

            return _optionalPropertyEvaluator.CompareTo(other._optionalPropertyEvaluator);
        }

        /// <summary>
        /// Compares only the type and filter portion and not the property evaluation portion.
        /// </summary>
        /// <param name="other">filter to compare</param>
        /// <returns>
        /// true if same
        /// </returns>
        public bool EqualsTypeAndFilter(FilterSpecCompiled other)
        {
            if (_filterForEventType != other._filterForEventType)
            {
                return false;
            }

            if (_parameters.Length != other._parameters.Length)
            {
                return false;
            }

            for (var i = 0; i < _parameters.Length; i++)
            {
                FilterSpecParam[] lineThis = this.Parameters[i];
                FilterSpecParam[] lineOther = other.Parameters[i];
                if (linethis.Length != lineOther.Length)
                {
                    return false;
                }

                for (int j = 0; j < linethis.Length; j++)
                {
                    if (!Equals(lineThis[j], lineOther[j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = FilterForEventType.GetHashCode();

            foreach (FilterSpecParam[] paramLine in Parameters) {
                foreach (FilterSpecParam param in paramLine) {
                    hashCode ^= 31 * param.GetHashCode();
                }
            }

            return hashCode;
        }

        public int GetFilterSpecIndexAmongAll(FilterSpecCompiled[] filterSpecAll)
        {
            for (int i = 0; i < filterSpecAll.Length; i++)
            {
                if (ReferenceEquals(this, filterSpecAll[i]))
                {
                    return i;
                }
            }
            throw new EPException("Failed to find find filter spec among list of known filters");
        }

        public static FilterSpecParam[][] SortRemoveDups(IList<FilterSpecParam>[] parameters)
        {
            var processed = new FilterSpecParam[parameters.Length][];
            for (int i = 0; i < parameters.Length; i++)
            {
                processed[i] = SortRemoveDups(parameters[i]);
            }
            return processed;
        }

        internal static FilterSpecParam[] SortRemoveDups(IList<FilterSpecParam> parameters)
        {
            if (parameters.IsEmpty()) {
                return FilterSpecParam.EMPTY_PARAM_ARRAY;
            }

            if (parameters.Count == 1) {
                return new FilterSpecParam[] {parameters[0]};
            }

            var result = new ArrayDeque<FilterSpecParam>();
            var map = new SortedDictionary<FilterOperator, List<FilterSpecParam>>(COMPARATOR_PARAMETERS);
            foreach (var parameter in parameters) {

                var list = map.Get(parameter.FilterOperator);
                if (list == null) {
                    list = new List<FilterSpecParam>();
                    map.Put(parameter.FilterOperator, list);
                }

                var hasDuplicate = list.Any(existing => existing.Lookupable.Equals(parameter.Lookupable));
                if (hasDuplicate) {
                    continue;
                }

                list.Add(parameter);
            }

            foreach (var entry in map) {
                result.AddAll(entry.Value);
            }
            return FilterSpecParam.ToArray(result);
        }
    }
}
