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
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// An event property as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalEventPropIndexedDouble : FilterSpecParamFilterForEvalDouble
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _resultEventAsName;
        private readonly int _resultEventIndex;
        private readonly string _resultEventProperty;
        private readonly string _statementName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="resultEventAsName">is the event tag</param>
        /// <param name="resultEventProperty">is the event property name</param>
        /// <param name="resultEventIndex">index for event</param>
        /// <param name="statementName">statement name</param>
        public FilterForEvalEventPropIndexedDouble(string resultEventAsName, int resultEventIndex, string resultEventProperty, string statementName)
        {
            this._resultEventAsName = resultEventAsName;
            this._resultEventIndex = resultEventIndex;
            this._resultEventProperty = resultEventProperty;
            this._statementName = statementName;
        }

        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            var events = (EventBean[])matchedEvents.GetMatchingEventAsObjectByTag(_resultEventAsName);

            object value;
            if (events == null)
            {
                Log.Warn("Matching events for tag '" + _resultEventAsName + "' returned a null result, using null value in filter criteria, for statement '" + _statementName + "'");
                return null;
            }
            else if (_resultEventIndex > (events.Length - 1))
            {
                Log.Warn("Matching events for tag '" + _resultEventAsName + "' returned no result for index " + _resultEventIndex + " at array length " + events.Length + ", using null value in filter criteria, for statement '" + _statementName + "'");
                return null;
            }
            else
            {
                value = events[_resultEventIndex].Get(_resultEventProperty);
            }

            return value;
        }

        public double GetFilterValueDouble(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext).AsDouble();
        }

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

            if (!(obj is FilterForEvalEventPropIndexedDouble))
            {
                return false;
            }

            var other = (FilterForEvalEventPropIndexedDouble)obj;
            if (other._resultEventAsName.Equals(this._resultEventAsName) && 
                other._resultEventProperty.Equals(this._resultEventProperty) && 
                (other._resultEventIndex == _resultEventIndex))
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
} // end of namespace