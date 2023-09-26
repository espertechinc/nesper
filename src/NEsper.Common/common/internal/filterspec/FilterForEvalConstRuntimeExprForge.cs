///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     A Double-typed value as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalConstRuntimeExprForge : FilterSpecParamFilterForEvalDoubleForge
    {
        private readonly ExprNode _runtimeConstant;

        public FilterForEvalConstRuntimeExprForge(ExprNode runtimeConstant)
        {
            _runtimeConstant = runtimeConstant;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(double), GetType(), classScope);
            var result = CodegenLegoMethodExpression.CodegenExpression(
                _runtimeConstant.Forge,
                method,
                classScope);
            method.Block.MethodReturn(
                ExprDotMethod(
                    LocalMethod(result, ConstantNull(), ConstantTrue(), ConstantNull()),
                    "AsDouble"));
            return LocalMethod(method);
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext evaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterForEvalConstRuntimeExprForge other)) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEquals(other._runtimeConstant, _runtimeConstant, true);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public void ValueToString(StringBuilder @out)
        {
            @out.Append("runtime constant expression '")
                .Append(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_runtimeConstant))
                .Append("'");
        }
    }
} // end of namespace