///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterForEvalContextPropDoubleForge : FilterSpecParamFilterForEvalDoubleForge
    {
        [JsonIgnore]
        [NonSerialized]
        private readonly EventPropertyGetterSPI _getter;
        private readonly string _propertyName;

        public FilterForEvalContextPropDoubleForge(
            EventPropertyGetterSPI getter,
            string propertyName)
        {
            _getter = getter;
            _propertyName = propertyName;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope)
                .AddParam(GET_FILTER_VALUE_FP);

            method.Block
                .DeclareVar<EventBean>("props", ExprDotName(REF_EXPREVALCONTEXT, "ContextProperties"))
                .IfNullReturnNull(Ref("props"))
                .DeclareVar<object>("result", _getter.EventBeanGetCodegen(Ref("props"), method, classScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(Cast(typeof(object), Ref("result")), "AsDouble"));

            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        object FilterSpecParamFilterForEvalForge.GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext);
        }

        public double? GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (exprEvaluatorContext.ContextProperties == null) {
                return null;
            }

            var @object = _getter.Get(exprEvaluatorContext.ContextProperties);

            return @object?.AsDouble();
        }

        public double? GetFilterValueDouble(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext);
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (FilterForEvalContextPropDoubleForge)o;

            return _propertyName.Equals(that._propertyName);
        }

        public override int GetHashCode()
        {
            return _propertyName.GetHashCode();
        }

        public void ValueToString(StringBuilder @out)
        {
            @out.Append("context property '")
                .Append(_propertyName)
                .Append("'");
        }
    }
} // end of namespace