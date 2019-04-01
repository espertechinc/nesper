///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// An event property as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalEventPropDouble : FilterSpecParamFilterForEvalDouble
    {
        private readonly string _resultEventAsName;
        private readonly string _resultEventProperty;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="resultEventAsName">is the event tag</param>
        /// <param name="resultEventProperty">is the event property name</param>
        public FilterForEvalEventPropDouble(string resultEventAsName, string resultEventProperty)
        {
            _resultEventAsName = resultEventAsName;
            _resultEventProperty = resultEventProperty;
        }

        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = matchedEvents.GetMatchingEventByTag(_resultEventAsName);
            if (theEvent == null)
            {
                throw new IllegalStateException("Matching event named '" + _resultEventAsName + "' not found in event result set");
            }

            var value = theEvent.Get(_resultEventProperty);
            if (value == null)
            {
                return null;
            }
            return value.AsDouble();
        }

        public double GetFilterValueDouble(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext).AsDouble();
        }

        /// <summary>
        /// Returns the tag name or stream name to use for the event property.
        /// </summary>
        /// <value>tag name</value>
        public string ResultEventAsName => _resultEventAsName;

        /// <summary>
        /// Returns the name of the event property.
        /// </summary>
        /// <returns>event property name</returns>
        public string ResultEventProperty => _resultEventProperty;

        public override String ToString()
        {
            return "resultEventProp=" + _resultEventAsName + '.' + _resultEventProperty;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj is FilterForEvalEventPropDouble other)
            {
                if ((other._resultEventAsName.Equals(_resultEventAsName)) &&
                    (other._resultEventProperty.Equals(_resultEventProperty)))
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _resultEventProperty.GetHashCode();
        }
    }
} // end of namespace