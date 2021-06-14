///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     A Double-typed value as a filter parameter representing a range.
    /// </summary>
    [Serializable]
    public class SupportFilterForEvalConstantDouble : FilterSpecParamFilterForEvalDouble
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="doubleValue">is the value of the range endpoint</param>
        public SupportFilterForEvalConstantDouble(double doubleValue)
        {
            DoubleValue = doubleValue;
        }

        /// <summary>
        ///     Returns the constant value.
        /// </summary>
        /// <value>constant</value>
        public double DoubleValue { get; }

        public double GetFilterValueDouble(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            return DoubleValue;
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            return DoubleValue;
        }

        public override string ToString()
        {
            return $"{nameof(DoubleValue)}: {DoubleValue}";
        }

        protected bool Equals(SupportFilterForEvalConstantDouble other)
        {
            return DoubleValue.Equals(other.DoubleValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((SupportFilterForEvalConstantDouble) obj);
        }

        public override int GetHashCode()
        {
            return DoubleValue.GetHashCode();
        }
    }
} // end of namespace