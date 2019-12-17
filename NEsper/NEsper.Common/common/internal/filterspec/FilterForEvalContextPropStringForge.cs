///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterForEvalContextPropStringForge : FilterSpecParamFilterForEvalForge
    {
        private readonly EventPropertyGetterSPI _getter;
        private readonly string _propertyName;

        public FilterForEvalContextPropStringForge(
            EventPropertyGetterSPI getter,
            string propertyName)
        {
            _getter = getter;
            _propertyName = propertyName;
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (exprEvaluatorContext.ContextProperties == null) {
                return null;
            }

            return _getter.Get(exprEvaluatorContext.ContextProperties);
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope).AddParam(GET_FILTER_VALUE_FP);

            method.Block
                .DeclareVar<EventBean>("props", ExprDotName(REF_EXPREVALCONTEXT, "ContextProperties"))
                .IfRefNullReturnNull(Ref("props"))
                .MethodReturn(_getter.EventBeanGetCodegen(Ref("props"), method, classScope));

            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (FilterForEvalContextPropStringForge) o;

            return _propertyName.Equals(that._propertyName);
        }

        public override int GetHashCode()
        {
            return _propertyName.GetHashCode();
        }
    }
} // end of namespace