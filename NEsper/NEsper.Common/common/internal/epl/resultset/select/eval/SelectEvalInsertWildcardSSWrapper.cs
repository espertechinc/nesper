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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertWildcardSSWrapper : SelectEvalBaseMap
    {
        public SelectEvalInsertWildcardSSWrapper(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType)
            : base(selectExprForgeContext, resultEventType)

        {
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
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            return StaticMethod(
                typeof(SelectEvalInsertWildcardSSWrapper),
                "ProcessSelectExprSSWrapper",
                props,
                refEPS,
                Constant(context.ExprForges.Length == 0),
                eventBeanFactory,
                resultEventType);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="props">props</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="emptyExpressions">flag</param>
        /// <param name="eventBeanTypedEventFactory">svc</param>
        /// <param name="resultEventType">type</param>
        /// <returns>bean</returns>
        public static EventBean ProcessSelectExprSSWrapper(
            IDictionary<string, object> props,
            EventBean[] eventsPerStream,
            bool emptyExpressions,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType resultEventType)
        {
            EventBean theEvent = eventsPerStream[0];
            return eventBeanTypedEventFactory.AdapterForTypedWrapper(theEvent, props, resultEventType);
        }
    }
} // end of namespace