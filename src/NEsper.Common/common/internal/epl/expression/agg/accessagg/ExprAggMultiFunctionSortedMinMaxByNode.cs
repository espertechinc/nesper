///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.access.sorted;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
    public class ExprAggMultiFunctionSortedMinMaxByNode : ExprAggregateNodeBase,
        ExprEnumerationForge,
        ExprAggMultiFunctionNode
    {
        private readonly bool max;
        private readonly bool ever;
        private readonly bool sortedwin;

        private EventType containedType;
        private AggregationForgeFactory aggregationForgeFactory;

        public ExprAggMultiFunctionSortedMinMaxByNode(
            bool max,
            bool ever,
            bool sortedwin) : base(false)
        {
            this.max = max;
            this.ever = ever;
            this.sortedwin = sortedwin;
        }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            AggregationForgeFactoryAccessSorted factory;

            if (validationContext.StatementRawInfo.StatementType == StatementType.CREATE_TABLE) {
                // handle create-table statements (state creator and default accessor, limited to certain options)
                factory = HandleCreateTable(validationContext);
            }
            else if (validationContext.StatementRawInfo.IntoTableName != null) {
                // handle into-table (state provided, accessor and agent needed, validation done by factory)
                factory = HandleIntoTable(validationContext);
            }
            else {
                // handle standalone
                factory = HandleNonTable(validationContext);
            }

            containedType = factory.ContainedEventType;
            aggregationForgeFactory = factory;
            return factory;
        }

        private AggregationForgeFactoryAccessSorted HandleNonTable(ExprValidationContext validationContext)
        {
            if (positionalParams.Length == 0) {
                throw new ExprValidationException("Missing the sort criteria expression");
            }

            // validate that the streams referenced in the criteria are a single stream's
            var streams = ExprNodeUtilityQuery.GetIdentStreamNumbers(positionalParams[0]);
            if (streams.Count > 1 || streams.IsEmpty()) {
                throw new ExprValidationException(
                    ErrorPrefix + " requires that any parameter expressions evaluate properties of the same stream");
            }

            var streamNum = streams.First();

            // validate that there is a remove stream, use "ever" if not
            if (!ever &&
                ExprAggMultiFunctionLinearAccessNode.GetIstreamOnly(validationContext.StreamTypeService, streamNum)) {
                if (sortedwin) {
                    throw new ExprValidationException(
                        ErrorPrefix + " requires that a data window is declared for the stream");
                }
            }

            // determine typing and evaluation
            containedType = validationContext.StreamTypeService.EventTypes[streamNum];

            var componentType = containedType.UnderlyingType;
            var accessorResultType = componentType;
            AggregationAccessorForge accessor;
            var tableMetadata =
                validationContext.TableCompileTimeResolver.ResolveTableFromEventType(containedType);
            if (!sortedwin) {
                if (tableMetadata != null) {
                    accessor = new AggregationAccessorMinMaxByTable(max, tableMetadata);
                }
                else {
                    accessor = new AggregationAccessorMinMaxByNonTable(max);
                }
            }
            else {
                if (tableMetadata != null) {
                    accessor = new AggregationAccessorSortedTable(max, componentType, tableMetadata);
                }
                else {
                    accessor = new AggregationAccessorSortedNonTable(max, componentType);
                }

                accessorResultType = TypeHelper.GetArrayType(accessorResultType);
            }

            var criteriaExpressions = CriteriaExpressions;

            AggregationStateTypeWStream type;
            if (ever) {
                type = max ? AggregationStateTypeWStream.MAXEVER : AggregationStateTypeWStream.MINEVER;
            }
            else {
                type = AggregationStateTypeWStream.SORTED;
            }

            var stateKey = new AggregationStateKeyWStream(
                streamNum,
                containedType,
                type,
                criteriaExpressions.First,
                optionalFilter);

            var optionalFilterForge = optionalFilter?.Forge;
            var streamEventType = validationContext.StreamTypeService.EventTypes[streamNum];
            var criteriaTypes = ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions.First);
            var criteriaSerdes = new DataInputOutputSerdeForge[criteriaTypes.Length];
            for (var i = 0; i < criteriaTypes.Length; i++) {
                criteriaSerdes[i] = validationContext.SerdeResolver.SerdeForAggregation(
                    criteriaTypes[i],
                    validationContext.StatementRawInfo);
            }

            var sortedDesc = new
                SortedAggregationStateDesc(
                    max,
                    validationContext.ImportService,
                    criteriaExpressions.First,
                    criteriaTypes,
                    criteriaSerdes,
                    criteriaExpressions.Second,
                    ever,
                    streamNum,
                    this,
                    optionalFilterForge,
                    streamEventType);

            var serdeForgeables = SerdeEventTypeUtility.Plan(
                containedType,
                validationContext.StatementRawInfo,
                validationContext.SerdeEventTypeRegistry,
                validationContext.SerdeResolver,
                validationContext.StateMgmtSettingsProvider);
            validationContext.AdditionalForgeables.AddAll(serdeForgeables);

            return new AggregationForgeFactoryAccessSorted(
                this,
                accessor,
                accessorResultType,
                containedType,
                stateKey,
                sortedDesc,
                AggregationAgentDefault.INSTANCE);
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var future = GetAggFuture(codegenClassScope);
            return ExprDotMethod(
                future,
                "GetCollectionOfEvents",
                Constant(column),
                exprSymbol.GetAddEPS(parent),
                exprSymbol.GetAddIsNewData(parent),
                exprSymbol.GetAddExprEvalCtx(parent));
        }

        private AggregationForgeFactoryAccessSorted HandleIntoTable(ExprValidationContext validationContext)
        {
            int streamNum;
            if (positionalParams.Length == 0 ||
                (positionalParams.Length == 1 && positionalParams[0] is ExprWildcard)) {
                ExprAggMultiFunctionUtil.ValidateWildcardStreamNumbers(
                    validationContext.StreamTypeService,
                    AggregationFunctionName);
                streamNum = 0;
            }
            else if (positionalParams.Length == 1 && positionalParams[0] is ExprStreamUnderlyingNode) {
                streamNum = ExprAggMultiFunctionUtil.ValidateStreamWildcardGetStreamNum(positionalParams[0]);
            }
            else if (positionalParams.Length > 0) {
                throw new ExprValidationException("When specifying into-table a sort expression cannot be provided");
            }
            else {
                streamNum = 0;
            }

            var containedType = validationContext.StreamTypeService.EventTypes[streamNum];
            var componentType = containedType.UnderlyingType;
            var accessorResultType = componentType;
            AggregationAccessorForge accessor;
            if (!sortedwin) {
                accessor = new AggregationAccessorMinMaxByNonTable(max);
            }
            else {
                accessor = new AggregationAccessorSortedNonTable(max, componentType);
                accessorResultType = TypeHelper.GetArrayType(accessorResultType);
            }

            var agent = AggregationAgentForgeFactory.Make(
                streamNum,
                optionalFilter,
                validationContext.ImportService,
                validationContext.StreamTypeService.IsOnDemandStreams,
                validationContext.StatementName);
            return new AggregationForgeFactoryAccessSorted(
                this,
                accessor,
                accessorResultType,
                containedType,
                null,
                null,
                agent);
        }

        private AggregationForgeFactoryAccessSorted HandleCreateTable(ExprValidationContext validationContext)
        {
            if (positionalParams.Length == 0) {
                throw new ExprValidationException("Missing the sort criteria expression");
            }

            var message = "For tables columns, the aggregation function requires the 'sorted(*)' declaration";
            if (!sortedwin && !ever) {
                throw new ExprValidationException(message);
            }

            if (validationContext.StreamTypeService.StreamNames.Length == 0) {
                throw new ExprValidationException("'Sorted' requires that the event type is provided");
            }

            var containedType = validationContext.StreamTypeService.EventTypes[0];
            var componentType = containedType.UnderlyingType;
            var criteriaExpressions = CriteriaExpressions;
            var accessorResultType = componentType;
            AggregationAccessorForge accessor;
            if (!sortedwin) {
                accessor = new AggregationAccessorMinMaxByNonTable(max);
            }
            else {
                accessor = new AggregationAccessorSortedNonTable(max, componentType);
                accessorResultType = TypeHelper.GetArrayType(accessorResultType);
            }

            var criteriaTypes = ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions.First);
            var criteriaSerdes = new DataInputOutputSerdeForge[criteriaTypes.Length];
            for (var i = 0; i < criteriaTypes.Length; i++) {
                criteriaSerdes[i] = validationContext.SerdeResolver.SerdeForAggregation(
                    criteriaTypes[i],
                    validationContext.StatementRawInfo);
            }

            var stateDesc = new SortedAggregationStateDesc(
                max,
                validationContext.ImportService,
                criteriaExpressions.First,
                criteriaTypes,
                criteriaSerdes,
                criteriaExpressions.Second,
                ever,
                0,
                this,
                null,
                containedType);

            var serdeForgeables = SerdeEventTypeUtility.Plan(
                containedType,
                validationContext.StatementRawInfo,
                validationContext.SerdeEventTypeRegistry,
                validationContext.SerdeResolver,
                validationContext.StateMgmtSettingsProvider);
            validationContext.AdditionalForgeables.AddAll(serdeForgeables);

            return new AggregationForgeFactoryAccessSorted(
                this,
                accessor,
                accessorResultType,
                containedType,
                null,
                stateDesc,
                null);
        }

        private Pair<ExprNode[], bool[]> CriteriaExpressions {
            get {
                // determine ordering ascending/descending and build criteria expression without "asc" marker
                var criteriaExpressions = new ExprNode[positionalParams.Length];
                var sortDescending = new bool[positionalParams.Length];
                for (var i = 0; i < positionalParams.Length; i++) {
                    var parameter = positionalParams[i];
                    criteriaExpressions[i] = parameter;
                    if (parameter is ExprOrderedExpr ordered) {
                        sortDescending[i] = ordered.IsDescending;
                        if (!ordered.IsDescending) {
                            criteriaExpressions[i] = ordered.ChildNodes[0];
                        }
                    }
                }

                return new Pair<ExprNode[], bool[]>(criteriaExpressions, sortDescending);
            }
        }

        public override string AggregationFunctionName {
            get {
                if (sortedwin) {
                    return "sorted";
                }

                if (ever) {
                    return max ? "maxbyever" : "minbyever";
                }

                return max ? "maxby" : "minby";
            }
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(AggregationFunctionName);
            ExprNodeUtilityPrint.ToExpressionStringParams(writer, positionalParams);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (!sortedwin) {
                return null;
            }

            return containedType;
        }

        public Type ComponentTypeCollection => null;

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (sortedwin) {
                return null;
            }

            return containedType;
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var future = GetAggFuture(codegenClassScope);
            return ExprDotMethod(
                future,
                "GetEventBean",
                Constant(column),
                exprSymbol.GetAddEPS(parent),
                exprSymbol.GetAddIsNewData(parent),
                exprSymbol.GetAddExprEvalCtx(parent));
        }

        public bool IsMax => max;

        public override bool IsFilterExpressionAsLastParameter => false;

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprAggMultiFunctionSortedMinMaxByNode other)) {
                return false;
            }

            return max == other.max &&
                   containedType == other.containedType &&
                   sortedwin == other.sortedwin &&
                   ever == other.ever;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public AggregationForgeFactory AggregationForgeFactory => aggregationForgeFactory;

        private string ErrorPrefix => $"The '{AggregationFunctionName}' aggregation function";
    }
} // end of namespace