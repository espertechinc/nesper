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
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public abstract class SelectEvalStreamBaseMap : SelectEvalStreamBase,
        SelectExprProcessorForge
    {
        protected SelectEvalStreamBaseMap(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard)
            : base(selectExprForgeContext, resultEventType, namedStreams, usingWildcard)

        {
        }

        public override CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var size = context.ExprForges.Length +
                       namedStreams.Count +
                       (isUsingWildcard && context.NumStreams > 1 ? context.NumStreams : 0);

            var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
            var refEPS = exprSymbol.GetAddEps(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);

            var init = size == 0
                ? StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(string), typeof(object) })
                : NewInstance(typeof(HashMap<string, object>));
            var block = methodNode.Block
                .DeclareVar<IDictionary<string, object>>("props", init);
            var count = 0;
            foreach (var forge in context.ExprForges) {
                block.Expression(
                    ExprDotMethod(
                        Ref("props"),
                        "Put",
                        Constant(context.ColumnNames[count]),
                        CodegenLegoMayVoid.ExpressionMayVoid(
                            typeof(object),
                            forge,
                            methodNode,
                            exprSymbol,
                            codegenClassScope)));
                count++;
            }

            foreach (var element in namedStreams) {
                var theEvent = ArrayAtIndex(refEPS, Constant(element.StreamNumber));
                if (element.TableMetadata != null) {
                    var eventToPublic = TableDeployTimeResolver.MakeTableEventToPublicField(
                        element.TableMetadata,
                        codegenClassScope,
                        GetType());
                    theEvent = ExprDotMethod(eventToPublic, "Convert", theEvent, refEPS, refIsNewData, refExprEvalCtx);
                }

                block.Expression(ExprDotMethod(Ref("props"), "Put", Constant(context.ColumnNames[count]), theEvent));
                count++;
            }

            if (isUsingWildcard && context.NumStreams > 1) {
                for (var i = 0; i < context.NumStreams; i++) {
                    block.Expression(
                        ExprDotMethod(
                            Ref("props"),
                            "Put",
                            Constant(context.ColumnNames[count]),
                            ArrayAtIndex(refEPS, Constant(i))));
                    count++;
                }
            }

            block.MethodReturn(
                ProcessSpecificCodegen(
                    resultEventType,
                    eventBeanFactory,
                    Ref("props"),
                    methodNode,
                    exprSymbol,
                    codegenClassScope));
            return methodNode;
        }

        protected abstract CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpression props,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace