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
    /// Processor for select-clause expressions that handles wildcards for single streams with no insert-into.
    /// </summary>
    public class SelectEvalWildcardTable : SelectExprProcessorForge
    {
        private readonly TableMetaData table;

        public SelectEvalWildcardTable(TableMetaData table)
        {
            this.table = table;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionInstanceField eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(table, codegenClassScope, this.GetType());
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                this.GetType(),
                codegenClassScope);
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            CodegenExpression refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            CodegenExpressionRef refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(0)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(
                    ExprDotMethod(eventToPublic, "Convert", Ref("@event"), refEPS, refIsNewData, refExprEvalCtx));
            return methodNode;
        }

        public EventType ResultEventType {
            get => table.PublicEventType;
        }
    }
} // end of namespace