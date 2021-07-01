///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertNoWildcardSingleColCoercionMapWrap : SelectEvalBaseFirstPropFromWrap
    {
        public SelectEvalInsertNoWildcardSingleColCoercionMapWrap(
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
            return ProcessFirstColCodegen(
                expression,
                eventBeanFactory,
                codegenClassScope,
                wrapper,
                "AdapterForTypedMap",
                typeof(IDictionary<string, object>));
        }

        public static CodegenExpression ProcessFirstColCodegen(
            CodegenExpression expression,
            CodegenExpression eventBeanFactory,
            CodegenClassScope codegenClassScope,
            WrapperEventType wrapperEventType,
            string adapterMethod,
            Type castType)
        {
            var memberUndType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(
                    wrapperEventType.UnderlyingEventType,
                    EPStatementInitServicesConstants.REF));
            var memberWrapperType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(WrapperEventType),
                Cast(
                    typeof(WrapperEventType),
                    EventTypeUtility.ResolveTypeCodegen(wrapperEventType, EPStatementInitServicesConstants.REF)));
            var wrapped = ExprDotMethod(
                eventBeanFactory,
                adapterMethod,
                castType == typeof(object) ? expression : Cast(castType, expression),
                memberUndType);
            return ExprDotMethod(
                eventBeanFactory,
                "AdapterForTypedWrapper",
                wrapped,
                StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(string), typeof(object) }),
                memberWrapperType);
        }
    }
} // end of namespace