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
    public class FilterForEvalEventPropMayCoerce : FilterSpecParamInValue
    {
        private readonly string _resultEventAsName;
        private readonly string _resultEventProperty;
        private readonly bool _isMustCoerce;
        private readonly Type _coercionType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="resultEventAsName">is the event tag</param>
        /// <param name="resultEventProperty">is the event property name</param>
        /// <param name="isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name="coercionType">indicates the numeric coercion type to use</param>
        public FilterForEvalEventPropMayCoerce(string resultEventAsName, string resultEventProperty, bool isMustCoerce, Type coercionType)
        {
            this._resultEventAsName = resultEventAsName;
            this._resultEventProperty = resultEventProperty;
            this._coercionType = coercionType;
            this._isMustCoerce = isMustCoerce;
        }

        public Type ReturnType => _coercionType;

        public bool IsConstant => false;

        public Object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext evaluatorContext)
        {
            EventBean theEvent = matchedEvents.GetMatchingEventByTag(_resultEventAsName);
            if (theEvent == null)
            {
                throw new IllegalStateException("Matching event named " +
                        '\'' + _resultEventAsName + "' not found in event result set");
            }

            Object value = theEvent.Get(_resultEventProperty);

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

            if (!(obj is FilterForEvalEventPropMayCoerce))
            {
                return false;
            }

            FilterForEvalEventPropMayCoerce other = (FilterForEvalEventPropMayCoerce)obj;
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