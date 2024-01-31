///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalStreamTable : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly int _streamNum;
        private readonly Type _returnType;
        private readonly TableMetaData _tableMetadata;

        public ExprEvalStreamTable(
            int streamNum,
            Type returnType,
            TableMetaData tableMetadata)
        {
            _streamNum = streamNum;
            _returnType = returnType;
            _tableMetadata = tableMetadata;
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
            var eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(_tableMetadata, codegenClassScope, GetType());
            var refEPS = exprSymbol.GetAddEps(codegenMethodScope);
            var refIsNewData = exprSymbol.GetAddIsNewData(codegenMethodScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            return StaticMethod(
                typeof(ExprEvalStreamTable),
                "EvaluateConvertTableEventToUnd",
                Constant(_streamNum),
                eventToPublic,
                refEPS,
                refIsNewData,
                refExprEvalCtx);
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
            var @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            return eventToPublic.ConvertToUnd(@event, eventsPerStream, isNewData, context);
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

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace