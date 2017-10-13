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
    /// <summary>Event property value in a list of values following an in-keyword. </summary>
    public class InSetOfValuesEventPropIndexed : FilterSpecParamInValue
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _resultEventAsName;
        private readonly int _resultEventIndex;
        private readonly String _resultEventProperty;
        private readonly bool _isMustCoerce;
        private readonly Type _coercionType;
        private readonly String _statementName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="resultEventAsName">is the event tag</param>
        /// <param name="resultEventindex">index</param>
        /// <param name="resultEventProperty">is the event property name</param>
        /// <param name="isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name="coercionType">indicates the numeric coercion type to use</param>
        /// <param name="statementName">Name of the statement.</param>
        public InSetOfValuesEventPropIndexed(
            String resultEventAsName,
            int resultEventindex,
            String resultEventProperty,
            bool isMustCoerce,
            Type coercionType,
            String statementName)
        {
            _resultEventAsName = resultEventAsName;
            _resultEventProperty = resultEventProperty;
            _resultEventIndex = resultEventindex;
            _coercionType = coercionType;
            _isMustCoerce = isMustCoerce;
            _statementName = statementName;
        }

        public Type ReturnType
        {
            get { return _coercionType; }
        }

        public bool IsConstant
        {
            get { return false; }
        }

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

        /// <summary>Returns the tag used for the event property. </summary>
        /// <value>tag</value>
        public string ResultEventAsName
        {
            get { return _resultEventAsName; }
        }

        /// <summary>Returns the event property name. </summary>
        /// <value>property name</value>
        public string ResultEventProperty
        {
            get { return _resultEventProperty; }
        }

        public override String ToString()
        {
            return "resultEventProp=" + _resultEventAsName + '.' + _resultEventProperty;
        }

        public bool Equals(InSetOfValuesEventPropIndexed other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._resultEventAsName, _resultEventAsName) && Equals(other._resultEventProperty, _resultEventProperty);
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
            if (obj.GetType() != typeof(InSetOfValuesEventPropIndexed)) return false;
            return Equals((InSetOfValuesEventPropIndexed)obj);
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
                return ((_resultEventAsName != null ? _resultEventAsName.GetHashCode() : 0) * 397) ^ (_resultEventProperty != null ? _resultEventProperty.GetHashCode() : 0);
            }
        }
    }
}
