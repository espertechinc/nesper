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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Event property value in a list of values following an in-keyword.
    /// </summary>
    public class InSetOfValuesEventProp : FilterSpecParamInValue
    {
        private readonly bool _isMustCoerce;
        private readonly Type _coercionType;

        /// <summary>Ctor. </summary>
        /// <param name="resultEventAsName">is the event tag</param>
        /// <param name="resultEventProperty">is the event property name</param>
        /// <param name="isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name="coercionType">indicates the numeric coercion type to use</param>
        public InSetOfValuesEventProp(String resultEventAsName, String resultEventProperty, bool isMustCoerce, Type coercionType)
        {
            ResultEventAsName = resultEventAsName;
            ResultEventProperty = resultEventProperty;
            _coercionType = coercionType;
            _isMustCoerce = isMustCoerce;
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
            EventBean theEvent = matchedEvents.GetMatchingEventByTag(ResultEventAsName);
            if (theEvent == null)
            {
                throw new IllegalStateException("Matching event named " +
                        '\'' + ResultEventAsName + "' not found in event result set");
            }

            Object value = theEvent.Get(ResultEventProperty);

            // Coerce if necessary
            if (_isMustCoerce)
            {
                if (value != null)
                {
                    value = CoercerFactory.CoerceBoxed(value, _coercionType);
                }
            }
            return value;
        }

        /// <summary>Returns the tag used for the event property. </summary>
        /// <value>tag</value>
        public string ResultEventAsName { get; private set; }

        /// <summary>Returns the event property name. </summary>
        /// <value>property name</value>
        public string ResultEventProperty { get; private set; }

        public override String ToString()
        {
            return "resultEventProp=" + ResultEventAsName + '.' + ResultEventProperty;
        }

        public bool Equals(InSetOfValuesEventProp other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ResultEventAsName, ResultEventAsName) && Equals(other.ResultEventProperty, ResultEventProperty);
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
            if (obj.GetType() != typeof(InSetOfValuesEventProp)) return false;
            return Equals((InSetOfValuesEventProp)obj);
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
                return ((ResultEventAsName != null ? ResultEventAsName.GetHashCode() : 0) * 397) ^ (ResultEventProperty != null ? ResultEventProperty.GetHashCode() : 0);
            }
        }
    }
}
