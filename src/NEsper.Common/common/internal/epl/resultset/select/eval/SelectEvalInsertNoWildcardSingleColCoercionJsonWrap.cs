///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertNoWildcardSingleColCoercionJsonWrap : SelectEvalBaseFirstPropFromWrap
    {
        public SelectEvalInsertNoWildcardSingleColCoercionJsonWrap(
            SelectExprForgeContext selectExprForgeContext,
            WrapperEventType wrapper)
            : base(selectExprForgeContext, wrapper)
        {
        }

        protected override CodegenExpression ProcessFirstColCodegen(
            Type evaluationType,
            CodegenExpression expression,
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var memberUndType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(JsonEventType),
                Cast(
                    typeof(JsonEventType),
                    EventTypeUtility.ResolveTypeCodegen(
                        wrapper.UnderlyingEventType,
                        EPStatementInitServicesConstants.REF)));
            var memberWrapperType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(WrapperEventType),
                Cast(
                    typeof(WrapperEventType),
                    EventTypeUtility.ResolveTypeCodegen(wrapper, EPStatementInitServicesConstants.REF)));
            var method = codegenMethodScope
                .MakeChild(typeof(EventBean), GetType(), codegenClassScope)
                .AddParam(evaluationType, "result")
                .Block
                .DeclareVar<string>("json", Cast(typeof(string), Ref("result")))
                .IfNullReturnNull(Ref("json"))
                .DeclareVar<object>(
                    "und",
                    ExprDotMethod(memberUndType, "Parse", Ref("json")))
                .DeclareVar<EventBean>("bean",
                    ExprDotMethod(eventBeanFactory, "AdapterForTypedJson", Ref("und"), memberUndType))
                .MethodReturn(
                    ExprDotMethod(
                        eventBeanFactory,
                        "AdapterForTypedWrapper",
                        Ref("bean"),
                        EnumValue(typeof(EmptyDictionary<string, object>), "Instance"),
                        memberWrapperType));
            return LocalMethodBuild(method).Pass(expression).Call();
        }
    }
} // end of namespace