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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    /// Event property value in a list of values following an in-keyword.
    /// </summary>
    public class FilterForEvalDeployTimeConstForge : FilterSpecParamInValueForge
    {
        [NonSerialized] private readonly ExprNodeDeployTimeConst deployTimeConst;
        [NonSerialized] private readonly SimpleNumberCoercer numberCoercer;
        [NonSerialized] private readonly Type returnType;

        public FilterForEvalDeployTimeConstForge(
            ExprNodeDeployTimeConst deployTimeConst,
            SimpleNumberCoercer numberCoercer,
            Type returnType)
        {
            this.deployTimeConst = deployTimeConst;
            this.numberCoercer = numberCoercer;
            this.returnType = returnType;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            CodegenMethod method = parent.MakeChild(typeof(object), this.GetType(), classScope).AddParam(GET_FILTER_VALUE_FP);

            CodegenExpression value = deployTimeConst.CodegenGetDeployTimeConstValue(classScope);
            if (numberCoercer != null) {
                value = numberCoercer.CoerceCodegenMayNullBoxed(value, returnType, method, classScope);
            }

            method.Block.MethodReturn(value);

            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        public Type ReturnType {
            get => returnType;
        }

        public bool IsConstant {
            get => false;
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext evaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            FilterForEvalDeployTimeConstForge that = (FilterForEvalDeployTimeConstForge) o;

            return deployTimeConst.Equals(that.deployTimeConst);
        }

        public override int GetHashCode()
        {
            return deployTimeConst.GetHashCode();
        }
    }
} // end of namespace