///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Event property value in a list of values following an in-keyword.
    /// </summary>
    [Serializable]
    public class InSetOfValuesContextProp : FilterSpecParamInValue
    {
        [NonSerialized]
        private readonly EventPropertyGetter _getter;
        [NonSerialized]
        private readonly Coercer _numberCoercer;
        private readonly string _propertyName;
        [NonSerialized]
        private readonly Type _returnType;

        public InSetOfValuesContextProp(String propertyName, EventPropertyGetter getter, Coercer coercer, Type returnType)
        {
            _propertyName = propertyName;
            _getter = getter;
            _numberCoercer = coercer;
            _returnType = returnType;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public bool IsConstant
        {
            get { return false; }
        }

        public Object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext evaluatorContext)
        {
            if (evaluatorContext.ContextProperties == null)
            {
                return null;
            }
            Object result = _getter.Get(evaluatorContext.ContextProperties);

            if (_numberCoercer == null)
            {
                return result;
            }
            return _numberCoercer.Invoke(result);
        }

        public bool Equals(InSetOfValuesContextProp other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._propertyName, _propertyName);
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
            if (obj.GetType() != typeof(InSetOfValuesContextProp)) return false;
            return Equals((InSetOfValuesContextProp)obj);
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
            return (_propertyName != null ? _propertyName.GetHashCode() : 0);
        }
    }
}
