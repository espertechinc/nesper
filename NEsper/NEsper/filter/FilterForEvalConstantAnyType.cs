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
    /// <summary>Constant value in a list of values following an in-keyword.</summary>
    public class FilterForEvalConstantAnyType : FilterSpecParamInValue
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="constant">is the constant value</param>
        public FilterForEvalConstantAnyType(object constant)
        {
            Constant = constant;
        }

        /// <summary>
        ///     Returns the constant value.
        /// </summary>
        /// <value>constant</value>
        public object Constant { get; }

        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext evaluatorContext)
        {
            return Constant;
        }

        public Type ReturnType => Constant?.GetType();

        public bool IsConstant => true;

        protected bool Equals(FilterForEvalConstantAnyType other)
        {
            return Constant?.Equals(other.Constant) ?? other.Constant == null;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FilterForEvalConstantAnyType) obj);
        }

        public override int GetHashCode()
        {
            return Constant != null ? Constant.GetHashCode() : 0;
        }
    }
} // end of namespace