///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;


namespace com.espertech.esper.filter
{
    /// <summary>
    /// A Double-typed value as a filter parameter representing a range.
    /// </summary>
    public class RangeValueDouble : FilterSpecParamRangeValue
    {
        private readonly double _doubleValue;

        /// <summary>Ctor. </summary>
        /// <param name="doubleValue">is the value of the range endpoint</param>
        public RangeValueDouble(double doubleValue)
        {
            _doubleValue = doubleValue;
        }
    
        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _doubleValue;
        }

        /// <summary>Returns the constant value. </summary>
        /// <value>constant</value>
        public double DoubleValue
        {
            get { return _doubleValue; }
        }

        public override String ToString()
        {
            return _doubleValue.ToString();
        }
    
        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }
    
            if (!(obj is RangeValueDouble))
            {
                return false;
            }
    
            RangeValueDouble other = (RangeValueDouble) obj;
            return other._doubleValue == this._doubleValue;
        }
    
        public override int GetHashCode()
        {
            long temp = _doubleValue != +0.0d ? BitConverter.DoubleToInt64Bits(_doubleValue) : 0L;
            return (int) (temp ^ (temp >> 32));
        }
    }
}
