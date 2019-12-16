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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public class ExprTableIdentNode : ExprNodeBase,
        ExprForgeInstrumentable
    {
        private readonly TableMetaData tableMetadata;
        private readonly string streamOrPropertyName;
        private readonly string unresolvedPropertyName;
        private readonly Type returnType;
        private readonly int streamNum;
        private readonly int columnNum;

        public ExprTableIdentNode(
            TableMetaData tableMetadata,
            string streamOrPropertyName,
            string unresolvedPropertyName,
            Type returnType,
            int streamNum,
            int columnNum)
        {
            this.tableMetadata = tableMetadata;
            this.streamOrPropertyName = streamOrPropertyName;
            this.unresolvedPropertyName = unresolvedPropertyName;
            this.returnType = returnType;
            this.streamNum = streamNum;
            this.columnNum = columnNum;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExprIdentNodeImpl.ToPrecedenceFreeEPL(writer, streamOrPropertyName, unresolvedPropertyName);
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.UNARY;
        }

        public bool IsConstantResult {
            get => false;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod method = parent.MakeChild(requiredType, this.GetType(), codegenClassScope);
            method.Block.DeclareVar<object>(
                "result",
                StaticMethod(
                    typeof(ExprTableIdentNode),
                    "TableColumnAggValue",
                    Constant(streamNum),
                    Constant(columnNum),
                    symbols.GetAddEPS(method),
                    symbols.GetAddIsNewData(method),
                    symbols.GetAddExprEvalCtx(method)));
            if (requiredType == typeof(object)) {
                method.Block.MethodReturn(Ref("result"));
            }
            else {
                method.Block.MethodReturn(Cast(Boxing.GetBoxedType(requiredType), Ref("result")));
            }

            return LocalMethod(method);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    this.GetType(),
                    this,
                    "ExprTableSubproperty",
                    requiredType,
                    parent,
                    symbols,
                    codegenClassScope)
                .Qparams(new CodegenExpression[] {Constant(tableMetadata.TableName), Constant(unresolvedPropertyName)})
                .Build();
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator() {
                    ProcEvaluate = (
                        eventsPerStream,
                        isNewData,
                        context) => {
                        throw new UnsupportedOperationException("Cannot evaluate at compile time");
                    },
                };
            }
        }

        public Type EvaluationType {
            get => returnType;
        }

        public override ExprForge Forge {
            get => this;
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprNode ForgeRenderable {
            get => this;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="streamNum">stream num</param>
        /// <param name="column">col</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new-data flow</param>
        /// <param name="ctx">context</param>
        /// <returns>value</returns>
        public static object TableColumnAggValue(
            int streamNum,
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext ctx)
        {
            ObjectArrayBackedEventBean oa = (ObjectArrayBackedEventBean) eventsPerStream[streamNum];
            AggregationRow row = ExprTableEvalStrategyUtil.GetRow(oa);
            return row.GetValue(column, eventsPerStream, isNewData, ctx);
        }
    }
} // end of namespace