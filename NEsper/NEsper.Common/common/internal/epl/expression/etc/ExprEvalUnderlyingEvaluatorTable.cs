///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly int streamNum;
        private readonly Type resultType;
        private readonly TableMetaData tableMetadata;

        public ExprEvalUnderlyingEvaluatorTable(
            int streamNum,
            Type resultType,
            TableMetaData tableMetadata)
        {
            this.streamNum = streamNum;
            this.resultType = resultType;
            this.tableMetadata = tableMetadata;
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public Type EvaluationType {
            get => resultType;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable() {
                    ProcToEPL = (
                        writer,
                        parentPrecedence) => {
                        writer.Write(this.GetType().Name);
                    },
                };
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
            CodegenExpressionInstanceField eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(tableMetadata, codegenClassScope, this.GetType());
            CodegenMethod method = parent.MakeChild(
                typeof(object[]),
                typeof(ExprEvalUnderlyingEvaluatorTable),
                codegenClassScope);
            method.Block.IfRefNullReturnNull(exprSymbol.GetAddEPS(method))
                .DeclareVar<EventBean>("@event", ArrayAtIndex(exprSymbol.GetAddEPS(method), Constant(streamNum)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(
                    ExprDotMethod(
                        eventToPublic,
                        "ConvertToUnd",
                        Ref("@event"),
                        exprSymbol.GetAddEPS(method),
                        exprSymbol.GetAddIsNewData(method),
                        exprSymbol.GetAddExprEvalCtx(method)));
            return LocalMethod(method);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace