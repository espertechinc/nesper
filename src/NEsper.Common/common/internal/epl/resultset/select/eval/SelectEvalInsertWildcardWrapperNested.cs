///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertWildcardWrapperNested : SelectEvalBaseMap,
        SelectExprProcessorForge
    {
        private readonly WrapperEventType innerWrapperType;

        public SelectEvalInsertWildcardWrapperNested(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            WrapperEventType innerWrapperType)
            : base(selectExprForgeContext, resultEventType)

        {
            this.innerWrapperType = innerWrapperType;
        }

        protected override CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpression props,
            CodegenMethod methodNode,
            SelectExprProcessorCodegenSymbol selectEnv,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var innerType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(innerWrapperType, EPStatementInitServicesConstants.REF));
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            return StaticMethod(
                GetType(),
                "WildcardNestedWrapper",
                ArrayAtIndex(refEPS, Constant(0)),
                innerType,
                resultEventType,
                eventBeanFactory,
                props);
        }

        public static EventBean WildcardNestedWrapper(
            EventBean @event,
            EventType innerWrapperType,
            EventType outerWrapperType,
            EventBeanTypedEventFactory factory,
            IDictionary<string, object> props)
        {
            var inner = factory.AdapterForTypedWrapper(
                @event,
                EmptyDictionary<string, object>.Instance,
                innerWrapperType);
            return factory.AdapterForTypedWrapper(inner, props, outerWrapperType);
        }
    }
} // end of namespace