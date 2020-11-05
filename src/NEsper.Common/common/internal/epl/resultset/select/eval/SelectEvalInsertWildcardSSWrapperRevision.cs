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
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertWildcardSSWrapperRevision : SelectEvalBaseMap
    {
        private readonly VariantEventType variantEventType;

        public SelectEvalInsertWildcardSSWrapperRevision(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            VariantEventType variantEventType)
            : base(selectExprForgeContext, resultEventType)

        {
            this.variantEventType = variantEventType;
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
            var type = VariantEventTypeUtil.GetField(variantEventType, codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            return StaticMethod(
                typeof(SelectEvalInsertWildcardSSWrapperRevision),
                "SelectExprInsertWildcardSSWrapRevision",
                refEPS,
                evaluators == null ? Constant(0) : Constant(evaluators.Length),
                props,
                type);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventsPerStream">events</param>
        /// <param name="numEvaluators">num evals</param>
        /// <param name="props">props</param>
        /// <param name="variantEventType">variant</param>
        /// <returns>bean</returns>
        public static EventBean SelectExprInsertWildcardSSWrapRevision(
            EventBean[] eventsPerStream,
            int numEvaluators,
            IDictionary<string, object> props,
            VariantEventType variantEventType)
        {
            DecoratingEventBean wrapper = (DecoratingEventBean) eventsPerStream[0];
            if (wrapper != null) {
                IDictionary<string, object> map = wrapper.DecoratingProperties;
                if ((numEvaluators == 0) && (!map.IsEmpty())) {
                    // no action
                }
                else {
                    props.PutAll(map);
                }
            }

            EventBean theEvent = eventsPerStream[0];
            return variantEventType.GetValueAddEventBean(theEvent);
        }
    }
} // end of namespace