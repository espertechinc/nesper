///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Event property value in a list of values following an in-keyword.
    /// </summary>
    public class FilterForEvalEventPropIndexedMayCoerce : FilterSpecParamInValue
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _resultEventAsName;
        private readonly int _resultEventIndex;
        private readonly string _resultEventProperty;
        private readonly bool _isMustCoerce;
        private readonly Type _coercionType;
        private readonly string _statementName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="resultEventAsName">is the event tag</param>
        /// <param name="resultEventProperty">is the event property name</param>
        /// <param name="isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name="coercionType">indicates the numeric coercion type to use</param>
        /// <param name="resultEventindex">index</param>
        /// <param name="statementName">statement name</param>
        public FilterForEvalEventPropIndexedMayCoerce(string resultEventAsName, int resultEventindex, string resultEventProperty, bool isMustCoerce, Type coercionType, string statementName)
        {
            this._resultEventAsName = resultEventAsName;
            this._resultEventProperty = resultEventProperty;
            this._resultEventIndex = resultEventindex;
            this._coercionType = coercionType;
            this._isMustCoerce = isMustCoerce;
            this._statementName = statementName;
        }

        public Type ReturnType => _coercionType;

        public bool IsConstant => false;

        public Object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext evaluatorContext)
        {
            EventBean[] events = (EventBean[])matchedEvents.GetMatchingEventAsObjectByTag(_resultEventAsName);

            Object value = null;
            if (events == null)
            {
                Log.Warn("Matching events for tag '" + _resultEventAsName + "' returned a null result, using null value in filter criteria, for statement '" + _statementName + "'");
            }
            else if (_resultEventIndex > (events.Length - 1))
            {
                Log.Warn("Matching events for tag '" + _resultEventAsName + "' returned no result for index " + _resultEventIndex + " at array length " + events.Length + ", using null value in filter criteria, for statement '" + _statementName + "'");
            }
            else
            {
                value = events[_resultEventIndex].Get(_resultEventProperty);
            }

            // Coerce if necessary
            if (_isMustCoerce)
            {
                value = CoercerFactory.CoerceBoxed(value, _coercionType);
            }
            return value;
        }

        /// <summary>
        /// Returns the tag used for the event property.
        /// </summary>
        /// <returns>tag</returns>
        public string ResultEventAsName => _resultEventAsName;

        /// <summary>
        /// Returns the event property name.
        /// </summary>
        /// <returns>property name</returns>
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

            if (!(obj is FilterForEvalEventPropIndexedMayCoerce))
            {
                return false;
            }

            FilterForEvalEventPropIndexedMayCoerce other = (FilterForEvalEventPropIndexedMayCoerce)obj;
            if ((other._resultEventAsName.Equals(this._resultEventAsName)) &&
                (other._resultEventProperty.Equals(this._resultEventProperty)))
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