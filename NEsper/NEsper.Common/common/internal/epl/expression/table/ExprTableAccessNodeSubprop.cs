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
        [NonSerialized] private EPType optionalEnumerationType;
        [NonSerialized] private ExprEnumerationGivenEventForge optionalPropertyEnumEvaluator;
        private Type _evaluationType;

        public ExprTableAccessNodeSubprop(
            string tableName,
            string subpropName)
            : base(tableName)
        {
            SubpropName = subpropName;
        }

        public override ExprForge Forge => this;

        protected override string InstrumentationQName => "ExprTableSubproperty";

        protected override CodegenExpression[] InstrumentationQParams =>
            new[] {Constant(tableMeta.TableName), Constant(SubpropName)};

        public string SubpropName { get; }

        public ExprEnumerationGivenEventForge OptionalPropertyEnumEvaluator => optionalPropertyEnumEvaluator;

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

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return EPTypeHelper.OptionalIsEventTypeColl(optionalEnumerationType);
        }

        public Type ComponentTypeCollection => EPTypeHelper.OptionalIsComponentTypeColl(optionalEnumerationType);

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return EPTypeHelper.OptionalIsEventTypeSingle(optionalEnumerationType);
        }

        public override ExprEvaluator ExprEvaluator => this;

        public override Type EvaluationType {
            get => _evaluationType;
        }

        public override ExprTableEvalStrategyFactoryForge TableAccessFactoryForge {
            get {
                var column = tableMeta.Columns.Get(SubpropName);
                var ungrouped = !tableMeta.IsKeyed;
                var forge = new ExprTableEvalStrategyFactoryForge(tableMeta, groupKeyEvaluators);

                if (column is TableMetadataColumnPlain) {
                    var plain = (TableMetadataColumnPlain) column;
                    forge.PropertyIndex = plain.IndexPlain;
                    forge.StrategyEnum = ungrouped ? UNGROUPED_PLAINCOL : GROUPED_PLAINCOL;
                    forge.OptionalEnumEval = optionalPropertyEnumEvaluator;
                }
                else {
                    var aggcol = (TableMetadataColumnAggregation) column;
                    forge.AggColumnNum = aggcol.Column;
                    forge.StrategyEnum = ungrouped ? UNGROUPED_AGG_SIMPLE : GROUPED_AGG_SIMPLE;
                }

                return forge;
            }
        }

        protected override void ValidateBindingInternal(ExprValidationContext validationContext)
        {
            ValidateGroupKeys(tableMeta, validationContext);
            var column = ValidateSubpropertyGetCol(tableMeta, SubpropName);
            _evaluationType = tableMeta.PublicEventType.GetPropertyType(SubpropName);
            if (column is TableMetadataColumnPlain) {
                ExprDotEnumerationSourceForgeForProps enumerationSource =
                    ExprDotNodeUtility.GetPropertyEnumerationSource(
                        SubpropName, 0, tableMeta.InternalEventType, true, true);
                optionalEnumerationType = enumerationSource.ReturnType;
                optionalPropertyEnumEvaluator = enumerationSource.EnumerationGivenEvent;
            }
            else {
                var aggcol = (TableMetadataColumnAggregation) column;
                optionalEnumerationType = aggcol.OptionalEnumerationType;
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ToPrecedenceFreeEPLInternal(writer, SubpropName);
        }

        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            var that = (ExprTableAccessNodeSubprop) other;
            return SubpropName.Equals(that.SubpropName);
        }
    }
} // end of namespace