///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Event property value in a list of values following an in-keyword.
    /// </summary>
    public class FilterForEvalContextPropForge : FilterSpecParamInValueForge
    {
        [NonSerialized] private readonly EventPropertyGetterSPI _getter;
        [NonSerialized] private readonly SimpleNumberCoercer _numberCoercer;
        private readonly string _propertyName;
        [NonSerialized] private readonly Type _returnType;

        public FilterForEvalContextPropForge(
            string propertyName,
            EventPropertyGetterSPI getter,
            SimpleNumberCoercer coercer,
            Type returnType)
        {
            _propertyName = propertyName;
            _getter = getter;
            _numberCoercer = coercer;
            _returnType = returnType;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope).AddParam(GET_FILTER_VALUE_FP);

            method.Block
                .DeclareVar<EventBean>("props", ExprDotName(REF_EXPREVALCONTEXT, "ContextProperties"))
                .IfRefNullReturnNull(Ref("props"))
                .DeclareVar<object>("result", _getter.EventBeanGetCodegen(Ref("props"), method, classScope));
            if (_numberCoercer != null) {
                method.Block.AssignRef(
                    "result",
                    _numberCoercer.CoerceCodegenMayNullBoxed(
                        Cast(typeof(object), Ref("result")),
                        typeof(object),
                        method,
                        classScope));
            }

            method.Block.MethodReturn(Ref("result"));

            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        public Type ReturnType => _returnType;

        public bool IsConstant => false;

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext evaluatorContext)
        {
            if (evaluatorContext.ContextProperties == null) {
                return null;
            }

            var result = _getter.Get(evaluatorContext.ContextProperties);

            if (_numberCoercer == null) {
                return result;
            }

            return _numberCoercer.CoerceBoxed(result);
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (FilterForEvalContextPropForge) o;

            if (!_propertyName.Equals(that._propertyName)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _propertyName.GetHashCode();
        }
    }
} // end of namespace