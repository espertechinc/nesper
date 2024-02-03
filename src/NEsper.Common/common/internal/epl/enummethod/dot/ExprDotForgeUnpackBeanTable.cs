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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotForgeUnpackBeanTable : ExprDotForge,
        ExprDotEval
    {
        private readonly EPChainableType returnType;
        private readonly TableMetaData tableMetadata;

        public ExprDotForgeUnpackBeanTable(
            EventType lambdaType,
            TableMetaData tableMetadata)
        {
            this.tableMetadata = tableMetadata;
            returnType = EPChainableTypeHelper.SingleValue(tableMetadata.PublicEventType.UnderlyingType);
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException("Table-row eval not available at compile time");
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(tableMetadata, classScope, GetType());
            var methodNode = parent.MakeChild(typeof(object[]), typeof(ExprDotForgeUnpackBeanTable), classScope)
                .AddParam<EventBean>("target");
            var refEPS = symbols.GetAddEps(methodNode);
            var refIsNewData = symbols.GetAddIsNewData(methodNode);
            var refExprEvalCtx = symbols.GetAddExprEvalCtx(methodNode);
            methodNode.Block.IfRefNullReturnNull("target")
                .MethodReturn(
                    ExprDotMethod(eventToPublic, "ConvertToUnd", Ref("target"), refEPS, refIsNewData, refExprEvalCtx));
            return LocalMethod(methodNode, inner);
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEvent();
        }

        public EPChainableType TypeInfo => returnType;

        public ExprDotEval DotEvaluator => this;

        public ExprDotForge DotForge => this;
    }
} // end of namespace