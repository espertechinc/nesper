///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class represents a filter parameter containing a reference to another event's property
    /// in the event pattern result, for use to describe a filter parameter in a <seealso cref="FilterSpecCompiled" /> filter specification.
    /// </summary>
    public sealed class FilterSpecParamEventProp : FilterSpecParam
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _resultEventAsName;
        private readonly string _resultEventProperty;
        private readonly bool _isMustCoerce;
        [NonSerialized] 
        private readonly Coercer _numberCoercer;
        private readonly Type _coercionType;
        private readonly string _statementName;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lookupable">is the property or function to get a lookup value</param>
        /// <param name="filterOperator">is the type of compare</param>
        /// <param name="resultEventAsName">is the name of the result event from which to get a property value to compare</param>
        /// <param name="resultEventProperty">is the name of the property to get from the named result event</param>
        /// <param name="isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name="coercionType">indicates the numeric coercion type to use</param>
        /// <param name="numberCoercer">interface to use to perform coercion</param>
        /// <param name="statementName">statement name</param>
        /// <exception cref="ArgumentException">if an operator was supplied that does not take a single constant value</exception>
        public FilterSpecParamEventProp(FilterSpecLookupable lookupable, FilterOperator filterOperator, string resultEventAsName,
                                        string resultEventProperty, bool isMustCoerce,
                                        Coercer numberCoercer, Type coercionType,
                                        string statementName)
            : base(lookupable, filterOperator)
        {
            _resultEventAsName = resultEventAsName;
            _resultEventProperty = resultEventProperty;
            _isMustCoerce = isMustCoerce;
            _numberCoercer = numberCoercer;
            _coercionType = coercionType;
            _statementName = statementName;
    
            if (filterOperator.IsRangeOperator()) {
                throw new ArgumentException("Illegal filter operator " + filterOperator + " supplied to " +
                        "event property filter parameter");
            }
        }

        /// <summary>
        /// Returns true if numeric coercion is required, or false if not
        /// </summary>
        /// <value>true to coerce at runtime</value>
        public bool IsMustCoerce
        {
            get { return _isMustCoerce; }
        }

        /// <summary>
        /// Returns the numeric coercion type.
        /// </summary>
        /// <value>type to coerce to</value>
        public Type CoercionType
        {
            get { return _coercionType; }
        }

        /// <summary>
        /// Returns tag for result event.
        /// </summary>
        /// <value>tag</value>
        public string ResultEventAsName
        {
            get { return _resultEventAsName; }
        }

        /// <summary>
        /// Returns the property of the result event.
        /// </summary>
        /// <value>property name</value>
        public string ResultEventProperty
        {
            get { return _resultEventProperty; }
        }

        public override Object GetFilterValue(MatchedEventMap matchedEvents, AgentInstanceContext agentInstanceContext)
        {
            EventBean theEvent = matchedEvents.GetMatchingEventByTag(_resultEventAsName);
            Object value = null;
            if (theEvent == null) {
                Log.Warn("Matching events for tag '{0}' returned a null result, using null value in filter criteria, for statement '{1}'", _resultEventAsName, _statementName);
            } else {
                value = theEvent.Get(_resultEventProperty);
            }
    
            // Coerce if necessary
            if (_isMustCoerce) {
                value = _numberCoercer.Invoke(value);
            }
            return value;
        }
    
        public override String ToString()
        {
            return base.ToString() +
                    " resultEventAsName=" + _resultEventAsName +
                    " resultEventProperty=" + _resultEventProperty;
        }
    
        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }
    
            if (!(obj is FilterSpecParamEventProp)) {
                return false;
            }
    
            var other = (FilterSpecParamEventProp) obj;
            if (!base.Equals(other)) {
                return false;
            }

            if ((!_resultEventAsName.Equals(other._resultEventAsName)) ||
                (!_resultEventProperty.Equals(other._resultEventProperty)))
            {
                return false;
            }
            return true;
        }
    
        public override int GetHashCode()
        {
            return 31 * base.GetHashCode() + _resultEventProperty.GetHashCode();
        }
    }
} // end of namespace
