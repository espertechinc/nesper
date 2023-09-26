///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeAggregationMethodForgeLocal : ExprDotNodeAggregationMethodForge
    {
        private readonly ExprAggMultiFunctionNode agg;

        public ExprDotNodeAggregationMethodForgeLocal(
            ExprDotNodeImpl parent,
            string aggregationMethodName,
            ExprNode[] parameters,
            AggregationPortableValidation validation,
            ExprAggMultiFunctionNode agg)
            : base(parent, aggregationMethodName, parameters, validation)
        {
            this.agg = agg;
        }

        protected override string TableName => null;

        protected override string TableColumnName => null;

        public override bool IsLocalInlinedClass => false;

        protected override CodegenExpression EvaluateCodegen(
            string readerMethodName,
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var future = agg.GetAggFuture(classScope);
            var method = parent.MakeChild(requiredType, GetType(), classScope);
            method.Block
                .DeclareVar<AggregationRow>("row",
                    ExprDotMethod(
                        future,
                        "GetAggregationRow",
                        ExprDotName(symbols.GetAddExprEvalCtx(parent), "AgentInstanceId"),
                        symbols.GetAddEPS(parent),
                        symbols.GetAddIsNewData(parent),
                        symbols.GetAddExprEvalCtx(parent)))
                .IfRefNullReturnNull("row")
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        requiredType,
                        ExprDotMethod(
                            GetReader(classScope),
                            readerMethodName,
                            Constant(agg.Column),
                            Ref("row"),
                            symbols.GetAddEPS(method),
                            symbols.GetAddIsNewData(method),
                            symbols.GetAddExprEvalCtx(method))));
            return LocalMethod(method);
        }

        protected override void ToEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            agg.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
        }
    }
} // end of namespace