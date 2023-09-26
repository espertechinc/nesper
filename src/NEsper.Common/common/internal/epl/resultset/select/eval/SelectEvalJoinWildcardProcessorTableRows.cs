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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    /// <summary>
    /// Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
    /// </summary>
    public class SelectEvalJoinWildcardProcessorTableRows : SelectExprProcessorForge
    {
        private readonly SelectExprProcessorForge innerForge;
        private readonly TableMetaData[] tables;
        private readonly EventType[] types;

        public SelectEvalJoinWildcardProcessorTableRows(
            EventType[] types,
            SelectExprProcessorForge inner,
            TableCompileTimeResolver tableResolver)
        {
            this.types = types;
            innerForge = inner;
            tables = new TableMetaData[types.Length];
            for (var i = 0; i < types.Length; i++) {
                tables[i] = tableResolver.ResolveTableFromEventType(types[i]);
            }
        }

        public EventType ResultEventType => innerForge.ResultEventType;

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            methodNode.Block.DeclareVar<EventBean[]>(
                "eventsPerStreamWTableRows",
                NewArrayByLength(typeof(EventBean), Constant(types.Length)));
            for (var i = 0; i < types.Length; i++) {
                if (tables[i] == null) {
                    methodNode.Block.AssignArrayElement(
                        "eventsPerStreamWTableRows",
                        Constant(i),
                        ArrayAtIndex(refEPS, Constant(i)));
                }
                else {
                    var eventToPublic =
                        TableDeployTimeResolver.MakeTableEventToPublicField(
                            tables[i],
                            codegenClassScope,
                            GetType());
                    var refname = "e" + i;
                    methodNode.Block.DeclareVar<EventBean>(refname, ArrayAtIndex(refEPS, Constant(i)))
                        .IfRefNotNull(refname)
                        .AssignArrayElement(
                            "eventsPerStreamWTableRows",
                            Constant(i),
                            ExprDotMethod(
                                eventToPublic,
                                "Convert",
                                Ref(refname),
                                refEPS,
                                refIsNewData,
                                refExprEvalCtx))
                        .BlockEnd();
                }
            }

            var innerMethod = innerForge.ProcessCodegen(
                resultEventType,
                eventBeanFactory,
                codegenMethodScope,
                selectSymbol,
                exprSymbol,
                codegenClassScope);
            methodNode.Block.AssignRef(refEPS.Ref, Ref("eventsPerStreamWTableRows"))
                .MethodReturn(LocalMethod(innerMethod));
            return methodNode;
        }
    }
} // end of namespace