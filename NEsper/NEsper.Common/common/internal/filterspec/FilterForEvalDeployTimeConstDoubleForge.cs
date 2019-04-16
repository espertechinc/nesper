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
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterForEvalDeployTimeConstDoubleForge : FilterSpecParamFilterForEvalDoubleForge
    {
        private readonly ExprNodeDeployTimeConst _deployTimeConst;

        public FilterForEvalDeployTimeConstDoubleForge(ExprNodeDeployTimeConst deployTimeConst)
        {
            this._deployTimeConst = deployTimeConst;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            CodegenMethod method = parent.MakeChild(typeof(object), this.GetType(), classScope).AddParam(GET_FILTER_VALUE_FP);
            method.Block
                .MethodReturn(ExprDotMethod(Cast(typeof(object), _deployTimeConst.CodegenGetDeployTimeConstValue(classScope)), "doubleValue"));
            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        object FilterSpecParamFilterForEvalForge.GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext);
        }

        public Double GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public Double GetFilterValueDouble(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            FilterForEvalDeployTimeConstDoubleForge that = (FilterForEvalDeployTimeConstDoubleForge) o;

            return _deployTimeConst.Equals(that._deployTimeConst);
        }

        public override int GetHashCode()
        {
            return _deployTimeConst.GetHashCode();
        }
    }
} // end of namespace