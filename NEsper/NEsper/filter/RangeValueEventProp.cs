///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// An event property as a filter parameter representing a range.
    /// </summary>
    public class RangeValueEventProp : FilterSpecParamRangeValue
    {
        private readonly String _resultEventAsName;
        private readonly String _resultEventProperty;

        /// <summary>Ctor. </summary>
        /// <param name="resultEventAsName">is the event tag</param>
        /// <param name="resultEventProperty">is the event property name</param>
        public RangeValueEventProp(String resultEventAsName, String resultEventProperty)
        {
            _resultEventAsName = resultEventAsName;
            _resultEventProperty = resultEventProperty;
        }

        public Object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = matchedEvents.GetMatchingEventByTag(_resultEventAsName);
            if (theEvent == null)
            {
                throw new IllegalStateException(
                    "Matching event named " + 
                    '\'' + _resultEventAsName + "' not found in event result set");
            }

            var value = theEvent.Get(_resultEventProperty);
            if (value == null)
            {
                return null;
            }
            return value.AsDouble();
        }

        /// <summary>
        /// Returns the tag name or stream name to use for the event property.
        /// </summary>
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
    
            if (!(obj is RangeValueEventProp))
            {
                return false;
            }
    
            var other = (RangeValueEventProp) obj;
            if ( (other._resultEventAsName.Equals(_resultEventAsName)) &&
                 (other._resultEventProperty.Equals(_resultEventProperty)))
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
