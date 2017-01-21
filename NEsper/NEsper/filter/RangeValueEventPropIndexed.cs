///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// An event property as a filter parameter representing a range.
    /// </summary>
    public class RangeValueEventPropIndexed : FilterSpecParamRangeValue
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _resultEventAsName;
        private readonly int _resultEventIndex;
        private readonly String _resultEventProperty;
        private readonly String _statementName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="resultEventAsName">is the event tag</param>
        /// <param name="resultEventIndex">index for event</param>
        /// <param name="resultEventProperty">is the event property name</param>
        /// <param name="statementName">Name of the statement.</param>
        public RangeValueEventPropIndexed(String resultEventAsName, int resultEventIndex, String resultEventProperty, String statementName)
        {
            _resultEventAsName = resultEventAsName;
            _resultEventIndex = resultEventIndex;
            _resultEventProperty = resultEventProperty;
            _statementName = statementName;
        }

        /// <summary>Returns the index. </summary>
        /// <value>index</value>
        public int ResultEventIndex
        {
            get { return _resultEventIndex; }
        }

        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean[] events = (EventBean[]) matchedEvents.GetMatchingEventAsObjectByTag(_resultEventAsName);
    
            if (events == null)
            {
                Log.Warn("Matching events for tag '" + _resultEventAsName + "' returned a null result, using null value in filter criteria, for statement '" + _statementName + "'");
                return null;
            }
            if (_resultEventIndex > (events.Length - 1))
            {
                Log.Warn("Matching events for tag '" + _resultEventAsName + "' returned no result for index " + _resultEventIndex + " at array length " + events.Length + ", using null value in filter criteria, for statement '" + _statementName + "'");
                return null;
            }
            
            var value = events[_resultEventIndex].Get(_resultEventProperty);
            if (value == null)
            {
                return null;
            }
            return value.AsDouble();
        }

        /// <summary>Returns the tag name or stream name to use for the event property. </summary>
        /// <value>tag name</value>
        public string ResultEventAsName
        {
            get { return _resultEventAsName; }
        }

        /// <summary>Returns the name of the event property. </summary>
        /// <value>event property name</value>
        public string ResultEventProperty
        {
            get { return _resultEventProperty; }
        }

        public override String ToString()
        {
            return "resultEventProp=" + _resultEventAsName + '.' + _resultEventProperty;
        }
    
        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }
    
            if (!(obj is RangeValueEventPropIndexed))
            {
                return false;
            }
    
            RangeValueEventPropIndexed other = (RangeValueEventPropIndexed) obj;
            if ( (other._resultEventAsName.Equals(this._resultEventAsName)) &&
                 (other._resultEventProperty.Equals(this._resultEventProperty) &&
                 (other._resultEventIndex == _resultEventIndex)))
            {
                return true;
            }
    
            return false;
        }
    
        public override int GetHashCode()
        {
            return _resultEventProperty.GetHashCode();
        }
    }
}
