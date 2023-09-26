///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Constant value in a list of values following an in-keyword.
    /// </summary>
    public class FilterForEvalConstantAnyTypeForge : FilterSpecParamInValueForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="constant">is the constant value</param>
        public FilterForEvalConstantAnyTypeForge(object constant)
        {
            Constant = constant;
        }

        /// <summary>
        ///     Returns the constant value.
        /// </summary>
        /// <returns>constant</returns>
        public object Constant { get; }

        public Type ReturnType => Constant?.GetType();

        public bool IsConstant => true;

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext evaluatorContext)
        {
            return Constant;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            return Constant(Constant);
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (FilterForEvalConstantAnyTypeForge)o;

            if (!Constant?.Equals(that.Constant) ?? that.Constant != null) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Constant != null ? Constant.GetHashCode() : 0;
        }

        public void ValueToString(StringBuilder @out)
        {
            FilterSpecParamConstantForge.ValueExprToString(@out, Constant);
        }
    }
} // end of namespace