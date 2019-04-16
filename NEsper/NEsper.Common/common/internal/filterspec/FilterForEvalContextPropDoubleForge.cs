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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterForEvalContextPropDoubleForge : FilterSpecParamFilterForEvalDoubleForge
    {
        [NonSerialized] private readonly EventPropertyGetterSPI _getter;
        private readonly string _propertyName;

        public FilterForEvalContextPropDoubleForge(
            EventPropertyGetterSPI getter,
            string propertyName)
        {
            this._getter = getter;
            this._propertyName = propertyName;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            CodegenMethod method = parent.MakeChild(typeof(object), this.GetType(), classScope).AddParam(GET_FILTER_VALUE_FP);

            method.Block
                .DeclareVar(typeof(EventBean), "props", ExprDotMethod(REF_EXPREVALCONTEXT, "getContextProperties"))
                .IfRefNullReturnNull(@Ref("props"))
                .DeclareVar(typeof(object), "result", _getter.EventBeanGetCodegen(@Ref("props"), method, classScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethod(Cast(typeof(object), @Ref("result")), "doubleValue"));

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

            object @object = _getter.Get(exprEvaluatorContext.ContextProperties);
            if (@object == null) {
                return null;
            }

            return @object.AsDouble();
        }

        public double? GetFilterValueDouble(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            FilterForEvalContextPropDoubleForge that = (FilterForEvalContextPropDoubleForge) o;

            return _propertyName.Equals(that._propertyName);
        }

        public override int GetHashCode()
        {
            return _propertyName.GetHashCode();
        }
    }
} // end of namespace