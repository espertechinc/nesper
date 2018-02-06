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
    /// <summary>
    /// A double?-typed value as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalConstantDouble : FilterSpecParamFilterForEvalDouble
    {
        private readonly double _doubleValue;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="doubleValue">is the value of the range endpoint</param>
        public FilterForEvalConstantDouble(double doubleValue)
        {
            _doubleValue = doubleValue;
        }

        public double GetFilterValueDouble(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _doubleValue;
        }

        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _doubleValue;
        }

        /// <summary>
        /// Returns the constant value.
        /// </summary>
        /// <returns>constant</returns>
        public double DoubleValue => _doubleValue;

        public override String ToString()
        {
            return _doubleValue.ToString();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            return (obj is FilterForEvalConstantDouble other) && (other._doubleValue == this._doubleValue);
        }

        public override int GetHashCode()
        {
            return _doubleValue.GetHashCode();
        }
    }
} // end of namespace