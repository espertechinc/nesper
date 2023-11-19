///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalStreamInsertTable : ExprForgeInstrumentable,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly int _streamNum;
        private readonly TableMetaData _tableMetadata;
        private readonly Type _returnType;

        public ExprEvalStreamInsertTable(
            int streamNum,
            TableMetaData tableMetadata,
            Type returnType)
        {
            _streamNum = streamNum;
            _tableMetadata = tableMetadata;
            _returnType = returnType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var refEps = exprSymbol.GetAddEps(codegenMethodScope);
            var refIsNewData = exprSymbol.GetAddIsNewData(codegenMethodScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            var eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(_tableMetadata, codegenClassScope, GetType());
            return StaticMethod(
                typeof(ExprEvalStreamInsertTable),
                "ConvertToTableEvent",
                Constant(_streamNum),
                eventToPublic,
                refEps,
                refIsNewData,
                refExprEvalCtx);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprStreamUndSelectClause",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="streamNum">stream num</param>
        /// <param name="eventToPublic">conversion</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">flag</param>
        /// <param name="exprEvaluatorContext">context</param>
        /// <returns>event</returns>
        public static EventBean ConvertToTableEvent(
            int streamNum,
            TableMetadataInternalEventToPublic eventToPublic,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream?[streamNum];
            if (@event != null) {
                @event = eventToPublic.Convert(@event, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return @event;
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => _returnType;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace