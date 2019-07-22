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
        private readonly int streamNum;
        private readonly TableMetaData tableMetadata;
        private readonly Type returnType;

        public ExprEvalStreamInsertTable(
            int streamNum,
            TableMetaData tableMetadata,
            Type returnType)
        {
            this.streamNum = streamNum;
            this.tableMetadata = tableMetadata;
            this.returnType = returnType;
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
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(codegenMethodScope);
            CodegenExpression refIsNewData = exprSymbol.GetAddIsNewData(codegenMethodScope);
            CodegenExpressionRef refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            CodegenExpressionField eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(tableMetadata, codegenClassScope, this.GetType());
            return StaticMethod(
                typeof(ExprEvalStreamInsertTable),
                "convertToTableEvent",
                Constant(streamNum),
                eventToPublic,
                refEPS,
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
                this.GetType(),
                this,
                "ExprStreamUndSelectClause",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

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
            EventBean @event = eventsPerStream == null ? null : eventsPerStream[streamNum];
            if (@event != null) {
                @event = eventToPublic.Convert(@event, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return @event;
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public Type EvaluationType {
            get => returnType;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => this;
        }

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(this.GetType().Name);
        }
    }
} // end of namespace