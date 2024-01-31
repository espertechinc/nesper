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
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalUnderlyingEvaluatorTable : ExprEvaluator,
        ExprForge
    {
        private readonly int _streamNum;
        private readonly Type _resultType;
        private readonly TableMetaData _tableMetadata;

        public ExprEvalUnderlyingEvaluatorTable(
            int streamNum,
            Type resultType,
            TableMetaData tableMetadata)
        {
            _streamNum = streamNum;
            _resultType = resultType;
            _tableMetadata = tableMetadata;
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => _resultType;

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable(
                    (
                        writer,
                        parentPrecedence,
                        flags) => {
                        writer.Write(GetType().Name);
                    });
            }
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(_tableMetadata, codegenClassScope, GetType());
            var method = parent.MakeChild(
                typeof(object[]),
                typeof(ExprEvalUnderlyingEvaluatorTable),
                codegenClassScope);
            method.Block.IfNullReturnNull(exprSymbol.GetAddEps(method))
                .DeclareVar<EventBean>("@event", ArrayAtIndex(exprSymbol.GetAddEps(method), Constant(_streamNum)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(
                    ExprDotMethod(
                        eventToPublic,
                        "ConvertToUnd",
                        Ref("@event"),
                        exprSymbol.GetAddEps(method),
                        exprSymbol.GetAddIsNewData(method),
                        exprSymbol.GetAddExprEvalCtx(method)));
            return LocalMethod(method);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace