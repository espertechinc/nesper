///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.table.strategy.ExprTableEvalStrategyEnum;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public class ExprTableAccessNodeSubprop : ExprTableAccessNode,
        ExprEvaluator,
        ExprEnumerationForge,
        ExprEnumerationEval,
        ExprForge
    {
        private readonly string subpropName;
        private Type bindingReturnType;
        [NonSerialized]
        private EPChainableType optionalEnumerationType;
        [NonSerialized]
        private ExprEnumerationGivenEventForge optionalPropertyEnumEvaluator;

        public ExprTableAccessNodeSubprop(
            string tableName,
            string subpropName) : base(tableName)
        {
            this.subpropName = subpropName;
        }

        public override ExprTableEvalStrategyFactoryForge TableAccessFactoryForge {
            get {
                var tableMeta = TableMeta;
                var column = tableMeta.Columns.Get(subpropName);
                var ungrouped = !tableMeta.IsKeyed;
                var forge = new ExprTableEvalStrategyFactoryForge(tableMeta, GroupKeyEvaluators);
                if (column is TableMetadataColumnPlain plain) {
                    forge.PropertyIndex = plain.IndexPlain;
                    forge.StrategyEnum = ungrouped ? UNGROUPED_PLAINCOL : GROUPED_PLAINCOL;
                    forge.OptionalEnumEval = optionalPropertyEnumEvaluator;
                }
                else {
                    var aggcol = (TableMetadataColumnAggregation)column;
                    forge.AggColumnNum = aggcol.Column;
                    forge.StrategyEnum = ungrouped ? UNGROUPED_AGG_SIMPLE : GROUPED_AGG_SIMPLE;
                }

                return forge;
            }
        }

        protected override string InstrumentationQName => "ExprTableSubproperty";

        protected override CodegenExpression[] InstrumentationQParams {
            get {
                return new CodegenExpression[] {
                    Constant(TableMeta.TableName),
                    Constant(subpropName)
                };
            }
        }

        protected override void ValidateBindingInternal(ExprValidationContext validationContext)
        {
            var tableMeta = TableMeta;
            ValidateGroupKeys(tableMeta, validationContext);
            var column = ValidateSubpropertyGetCol(tableMeta, subpropName);
            var propType = tableMeta.PublicEventType.GetPropertyType(subpropName);
            bindingReturnType = propType;
            if (column is TableMetadataColumnPlain) {
                var enumerationSource = ExprDotNodeUtility.GetPropertyEnumerationSource(
                    subpropName,
                    0,
                    tableMeta.InternalEventType,
                    true,
                    true);
                optionalEnumerationType = enumerationSource.ReturnType;
                optionalPropertyEnumEvaluator = enumerationSource.EnumerationGivenEvent;
            }
            else {
                var aggcol = (TableMetadataColumnAggregation)column;
                optionalEnumerationType = aggcol.OptionalEnumerationType;
            }
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ToPrecedenceFreeEPLInternal(writer, subpropName, flags);
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return optionalEnumerationType.OptionalIsEventTypeColl();
        }

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

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return optionalEnumerationType.OptionalIsEventTypeSingle();
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            var that = (ExprTableAccessNodeSubprop)other;
            return subpropName.Equals(that.subpropName);
        }

        public override ExprEvaluator ExprEvaluator => this;

        public override Type EvaluationType => bindingReturnType;

        public override ExprForge Forge => this;

        [JsonIgnore]
        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        [JsonIgnore]
        public string SubpropName => subpropName;

        [JsonIgnore]
        public Type ComponentTypeCollection =>
            EPChainableTypeHelper.GetCollectionOrArrayComponentTypeOrNull(optionalEnumerationType);
    }
} // end of namespace