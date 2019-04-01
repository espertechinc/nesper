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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public class ExprTableAccessNodeSubpropAccessor : ExprTableAccessNode,
        ExprEvaluator,
        ExprEnumerationForge,
        ExprEnumerationEval,
        ExprForge
    {
        private readonly ExprNode aggregateAccessMultiValueNode;

        private AggregationTableReadDesc tableAccessDesc;

        public ExprTableAccessNodeSubpropAccessor(
            string tableName, string subpropName, ExprNode aggregateAccessMultiValueNode) : base(tableName)
        {
            SubpropName = subpropName;
            this.aggregateAccessMultiValueNode = aggregateAccessMultiValueNode;
        }

        public ExprAggregateNodeBase AggregateAccessMultiValueNode =>
            (ExprAggregateNodeBase) aggregateAccessMultiValueNode;

        public override ExprForge Forge => this;

        protected override string InstrumentationQName => "ExprTableSubpropAccessor";

        protected override CodegenExpression[] InstrumentationQParams => new[] {
            Constant(tableMeta.TableName), Constant(SubpropName),
            Constant(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(aggregateAccessMultiValueNode))
        };

        public string SubpropName { get; }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices)
        {
            return tableAccessDesc.EventTypeCollection;
        }

        public Type ComponentTypeCollection => tableAccessDesc.ComponentTypeCollection;

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices)
        {
            return tableAccessDesc.EventTypeSingle;
        }

        public override ExprEvaluator ExprEvaluator => this;

        public override Type EvaluationType => tableAccessDesc.Reader.ResultType;

        protected override void ValidateBindingInternal(ExprValidationContext validationContext)
        {
            // validate group keys
            ValidateGroupKeys(tableMeta, validationContext);
            var column = (TableMetadataColumnAggregation) ValidateSubpropertyGetCol(tableMeta, SubpropName);

            // validate accessor factory i.e. the parameters types and the match to the required state
            if (column.IsMethodAgg) {
                throw new ExprValidationException("Invalid combination of aggregation state and aggregation accessor");
            }

            var mfNode = (ExprAggMultiFunctionNode) aggregateAccessMultiValueNode;
            mfNode.ValidatePositionals(validationContext);
            tableAccessDesc = mfNode.ValidateAggregationTableRead(validationContext, column, tableMeta);
        }

        public override ExprTableEvalStrategyFactoryForge TableAccessFactoryForge {
            get {
                var forge = new ExprTableEvalStrategyFactoryForge(tableMeta, groupKeyEvaluators);
                var column = (TableMetadataColumnAggregation) tableMeta.Columns.Get(SubpropName);
                forge.AggColumnNum = column.Column;
                var ungrouped = !tableMeta.IsKeyed;
                forge.StrategyEnum = ungrouped
                    ? ExprTableEvalStrategyEnum.UNGROUPED_AGG_ACCESSREAD
                    : ExprTableEvalStrategyEnum.GROUPED_AGG_ACCESSREAD;
                forge.AccessAggStrategy = tableAccessDesc.Reader;
                return forge;
            }
        }

        public override void ToPrecedenceFreeEPL(StringWriter writer)
        {
            ToPrecedenceFreeEPLInternal(writer, SubpropName);
            writer.Write(".");
            aggregateAccessMultiValueNode.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
        }

        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            var that = (ExprTableAccessNodeSubpropAccessor) other;
            if (!SubpropName.Equals(that.SubpropName)) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEquals(
                aggregateAccessMultiValueNode, that.aggregateAccessMultiValueNode, false);
        }
    }
} // end of namespace