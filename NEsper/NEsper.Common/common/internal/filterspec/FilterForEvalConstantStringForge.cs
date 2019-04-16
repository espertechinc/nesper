///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    /// A String-typed value as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalConstantStringForge : FilterSpecParamFilterForEvalForge
    {
        private readonly string theStringValue;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="theStringValue">is the value of the range endpoint</param>
        public FilterForEvalConstantStringForge(string theStringValue)
        {
            this.theStringValue = theStringValue;
        }

        object FilterSpecParamFilterForEvalForge.GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext);
        }

        public string GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return theStringValue;
        }

        public override string ToString()
        {
            return theStringValue;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            return Constant(theStringValue);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            FilterForEvalConstantStringForge that = (FilterForEvalConstantStringForge) o;

            if (theStringValue != null ? !theStringValue.Equals(that.theStringValue) : that.theStringValue != null)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return theStringValue != null ? theStringValue.GetHashCode() : 0;
        }
    }
} // end of namespace