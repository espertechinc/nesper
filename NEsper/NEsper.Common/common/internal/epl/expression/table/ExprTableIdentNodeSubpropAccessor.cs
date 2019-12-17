///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public class ExprTableIdentNodeSubpropAccessor : ExprNodeBase,
        ExprForgeInstrumentable,
        ExprEvaluator,
        ExprEnumerationForge,
        ExprEnumerationEval
    {
        private readonly ExprNode _aggregateAccessMultiValueNode;
        private readonly string _optionalStreamName;
        private readonly int _streamNum;
        private readonly TableMetaData _table;
        private readonly TableMetadataColumnAggregation _tableAccessColumn;
        private AggregationTableReadDesc _tableAccessDesc;

        public ExprTableIdentNodeSubpropAccessor(
            int streamNum,
            string optionalStreamName,
            TableMetaData table,
            TableMetadataColumnAggregation tableAccessColumn,
            ExprNode aggregateAccessMultiValueNode)
        {
            _streamNum = streamNum;
            _optionalStreamName = optionalStreamName;
            _table = table;
            _tableAccessColumn = tableAccessColumn;
            _aggregateAccessMultiValueNode = aggregateAccessMultiValueNode;
        }

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsConstantResult => false;

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var reader = classScope.AddOrGetDefaultFieldSharable(
                new AggregationTableAccessAggReaderCodegenField(_tableAccessDesc.Reader, classScope, GetType()));
            return StaticMethod(
                typeof(ExprTableIdentNodeSubpropAccessor),
                "EvaluateTableWithReaderCollectionEvents",
                Constant(_streamNum),
                reader,
                Constant(_tableAccessColumn.Column),
                symbols.GetAddEPS(parent),
                symbols.GetAddIsNewData(parent),
                symbols.GetAddExprEvalCtx(parent));
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return _tableAccessDesc.EventTypeCollection;
        }

        public Type ComponentTypeCollection => _tableAccessDesc.ComponentTypeCollection;

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var reader = classScope.AddOrGetDefaultFieldSharable(
                new AggregationTableAccessAggReaderCodegenField(_tableAccessDesc.Reader, classScope, GetType()));
            return StaticMethod(
                typeof(ExprTableIdentNodeSubpropAccessor),
                "EvaluateTableWithReaderCollectionScalar",
                Constant(_streamNum),
                reader,
                Constant(_tableAccessColumn.Column),
                symbols.GetAddEPS(parent),
                symbols.GetAddIsNewData(parent),
                symbols.GetAddExprEvalCtx(parent));
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return _tableAccessDesc.EventTypeSingle;
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
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
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var reader = classScope.AddOrGetDefaultFieldSharable(
                new AggregationTableAccessAggReaderCodegenField(_tableAccessDesc.Reader, classScope, GetType()));
            return CodegenLegoCast.CastSafeFromObjectType(
                requiredType,
                StaticMethod(
                    typeof(ExprTableIdentNodeSubpropAccessor),
                    "EvaluateTableWithReader",
                    Constant(_streamNum),
                    reader,
                    Constant(_tableAccessColumn.Column),
                    symbols.GetAddEPS(parent),
                    symbols.GetAddIsNewData(parent),
                    symbols.GetAddExprEvalCtx(parent)));
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return EvaluateCodegenUninstrumented(requiredType, parent, symbols, classScope);
        }

        public Type EvaluationType => _tableAccessDesc.Reader.ResultType;

        public ExprNodeRenderable EnumForgeRenderable => this;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public ExprEvaluator ExprEvaluator => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (_tableAccessColumn.IsMethodAgg) {
                throw new ExprValidationException("Invalid combination of aggregation state and aggregation accessor");
            }

            var mfNode = (ExprAggMultiFunctionNode) _aggregateAccessMultiValueNode;
            mfNode.ValidatePositionals(validationContext);
            _tableAccessDesc = mfNode.ValidateAggregationTableRead(validationContext, _tableAccessColumn, _table);
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (_optionalStreamName != null) {
                writer.Write(_optionalStreamName);
                writer.Write(".");
            }

            writer.Write(_tableAccessColumn.ColumnName);
            writer.Write(".");
            _aggregateAccessMultiValueNode.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="streamNum">stream number</param>
        /// <param name="reader">reader</param>
        /// <param name="aggColNum">agg col</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new-data flag</param>
        /// <param name="exprEvaluatorContext">expr ctx</param>
        /// <returns>value</returns>
        public static object EvaluateTableWithReader(
            int streamNum,
            AggregationMultiFunctionTableReader reader,
            int aggColNum,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            var row = ExprTableEvalStrategyUtil.GetRow((ObjectArrayBackedEventBean) @event);
            return reader.GetValue(aggColNum, row, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="streamNum">stream number</param>
        /// <param name="reader">reader</param>
        /// <param name="aggColNum">agg col</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new-data flag</param>
        /// <param name="exprEvaluatorContext">expr ctx</param>
        /// <returns>value</returns>
        public static ICollection<EventBean> EvaluateTableWithReaderCollectionEvents(
            int streamNum,
            AggregationMultiFunctionTableReader reader,
            int aggColNum,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            var row = ExprTableEvalStrategyUtil.GetRow((ObjectArrayBackedEventBean) @event);
            return reader.GetValueCollectionEvents(
                aggColNum, row, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="streamNum">stream number</param>
        /// <param name="reader">reader</param>
        /// <param name="aggColNum">agg col</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new-data flag</param>
        /// <param name="exprEvaluatorContext">expr ctx</param>
        /// <returns>value</returns>
        public static ICollection<object> EvaluateTableWithReaderCollectionScalar(
            int streamNum,
            AggregationMultiFunctionTableReader reader,
            int aggColNum,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            var row = ExprTableEvalStrategyUtil.GetRow((ObjectArrayBackedEventBean) @event);
            return reader.GetValueCollectionScalar(aggColNum, row, eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
} // end of namespace