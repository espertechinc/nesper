///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public abstract class SelectEvalStreamBaseObjectArray : SelectEvalStreamBase
    {
        public SelectEvalStreamBaseObjectArray(
            SelectExprForgeContext context,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard)
            : base(context, resultEventType, namedStreams, usingWildcard)

        {
        }

        protected abstract CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpressionRef props,
            CodegenClassScope codegenClassScope);

        public override CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var size = ComputeSize();
            var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
            var refEPS = exprSymbol.GetAddEps(methodNode);

            var block = methodNode.Block
                .DeclareVar<object[]>("props", NewArrayByLength(typeof(object), Constant(size)));
            var count = 0;
            foreach (var forge in context.ExprForges) {
                block.AssignArrayElement(
                    Ref("props"),
                    Constant(count),
                    CodegenLegoMayVoid.ExpressionMayVoid(
                        typeof(object),
                        forge,
                        methodNode,
                        exprSymbol,
                        codegenClassScope));
                count++;
            }

            foreach (var element in namedStreams) {
                var theEvent = ArrayAtIndex(refEPS, Constant(element.StreamNumber));
                block.AssignArrayElement(Ref("props"), Constant(count), theEvent);
                count++;
            }

            if (isUsingWildcard && context.NumStreams > 1) {
                for (var i = 0; i < context.NumStreams; i++) {
                    block.AssignArrayElement(Ref("props"), Constant(count), ArrayAtIndex(refEPS, Constant(i)));
                    count++;
                }
            }

            block.MethodReturn(
                ProcessSpecificCodegen(resultEventType, eventBeanFactory, Ref("props"), codegenClassScope));
            return methodNode;
        }

        private int ComputeSize()
        {
            // Evaluate all expressions and build a map of name-value pairs
            var size = isUsingWildcard && context.NumStreams > 1 ? context.NumStreams : 0;
            size += context.ExprForges.Length + namedStreams.Count;
            return size;
        }
    }
} // end of namespace