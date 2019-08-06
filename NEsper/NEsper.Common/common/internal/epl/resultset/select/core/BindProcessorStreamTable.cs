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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class BindProcessorStreamTable : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly int streamNum;
        private readonly Type returnType;
        private readonly TableMetaData tableMetadata;

        public BindProcessorStreamTable(
            int streamNum,
            Type returnType,
            TableMetaData tableMetadata)
        {
            this.streamNum = streamNum;
            this.returnType = returnType;
            this.tableMetadata = tableMetadata;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(tableMetadata, codegenClassScope, this.GetType());
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(codegenMethodScope);
            CodegenExpression refIsNewData = exprSymbol.GetAddIsNewData(codegenMethodScope);
            CodegenExpressionRef refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            return StaticMethod(
                typeof(BindProcessorStreamTable),
                "EvaluateConvertTableEventToUnd",
                Constant(streamNum),
                eventToPublic,
                refEPS,
                refIsNewData,
                refExprEvalCtx);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="streamNum">stream</param>
        /// <param name="eventToPublic">conversion</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">flag</param>
        /// <param name="context">context</param>
        /// <returns>event</returns>
        public static object[] EvaluateConvertTableEventToUnd(
            int streamNum,
            TableMetadataInternalEventToPublic eventToPublic,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            EventBean @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            return eventToPublic.ConvertToUnd(@event, eventsPerStream, isNewData, context);
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