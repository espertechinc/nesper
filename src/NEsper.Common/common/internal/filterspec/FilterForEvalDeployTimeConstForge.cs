///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Event property value in a list of values following an in-keyword.
    /// </summary>
    public class FilterForEvalDeployTimeConstForge : FilterSpecParamInValueForge
    {
        [JsonIgnore]
        [NonSerialized]
        private readonly ExprNodeDeployTimeConst _deployTimeConst;
        [JsonIgnore]
        [NonSerialized]
        private readonly Coercer _numberCoercer;
        [JsonIgnore]
        [NonSerialized]
        private readonly Type _returnType;

        public FilterForEvalDeployTimeConstForge(
            ExprNodeDeployTimeConst deployTimeConst,
            Coercer numberCoercer,
            Type returnType)
        {
            _deployTimeConst = deployTimeConst;
            _numberCoercer = numberCoercer;
            _returnType = returnType;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope)
                .AddParam(GET_FILTER_VALUE_FP);

            var value = _deployTimeConst.CodegenGetDeployTimeConstValue(classScope);
            if (_numberCoercer != null) {
                value = _numberCoercer.CoerceCodegenMayNullBoxed(value, _returnType, method, classScope);
            }

            method.Block.MethodReturn(value);

            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        public Type ReturnType => _returnType;

        public bool IsConstant => false;

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext evaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (FilterForEvalDeployTimeConstForge)o;

            return _deployTimeConst.Equals(that._deployTimeConst);
        }

        public override int GetHashCode()
        {
            return _deployTimeConst.GetHashCode();
        }

        public void ValueToString(StringBuilder @out)
        {
            @out.Append("deploy-time constant ");
            _deployTimeConst.RenderForFilterPlan(@out);
        }
    }
} // end of namespace