///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>Constant value in a list of values following an in-keyword. </summary>
    public class InSetOfValuesConstant : FilterSpecParamInValue
    {
        /// <summary>Ctor. </summary>
        /// <param name="constant">is the constant value</param>
        public InSetOfValuesConstant(Object constant)
        {
            Constant = constant;
        }

        public Object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext evaluatorContext)
        {
            return Constant;
        }

        public Type ReturnType
        {
            get { return Constant == null ? null : Constant.GetType(); }
        }

        public bool IsConstant
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the constant value.
        /// </summary>
        /// <value>
        /// constant
        /// </value>
        public object Constant { get; private set; }

        public bool Equals(InSetOfValuesConstant other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Constant, Constant);
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
            if (obj.GetType() != typeof(InSetOfValuesConstant)) return false;
            return Equals((InSetOfValuesConstant)obj);
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
            return (Constant != null ? Constant.GetHashCode() : 0);
        }
    }
}
