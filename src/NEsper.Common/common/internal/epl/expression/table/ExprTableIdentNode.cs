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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public class ExprTableIdentNode : ExprNodeBase,
        ExprForgeInstrumentable
    {
        private readonly int _columnNum;
        private readonly string _columnName;
        private readonly int _streamNum;
        private readonly string _streamOrPropertyName;
        private readonly TableMetaData _tableMetadata;
        private readonly string _unresolvedPropertyName;

        public ExprTableIdentNode(
            TableMetaData tableMetadata,
            string streamOrPropertyName,
            string unresolvedPropertyName,
            Type returnType,
            int streamNum,
            string columnName,
            int columnNum)
        {
            _tableMetadata = tableMetadata;
            _streamOrPropertyName = streamOrPropertyName;
            _unresolvedPropertyName = unresolvedPropertyName;
            EvaluationType = returnType;
            _streamNum = streamNum;
            _columnNum = columnNum;
            _columnName = columnName;
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsConstantResult => false;

        public override ExprForge Forge => this;

        public ExprNode ForgeRenderable => this;

        public int ColumnNum => _columnNum;

        public string ColumnName => _columnName;

        public int StreamNum => _streamNum;

        public TableMetaData TableMetadata => _tableMetadata;

        public string UnresolvedPropertyName => _unresolvedPropertyName;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            var method = parent.MakeChild(requiredType, GetType(), codegenClassScope);
            method.Block.DeclareVar<object>(
                "result",
                StaticMethod(
                    typeof(ExprTableIdentNode),
                    "TableColumnAggValue",
                    Constant(_streamNum),
                    Constant(_columnNum),
                    symbols.GetAddEps(method),
                    symbols.GetAddIsNewData(method),
                    symbols.GetAddExprEvalCtx(method)));
            if (requiredType == typeof(object)) {
                method.Block.MethodReturn(Ref("result"));
            }
            else {
                method.Block.MethodReturn(Cast(requiredType.GetBoxedType(), Ref("result")));
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
                    GetType(),
                    this,
                    "ExprTableSubproperty",
                    requiredType,
                    parent,
                    symbols,
                    codegenClassScope)
                .Qparams(Constant(_tableMetadata.TableName), Constant(_unresolvedPropertyName))
                .Build();
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator {
                    ProcEvaluate = (
                        eventsPerStream,
                        isNewData,
                        context) => {
                        throw new UnsupportedOperationException("Cannot evaluate at compile time");
                    }
                };
            }
        }

        public Type EvaluationType { get; }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ExprIdentNodeImpl.ToPrecedenceFreeEPL(
                writer,
                _streamOrPropertyName,
                _unresolvedPropertyName,
                ExprNodeRenderableFlags.DEFAULTFLAGS);
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

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="streamNum">stream num</param>
        /// <param name="eventsPerStream">events</param>
        /// <returns>value</returns>
        public static AggregationRow TableColumnRow(
            int streamNum,
            EventBean[] eventsPerStream)
        {
            var oa = (ObjectArrayBackedEventBean)eventsPerStream[streamNum];
            return ExprTableEvalStrategyUtil.GetRow(oa);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
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
            var oa = (ObjectArrayBackedEventBean)eventsPerStream[streamNum];
            var row = ExprTableEvalStrategyUtil.GetRow(oa);
            return row.GetValue(column, eventsPerStream, isNewData, ctx);
        }
    }
} // end of namespace