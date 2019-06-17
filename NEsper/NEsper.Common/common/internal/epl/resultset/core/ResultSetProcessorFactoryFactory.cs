///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.prev;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.agggrouped;
using com.espertech.esper.common.@internal.epl.resultset.handthru;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.common.@internal.epl.resultset.rowperevent;
using com.espertech.esper.common.@internal.epl.resultset.rowpergroup;
using com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.epl.resultset.simple;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    /// <summary>
    ///     Factory for output processors. Output processors process the result set of a join or of a view
    ///     and apply aggregation/grouping, having and some output limiting logic.
    ///     <para />
    ///     The instance produced by the factory depends on the presence of aggregation functions in the select list,
    ///     the presence and nature of the group-by clause.
    ///     <para />
    ///     In case (1) and (2) there are no aggregation functions in the select clause.
    ///     <para />
    ///     Case (3) is without group-by and with aggregation functions and without non-aggregated properties
    ///     in the select list: <pre>select sum(volume) </pre>.
    ///     Always produces one row for new and old data, aggregates without grouping.
    ///     <para />
    ///     Case (4) is without group-by and with aggregation functions but with non-aggregated properties
    ///     in the select list: <pre>select price, sum(volume) </pre>.
    ///     Produces a row for each event, aggregates without grouping.
    ///     <para />
    ///     Case (5) is with group-by and with aggregation functions and all selected properties are grouped-by.
    ///     in the select list: <pre>select customerId, sum(volume) group by customerId</pre>.
    ///     Produces a old and new data row for each group changed, aggregates with grouping.
    ///     <para />
    ///     Case (6) is with group-by and with aggregation functions and only some selected properties are grouped-by.
    ///     in the select list: <pre>select customerId, supplierId, sum(volume) group by customerId</pre>.
    ///     Produces row for each event, aggregates with grouping.
    /// </summary>
    public class ResultSetProcessorFactoryFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static ResultSetProcessorDesc GetProcessorPrototype(
            ResultSetSpec spec,
            StreamTypeService typeService,
            ViewResourceDelegateExpr viewResourceDelegate,
            bool[] isUnidirectionalStream,
            bool allowAggregation,
            ContextPropertyRegistry contextPropertyRegistry,
            bool isFireAndForget,
            bool isOnSelect,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var orderByListUnexpanded = spec.OrderByList;
            var selectClauseSpec = spec.SelectClauseSpec;
            var insertIntoDesc = spec.InsertIntoDesc;
            var optionalHavingNode = spec.HavingClause;
            var outputLimitSpec = spec.OptionalOutputLimitSpec;
            var groupByClauseExpressions = spec.GroupByClauseExpressions;
            IList<ExprDeclaredNode> declaredNodes = new List<ExprDeclaredNode>();

            // validate output limit spec
            ValidateOutputLimit(outputLimitSpec, statementRawInfo, services);

            // determine unidirectional
            var isUnidirectional = false;
            for (var i = 0; i < isUnidirectionalStream.Length; i++) {
                isUnidirectional |= isUnidirectionalStream[i];
            }

            // determine single-stream historical
            var isHistoricalOnly = false;
            if (spec.StreamSpecs.Length == 1) {
                var streamSpec = spec.StreamSpecs[0];
                if (streamSpec is DBStatementStreamSpec || streamSpec is MethodStreamSpec ||
                    streamSpec is TableQueryStreamSpec) {
                    isHistoricalOnly = true;
                }
            }

            // determine join or number of streams
            var numStreams = typeService.EventTypes.Length;
            var join = numStreams > 1;

            // Expand any instances of select-clause names in the
            // order-by clause with the full expression
            var orderByList = ExpandColumnNames(selectClauseSpec.SelectExprList, orderByListUnexpanded);

            // Validate selection expressions, if any (could be wildcard i.e. empty list)
            IList<SelectClauseExprCompiledSpec> namedSelectionList = new List<SelectClauseExprCompiledSpec>();
            var allowRollup = groupByClauseExpressions != null && groupByClauseExpressions.GroupByRollupLevels != null;
            var resettableAggs = isUnidirectional || statementRawInfo.StatementType.IsOnTriggerInfra();
            var intoTableName = spec.IntoTableSpec == null ? null : spec.IntoTableSpec.Name;
            var validationContext = new ExprValidationContextBuilder(typeService, statementRawInfo, services)
                .WithViewResourceDelegate(viewResourceDelegate).WithAllowRollupFunctions(allowRollup)
                .WithAllowBindingConsumption(true)
                .WithIsResettingAggregations(resettableAggs).WithIntoTableName(intoTableName).Build();

            ValidateSelectAssignColNames(selectClauseSpec, namedSelectionList, validationContext);
            if (spec.GroupByClauseExpressions != null && spec.GroupByClauseExpressions.SelectClausePerLevel != null) {
                ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.GROUPBY, spec.GroupByClauseExpressions.SelectClausePerLevel, validationContext);
            }

            var isUsingWildcard = selectClauseSpec.IsUsingWildcard;

            // Validate stream selections, if any (such as stream.*)
            var isUsingStreamSelect = false;
            foreach (var compiled in selectClauseSpec.SelectExprList) {
                if (!(compiled is SelectClauseStreamCompiledSpec)) {
                    continue;
                }

                var streamSelectSpec = (SelectClauseStreamCompiledSpec) compiled;
                var streamNum = int.MinValue;
                var isFragmentEvent = false;
                var isProperty = false;
                Type propertyType = null;
                isUsingStreamSelect = true;
                for (var i = 0; i < typeService.StreamNames.Length; i++) {
                    var streamName = streamSelectSpec.StreamName;
                    if (typeService.StreamNames[i].Equals(streamName)) {
                        streamNum = i;
                        break;
                    }

                    // see if the stream name is known as a nested event type
                    var candidateProviderOfFragments = typeService.EventTypes[i];
                    // for the native event type we don't need to fragment, we simply use the property itself since all wrappers understand objects
                    if (!(candidateProviderOfFragments is NativeEventType) &&
                        candidateProviderOfFragments.GetFragmentType(streamName) != null) {
                        streamNum = i;
                        isFragmentEvent = true;
                        break;
                    }
                }

                // stream name not found
                if (streamNum == int.MinValue) {
                    // see if the stream name specified resolves as a property
                    PropertyResolutionDescriptor desc = null;
                    try {
                        desc = typeService.ResolveByPropertyName(streamSelectSpec.StreamName, false);
                    }
                    catch (StreamTypesException) {
                        // not handled
                    }

                    if (desc == null) {
                        throw new ExprValidationException(
                            "Stream selector '" + streamSelectSpec.StreamName +
                            ".*' does not match any stream name in the from clause");
                    }

                    isProperty = true;
                    propertyType = desc.PropertyType;
                    streamNum = desc.StreamNum;
                }

                streamSelectSpec.StreamNumber = streamNum;
                streamSelectSpec.IsFragmentEvent = isFragmentEvent;
                streamSelectSpec.SetProperty(isProperty, propertyType);

                if (streamNum >= 0) {
                    var table = services.TableCompileTimeResolver.ResolveTableFromEventType(
                        typeService.EventTypes[streamNum]);
                    streamSelectSpec.TableMetadata = table;
                }
            }

            // Validate having clause, if present
            if (optionalHavingNode != null) {
                optionalHavingNode = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.HAVING, optionalHavingNode, validationContext);
                if (spec.GroupByClauseExpressions != null) {
                    ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.GROUPBY, spec.GroupByClauseExpressions.OptHavingNodePerLevel, validationContext);
                }
            }

            // Validate order-by expressions, if any (could be empty list for no order-by)
            for (var i = 0; i < orderByList.Count; i++) {
                var orderByNode = orderByList[i].ExprNode;

                // Ensure there is no subselects
                var visitor = new ExprNodeSubselectDeclaredDotVisitor();
                orderByNode.Accept(visitor);
                if (visitor.Subselects.Count > 0) {
                    throw new ExprValidationException("Subselects not allowed within order-by clause");
                }

                var isDescending = orderByList[i].IsDescending;
                var validatedOrderBy = new OrderByItem(
                    ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.ORDERBY, orderByNode, validationContext),
                    isDescending);
                orderByList[i] = validatedOrderBy;

                if (spec.GroupByClauseExpressions != null && spec.GroupByClauseExpressions.OptOrderByPerLevel != null) {
                    ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.GROUPBY, spec.GroupByClauseExpressions.OptOrderByPerLevel, validationContext);
                }
            }

            // Get the select expression nodes
            IList<ExprNode> selectNodes = new List<ExprNode>();
            foreach (var element in namedSelectionList) {
                selectNodes.Add(element.SelectExpression);
            }

            // Get the order-by expression nodes
            IList<ExprNode> orderByNodes = new List<ExprNode>();
            foreach (var element in orderByList) {
                orderByNodes.Add(element.ExprNode);
            }

            // Determine aggregate functions used in select, if any
            IList<ExprAggregateNode> selectAggregateExprNodes = new List<ExprAggregateNode>();
            IDictionary<ExprNode, string> selectAggregationNodesNamed = new Dictionary<ExprNode, string>();
            var declaredNodeVisitor = new ExprNodeDeclaredVisitor();
            foreach (var element in namedSelectionList) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(element.SelectExpression, selectAggregateExprNodes);
                if (element.ProvidedName != null) {
                    selectAggregationNodesNamed.Put(element.SelectExpression, element.ProvidedName);
                }

                element.SelectExpression.Accept(declaredNodeVisitor);
                declaredNodes.AddAll(declaredNodeVisitor.DeclaredExpressions);
                declaredNodeVisitor.Clear();
            }

            if (spec.GroupByClauseExpressions != null) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(
                    spec.GroupByClauseExpressions.SelectClausePerLevel, selectAggregateExprNodes);
            }

            if (!allowAggregation && !selectAggregateExprNodes.IsEmpty()) {
                throw new ExprValidationException("Aggregation functions are not allowed in this context");
            }

            // Determine if we have a having clause with aggregation
            IList<ExprAggregateNode> havingAggregateExprNodes = new List<ExprAggregateNode>();
            var propertiesAggregatedHaving = new ExprNodePropOrStreamSet();
            if (optionalHavingNode != null) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(optionalHavingNode, havingAggregateExprNodes);
                if (groupByClauseExpressions != null) {
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(
                        groupByClauseExpressions.OptHavingNodePerLevel, havingAggregateExprNodes);
                }

                propertiesAggregatedHaving =
                    ExprNodeUtilityAggregation.GetAggregatedProperties(havingAggregateExprNodes);
            }

            if (!allowAggregation && !havingAggregateExprNodes.IsEmpty()) {
                throw new ExprValidationException("Aggregation functions are not allowed in this context");
            }

            // Determine if we have a order-by clause with aggregation
            IList<ExprAggregateNode> orderByAggregateExprNodes = new List<ExprAggregateNode>();
            if (orderByNodes != null && !orderByNodes.IsEmpty()) {
                foreach (var orderByNode in orderByNodes) {
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(orderByNode, orderByAggregateExprNodes);
                }

                if (groupByClauseExpressions != null) {
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(
                        groupByClauseExpressions.OptOrderByPerLevel, orderByAggregateExprNodes);
                }

                if (!allowAggregation && !orderByAggregateExprNodes.IsEmpty()) {
                    throw new ExprValidationException("Aggregation functions are not allowed in this context");
                }
            }

            // Analyze rollup
            var groupByRollupInfo = AnalyzeValidateGroupBy(groupByClauseExpressions, validationContext);
            var groupByNodesValidated = groupByRollupInfo == null
                ? ExprNodeUtilityQuery.EMPTY_EXPR_ARRAY
                : groupByRollupInfo.ExprNodes;
            var groupByRollupDesc =
                groupByRollupInfo == null ? null : groupByRollupInfo.RollupDesc;

            // Construct the appropriate aggregation service
            var hasGroupBy = groupByNodesValidated.Length > 0;
            var aggregationServiceForgeDesc = AggregationServiceFactoryFactory.GetService(
                selectAggregateExprNodes, selectAggregationNodesNamed, declaredNodes, groupByNodesValidated,
                havingAggregateExprNodes, orderByAggregateExprNodes,
                Collections.GetEmptyList<ExprAggregateNodeGroupKey>(), hasGroupBy,
                statementRawInfo.Annotations, services.VariableCompileTimeResolver, false,
                spec.WhereClause, spec.HavingClause,
                typeService.EventTypes, groupByRollupDesc,
                spec.ContextName, spec.IntoTableSpec, services.TableCompileTimeResolver,
                isUnidirectional, isFireAndForget, isOnSelect,
                services.ImportServiceCompileTime, statementRawInfo.StatementName);

            // Compare local-aggregation versus group-by
            var localGroupByMatchesGroupBy = AnalyzeLocalGroupBy(
                groupByNodesValidated, selectAggregateExprNodes, havingAggregateExprNodes, orderByAggregateExprNodes);

            // Construct the processor for evaluating the select clause
            var args = new SelectProcessorArgs(
                selectClauseSpec.SelectExprList, groupByRollupInfo, isUsingWildcard, null, spec.ForClauseSpec,
                typeService,
                null, isFireAndForget, spec.Annotations, statementRawInfo, services);
            var selectExprProcessorDesc = SelectExprProcessorFactory.GetProcessor(args, insertIntoDesc, true);
            var selectExprProcessorForge = selectExprProcessorDesc.Forge;
            var selectSubscriberDescriptor = selectExprProcessorDesc.SubscriberDescriptor;
            var resultEventType = selectExprProcessorForge.ResultEventType;

            // compute rollup if applicable
            GroupByRollupPerLevelForge rollupPerLevelForges = null;
            if (groupByRollupDesc != null) {
                rollupPerLevelForges = GetRollUpPerLevelExpressions(
                    spec, groupByNodesValidated, groupByRollupDesc, groupByRollupInfo, insertIntoDesc, typeService,
                    validationContext, isFireAndForget, statementRawInfo, services);
            }

            // Construct the processor for sorting output events
            var orderByProcessorFactory = OrderByProcessorFactoryFactory.GetProcessor(
                namedSelectionList,
                orderByList, spec.RowLimitSpec, services.VariableCompileTimeResolver,
                services.Configuration.Compiler.Language.IsSortUsingCollator,
                spec.ContextName, rollupPerLevelForges == null ? null : rollupPerLevelForges.OptionalOrderByElements);
            var hasOrderBy = orderByProcessorFactory != null;

            // Get a list of event properties being aggregated in the select clause, if any
            var propertiesGroupBy =
                ExprNodeUtilityAggregation.GetGroupByPropertiesValidateHasOne(groupByNodesValidated);
            // Figure out all non-aggregated event properties in the select clause (props not under a sum/avg/max aggregation node)
            var nonAggregatedPropsSelect = ExprNodeUtilityAggregation.GetNonAggregatedProps(
                typeService.EventTypes, selectNodes, contextPropertyRegistry);
            if (optionalHavingNode != null) {
                ExprNodeUtilityAggregation.AddNonAggregatedProps(
                    optionalHavingNode, nonAggregatedPropsSelect, typeService.EventTypes, contextPropertyRegistry);
            }

            // Validate the having-clause (selected aggregate nodes and all in group-by are allowed)
            var isAggregated = !selectAggregateExprNodes.IsEmpty() || !havingAggregateExprNodes.IsEmpty() ||
                               !orderByAggregateExprNodes.IsEmpty() || !propertiesAggregatedHaving.IsEmpty();
            if (optionalHavingNode != null && isAggregated) {
                ValidateHaving(propertiesGroupBy, optionalHavingNode);
            }

            // We only generate Remove-Stream events if they are explicitly selected, or the insert-into requires them
            var isSelectRStream =
                spec.SelectClauseStreamSelector == SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH ||
                spec.SelectClauseStreamSelector == SelectClauseStreamSelectorEnum.RSTREAM_ONLY;
            if (spec.InsertIntoDesc != null && spec.InsertIntoDesc.StreamSelector.IsSelectsRStream) {
                isSelectRStream = true;
            }

            var optionalHavingForge = optionalHavingNode == null ? null : optionalHavingNode.Forge;
            var hasOutputLimitOpt = ResultSetProcessorOutputConditionTypeExtensions.GetOutputLimitOpt(
                statementRawInfo.Annotations, services.Configuration, hasOrderBy);
            var hasOutputLimitSnapshot =
                outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT;
            var isGrouped = groupByNodesValidated.Length > 0 || groupByRollupDesc != null;
            var outputConditionType = outputLimitSpec != null
                ? (ResultSetProcessorOutputConditionType?) ResultSetProcessorOutputConditionTypeExtensions
                    .GetConditionType(outputLimitSpec.DisplayLimit, isAggregated, hasOrderBy, hasOutputLimitOpt, isGrouped)
                : null;

            // Determine output-first condition factory
            OutputConditionPolledFactoryForge optionalOutputFirstConditionFactoryForge = null;
            if (outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST) {
                optionalOutputFirstConditionFactoryForge =
                    OutputConditionPolledFactoryFactory.CreateConditionFactory(
                        outputLimitSpec, statementRawInfo, services);
            }

            var hasOutputLimit = outputLimitSpec != null;

            // (1)
            // There is no group-by clause and no aggregate functions with event properties in the select clause and having clause (simplest case)
            if (groupByNodesValidated.Length == 0 && selectAggregateExprNodes.IsEmpty() &&
                havingAggregateExprNodes.IsEmpty()) {
                // Determine if any output rate limiting must be performed early while processing results
                // Snapshot output does not count in terms of limiting output for grouping/aggregation purposes
                var isOutputLimitingNoSnapshot = outputLimitSpec != null &&
                                                 outputLimitSpec.DisplayLimit != OutputLimitLimitType.SNAPSHOT;

                // (1a)
                // There is no need to perform select expression processing, the single view itself (no join) generates
                // events in the desired format, therefore there is no output processor. There are no order-by expressions.
                if (orderByNodes.IsEmpty() && optionalHavingNode == null && !isOutputLimitingNoSnapshot &&
                    spec.RowLimitSpec == null) {
                    Log.Debug(".getProcessor Using no result processor");
                    var throughFactoryForge = new ResultSetProcessorHandThroughFactoryForge(
                        resultEventType, selectExprProcessorForge, isSelectRStream);
                    return new ResultSetProcessorDesc(
                        throughFactoryForge,
                        ResultSetProcessorType.HANDTHROUGH,
                        new[] {selectExprProcessorForge},
                        join,
                        hasOutputLimit,
                        outputConditionType.Value,
                        hasOutputLimitSnapshot,
                        resultEventType, false,
                        aggregationServiceForgeDesc,
                        orderByProcessorFactory,
                        selectSubscriberDescriptor);
                }

                // (1b)
                // We need to process the select expression in a simple fashion, with each event (old and new)
                // directly generating one row, and no need to update aggregate state since there is no aggregate function.
                // There might be some order-by expressions.
                var simpleForge = new ResultSetProcessorSimpleForge(
                    resultEventType,
                    selectExprProcessorForge,
                    optionalHavingForge,
                    isSelectRStream,
                    outputLimitSpec,
                    outputConditionType.Value,
                    hasOrderBy,
                    typeService.EventTypes);
                return new ResultSetProcessorDesc(
                    simpleForge, ResultSetProcessorType.UNAGGREGATED_UNGROUPED, new[] {selectExprProcessorForge},
                    join, hasOutputLimit, outputConditionType.Value, hasOutputLimitSnapshot, resultEventType, false,
                    aggregationServiceForgeDesc, orderByProcessorFactory, selectSubscriberDescriptor);
            }

            // (2)
            // A wildcard select-clause has been specified and the group-by is ignored since no aggregation functions are used, and no having clause
            var isLast = outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;
            var isFirst = outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST;
            if (namedSelectionList.IsEmpty() && propertiesAggregatedHaving.IsEmpty() &&
                havingAggregateExprNodes.IsEmpty() && !isLast && !isFirst) {
                var simpleForge = new ResultSetProcessorSimpleForge(
                    resultEventType, selectExprProcessorForge, optionalHavingForge, isSelectRStream, outputLimitSpec,
                    outputConditionType.Value, hasOrderBy, typeService.EventTypes);
                return new ResultSetProcessorDesc(
                    simpleForge, ResultSetProcessorType.UNAGGREGATED_UNGROUPED, new[] {selectExprProcessorForge},
                    join, hasOutputLimit, outputConditionType.Value, hasOutputLimitSnapshot, resultEventType, false,
                    aggregationServiceForgeDesc, orderByProcessorFactory, selectSubscriberDescriptor);
            }

            if (groupByNodesValidated.Length == 0 && isAggregated) {
                // (3)
                // There is no group-by clause and there are aggregate functions with event properties in the select clause (aggregation case)
                // or having class, and all event properties are aggregated (all properties are under aggregation functions).
                var hasStreamSelect = ExprNodeUtilityQuery.HasStreamSelect(selectNodes);
                if (nonAggregatedPropsSelect.IsEmpty() && !hasStreamSelect && !isUsingWildcard &&
                    !isUsingStreamSelect && localGroupByMatchesGroupBy &&
                    (viewResourceDelegate == null || viewResourceDelegate.PreviousRequests.IsEmpty())) {
                    Log.Debug(".getProcessor Using ResultSetProcessorRowForAll");
                    var allForge = new ResultSetProcessorRowForAllForge(
                        resultEventType, selectExprProcessorForge, optionalHavingForge, isSelectRStream,
                        isUnidirectional, isHistoricalOnly, outputLimitSpec, hasOrderBy, outputConditionType.Value);
                    return new ResultSetProcessorDesc(
                        allForge, ResultSetProcessorType.FULLYAGGREGATED_UNGROUPED, new[] {selectExprProcessorForge},
                        join, hasOutputLimit, outputConditionType.Value, hasOutputLimitSnapshot, resultEventType, false,
                        aggregationServiceForgeDesc, orderByProcessorFactory, selectSubscriberDescriptor);
                }

                // (4)
                // There is no group-by clause but there are aggregate functions with event properties in the select clause (aggregation case)
                // or having clause and not all event properties are aggregated (some properties are not under aggregation functions).
                Log.Debug(".getProcessor Using ResultSetProcessorRowPerEventImpl");
                var eventForge = new ResultSetProcessorRowPerEventForge(
                    selectExprProcessorForge.ResultEventType, selectExprProcessorForge, optionalHavingForge,
                    isSelectRStream, isUnidirectional, isHistoricalOnly, outputLimitSpec, outputConditionType.Value,
                    hasOrderBy);
                return new ResultSetProcessorDesc(
                    eventForge, ResultSetProcessorType.AGGREGATED_UNGROUPED, new[] {selectExprProcessorForge},
                    join, hasOutputLimit, outputConditionType.Value, hasOutputLimitSnapshot, resultEventType, false,
                    aggregationServiceForgeDesc, orderByProcessorFactory, selectSubscriberDescriptor);
            }

            // Handle group-by cases
            if (groupByNodesValidated.Length == 0) {
                throw new IllegalStateException("Unexpected empty group-by expression list");
            }

            // Figure out if all non-aggregated event properties in the select clause are listed in the group by
            var allInGroupBy = true;
            string notInGroupByReason = null;
            if (isUsingStreamSelect) {
                allInGroupBy = false;
                notInGroupByReason = "stream select";
            }

            var reasonMessage = propertiesGroupBy.NotContainsAll(nonAggregatedPropsSelect);
            if (reasonMessage != null) {
                notInGroupByReason = reasonMessage;
                allInGroupBy = false;
            }

            // Wildcard select-clause means we do not have all selected properties in the group
            if (isUsingWildcard) {
                allInGroupBy = false;
                notInGroupByReason = "wildcard select";
            }

            // Figure out if all non-aggregated event properties in the order-by clause are listed in the select expression
            var nonAggregatedPropsOrderBy = ExprNodeUtilityAggregation.GetNonAggregatedProps(
                typeService.EventTypes, orderByNodes, contextPropertyRegistry);

            reasonMessage = nonAggregatedPropsSelect.NotContainsAll(nonAggregatedPropsOrderBy);
            var allInSelect = reasonMessage == null;

            // Wildcard select-clause means that all order-by props in the select expression
            if (isUsingWildcard) {
                allInSelect = true;
            }

            // (4)
            // There is a group-by clause, and all event properties in the select clause that are not under an aggregation
            // function are listed in the group-by clause, and if there is an order-by clause, all non-aggregated properties
            // referred to in the order-by clause also appear in the select (output one row per group, not one row per event)
            if (allInGroupBy && allInSelect && localGroupByMatchesGroupBy) {
                var noDataWindowSingleStream = typeService.IStreamOnly[0] && typeService.EventTypes.Length < 2;
                var iterableUnboundConfig = services.Configuration.Compiler.ViewResources.IsIterableUnbound;
                var iterateUnbounded = noDataWindowSingleStream &&
                                       (iterableUnboundConfig || AnnotationUtil.FindAnnotation(
                                            statementRawInfo.Annotations, typeof(IterableUnboundAttribute)) != null);

                Log.Debug(".getProcessor Using ResultSetProcessorRowPerGroup");
                ResultSetProcessorFactoryForge factoryForge;
                ResultSetProcessorType type;
                SelectExprProcessorForge[] selectExprProcessorForges;
                bool rollup;
                if (groupByRollupDesc != null) {
                    factoryForge = new ResultSetProcessorRowPerGroupRollupForge(
                        resultEventType, rollupPerLevelForges, groupByNodesValidated, isSelectRStream, isUnidirectional,
                        outputLimitSpec, orderByProcessorFactory != null, noDataWindowSingleStream, groupByRollupDesc,
                        typeService.EventTypes.Length > 1, isHistoricalOnly, iterateUnbounded, outputConditionType.Value,
                        optionalOutputFirstConditionFactoryForge, typeService.EventTypes);
                    type = ResultSetProcessorType.FULLYAGGREGATED_GROUPED_ROLLUP;
                    selectExprProcessorForges = rollupPerLevelForges.SelectExprProcessorForges;
                    rollup = true;
                }
                else {
                    factoryForge = new ResultSetProcessorRowPerGroupForge(
                        resultEventType, typeService.EventTypes, selectExprProcessorForge, groupByNodesValidated,
                        optionalHavingForge, isSelectRStream, isUnidirectional, outputLimitSpec, hasOrderBy,
                        noDataWindowSingleStream, isHistoricalOnly, iterateUnbounded, outputConditionType.Value,
                        typeService.EventTypes, optionalOutputFirstConditionFactoryForge);
                    type = ResultSetProcessorType.FULLYAGGREGATED_GROUPED;
                    selectExprProcessorForges = new[] {selectExprProcessorForge};
                    rollup = false;
                }

                return new ResultSetProcessorDesc(
                    factoryForge, type, selectExprProcessorForges,
                    join, hasOutputLimit, outputConditionType.Value, hasOutputLimitSnapshot, resultEventType, rollup,
                    aggregationServiceForgeDesc, orderByProcessorFactory, selectSubscriberDescriptor);
            }

            if (groupByRollupDesc != null) {
                throw new ExprValidationException(
                    "Group-by with rollup requires a fully-aggregated query, the query is not full-aggregated because of " +
                    notInGroupByReason);
            }

            // (6)
            // There is a group-by clause, and one or more event properties in the select clause that are not under an aggregation
            // function are not listed in the group-by clause (output one row per event, not one row per group)
            var forge = new ResultSetProcessorAggregateGroupedForge(
                resultEventType, groupByNodesValidated, optionalHavingForge, isSelectRStream, isUnidirectional,
                outputLimitSpec, hasOrderBy, isHistoricalOnly, outputConditionType.Value,
                optionalOutputFirstConditionFactoryForge, typeService.EventTypes);
            return new ResultSetProcessorDesc(
                forge, ResultSetProcessorType.AGGREGATED_GROUPED, new[] {selectExprProcessorForge},
                join, hasOutputLimit, outputConditionType.Value, hasOutputLimitSnapshot, resultEventType, false,
                aggregationServiceForgeDesc, orderByProcessorFactory, selectSubscriberDescriptor);
        }

        private static void ValidateOutputLimit(
            OutputLimitSpec outputLimitSpec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (outputLimitSpec == null) {
                return;
            }

            var validationContext = new ExprValidationContextBuilder(
                new StreamTypeServiceImpl(false), statementRawInfo, services).Build();
            if (outputLimitSpec.AfterTimePeriodExpr != null) {
                var timePeriodExpr = (ExprTimePeriod) ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.OUTPUTLIMIT, outputLimitSpec.AfterTimePeriodExpr, validationContext);
                outputLimitSpec.AfterTimePeriodExpr = timePeriodExpr;
            }

            if (outputLimitSpec.TimePeriodExpr != null) {
                var timePeriodExpr = (ExprTimePeriod) ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.OUTPUTLIMIT, outputLimitSpec.TimePeriodExpr, validationContext);
                outputLimitSpec.TimePeriodExpr = timePeriodExpr;
                if (timePeriodExpr.IsConstantResult && timePeriodExpr.EvaluateAsSeconds(null, true, null) <= 0) {
                    throw new ExprValidationException(
                        "Invalid time period expression returns a zero or negative time interval");
                }
            }
        }

        private static bool AnalyzeLocalGroupBy(
            ExprNode[] groupByNodesValidated,
            IList<ExprAggregateNode> selectAggregateExprNodes,
            IList<ExprAggregateNode> havingAggregateExprNodes,
            IList<ExprAggregateNode> orderByAggregateExprNodes)
        {
            var localGroupByMatchesGroupBy = AnalyzeLocalGroupBy(groupByNodesValidated, selectAggregateExprNodes);
            localGroupByMatchesGroupBy = localGroupByMatchesGroupBy &&
                                         AnalyzeLocalGroupBy(groupByNodesValidated, havingAggregateExprNodes);
            localGroupByMatchesGroupBy = localGroupByMatchesGroupBy &&
                                         AnalyzeLocalGroupBy(groupByNodesValidated, orderByAggregateExprNodes);
            return localGroupByMatchesGroupBy;
        }

        private static bool AnalyzeLocalGroupBy(
            ExprNode[] groupByNodesValidated,
            IList<ExprAggregateNode> aggNodes)
        {
            foreach (var agg in aggNodes) {
                if (agg.OptionalLocalGroupBy != null) {
                    if (!ExprNodeUtilityCompare.DeepEqualsIsSubset(
                        agg.OptionalLocalGroupBy.PartitionExpressions, groupByNodesValidated)) {
                        return false;
                    }
                }
            }

            return true;
        }

        private static GroupByRollupInfo AnalyzeValidateGroupBy(
            GroupByClauseExpressions groupBy,
            ExprValidationContext validationContext)
        {
            if (groupBy == null) {
                return null;
            }

            // validate that group-by expressions are somewhat-plain expressions
            ExprNodeUtilityValidate.ValidateNoSpecialsGroupByExpressions(groupBy.GroupByNodes);

            // validate each expression
            var validated = new ExprNode[groupBy.GroupByNodes.Length];
            for (var i = 0; i < validated.Length; i++) {
                validated[i] = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.GROUPBY, groupBy.GroupByNodes[i], validationContext);
            }

            if (groupBy.GroupByRollupLevels == null) {
                return new GroupByRollupInfo(validated, null);
            }

            var rollup = AggregationGroupByRollupDesc.Make(groupBy.GroupByRollupLevels);

            // callback when hook reporting enabled
            try {
                var hook = (GroupByRollupPlanHook) ImportUtil.GetAnnotationHook(
                    validationContext.Annotations, HookType.INTERNAL_GROUPROLLUP_PLAN, typeof(GroupByRollupPlanHook),
                    validationContext.ImportService);
                if (hook != null) {
                    hook.Query(new GroupByRollupPlanDesc(validated, rollup));
                }
            }
            catch (ExprValidationException) {
                throw new EPException("Failed to obtain hook for " + HookType.INTERNAL_QUERY_PLAN);
            }

            return new GroupByRollupInfo(validated, rollup);
        }

        private static GroupByRollupPerLevelForge GetRollUpPerLevelExpressions(
            ResultSetSpec spec,
            ExprNode[] groupByNodesValidated,
            AggregationGroupByRollupDesc groupByRollupDesc,
            GroupByRollupInfo groupByRollupInfo,
            InsertIntoDesc insertIntoDesc,
            StreamTypeService typeService,
            ExprValidationContext validationContext,
            bool isFireAndForget,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var numLevels = groupByRollupDesc.Levels.Length;
            var groupByExpressions = spec.GroupByClauseExpressions;

            // allocate
            var processors = new SelectExprProcessorForge[numLevels];
            ExprForge[] havingClauses = null;
            if (groupByExpressions.OptHavingNodePerLevel != null) {
                havingClauses = new ExprForge[numLevels];
            }

            OrderByElementForge[][] orderByElements = null;
            if (groupByExpressions.OptOrderByPerLevel != null) {
                orderByElements = new OrderByElementForge[numLevels][];
            }

            // for each expression in the group-by clause determine which properties it refers to
            var propsPerGroupByExpr = new ExprNodePropOrStreamSet[groupByNodesValidated.Length];
            for (var i = 0; i < groupByNodesValidated.Length; i++) {
                propsPerGroupByExpr[i] =
                    ExprNodeUtilityAggregation.GetGroupByPropertiesValidateHasOne(new[] {groupByNodesValidated[i]});
            }

            // for each level obtain a separate select expression processor
            for (var i = 0; i < numLevels; i++) {
                var level = groupByRollupDesc.Levels[i];

                // determine properties rolled up for this level
                var rolledupProps = GetRollupProperties(level, propsPerGroupByExpr);

                var selectClauseLevel = groupByExpressions.SelectClausePerLevel[i];
                var selectClause = GetRollUpSelectClause(
                    spec.SelectClauseSpec, selectClauseLevel, level, rolledupProps, groupByNodesValidated,
                    validationContext);
                var args = new SelectProcessorArgs(
                    selectClause, groupByRollupInfo, false, null, spec.ForClauseSpec, typeService,
                    statementRawInfo.OptionalContextDescriptor, isFireAndForget, spec.Annotations, statementRawInfo,
                    compileTimeServices);
                var forge = SelectExprProcessorFactory.GetProcessor(args, insertIntoDesc, false)
                    .Forge;
                processors[i] = forge;

                if (havingClauses != null) {
                    var havingNode = RewriteRollupValidateExpression(
                        ExprNodeOrigin.HAVING, groupByExpressions.OptHavingNodePerLevel[i], validationContext,
                        rolledupProps, groupByNodesValidated, level);
                    havingClauses[i] = havingNode.Forge;
                }

                if (orderByElements != null) {
                    orderByElements[i] = RewriteRollupOrderBy(
                        spec.OrderByList, groupByExpressions.OptOrderByPerLevel[i], validationContext, rolledupProps,
                        groupByNodesValidated, level);
                }
            }

            return new GroupByRollupPerLevelForge(processors, havingClauses, orderByElements);
        }

        private static OrderByElementForge[] RewriteRollupOrderBy(
            IList<OrderByItem> items,
            ExprNode[] orderByList,
            ExprValidationContext validationContext,
            ExprNodePropOrStreamSet rolledupProps,
            ExprNode[] groupByNodes,
            AggregationGroupByRollupLevel level)
        {
            var elements = new OrderByElementForge[orderByList.Length];
            for (var i = 0; i < orderByList.Length; i++) {
                var validated = RewriteRollupValidateExpression(
                    ExprNodeOrigin.ORDERBY, orderByList[i], validationContext, rolledupProps, groupByNodes, level);
                elements[i] = new OrderByElementForge(validated, items[i].IsDescending);
            }

            return elements;
        }

        private static ExprNodePropOrStreamSet GetRollupProperties(
            AggregationGroupByRollupLevel level,
            ExprNodePropOrStreamSet[] propsPerGroupByExpr)
        {
            // determine properties rolled up for this level
            var rolledupProps = new ExprNodePropOrStreamSet();
            for (var i = 0; i < propsPerGroupByExpr.Length; i++) {
                if (level.IsAggregationTop) {
                    rolledupProps.AddAll(propsPerGroupByExpr[i]);
                }
                else {
                    var rollupContainsGroupExpr = false;
                    foreach (var num in level.RollupKeys) {
                        if (num == i) {
                            rollupContainsGroupExpr = true;
                            break;
                        }
                    }

                    if (!rollupContainsGroupExpr) {
                        rolledupProps.AddAll(propsPerGroupByExpr[i]);
                    }
                }
            }

            return rolledupProps;
        }

        private static SelectClauseElementCompiled[] GetRollUpSelectClause(
            SelectClauseSpecCompiled selectClauseSpec,
            ExprNode[] selectClauseLevel,
            AggregationGroupByRollupLevel level,
            ExprNodePropOrStreamSet rolledupProps,
            ExprNode[] groupByNodesValidated,
            ExprValidationContext validationContext)
        {
            var rewritten = new SelectClauseElementCompiled[selectClauseSpec.SelectExprList.Length];
            for (var i = 0; i < rewritten.Length; i++) {
                var spec = selectClauseSpec.SelectExprList[i];
                if (!(spec is SelectClauseExprCompiledSpec)) {
                    throw new ExprValidationException("Group-by clause with roll-up does not allow wildcard");
                }

                var exprSpec = (SelectClauseExprCompiledSpec) spec;
                var validated = RewriteRollupValidateExpression(
                    ExprNodeOrigin.SELECT, selectClauseLevel[i], validationContext, rolledupProps,
                    groupByNodesValidated, level);
                rewritten[i] = new SelectClauseExprCompiledSpec(
                    validated, exprSpec.AssignedName, exprSpec.ProvidedName, exprSpec.IsEvents);
            }

            return rewritten;
        }

        private static ExprNode RewriteRollupValidateExpression(
            ExprNodeOrigin exprNodeOrigin,
            ExprNode exprNode,
            ExprValidationContext validationContext,
            ExprNodePropOrStreamSet rolledupProps,
            ExprNode[] groupByNodes,
            AggregationGroupByRollupLevel level)
        {
            // rewrite grouping expressions
            var groupingVisitor = new ExprNodeGroupingVisitorWParent();
            exprNode.Accept(groupingVisitor);
            foreach (var groupingNodePair in groupingVisitor.GroupingNodes) {
                // obtain combination - always a single one as grouping nodes cannot have
                var combination = GetGroupExprCombination(groupByNodes, groupingNodePair.Second.ChildNodes);

                var found = false;
                var rollupIndexes = level.IsAggregationTop ? new int[0] : level.RollupKeys;
                foreach (var index in rollupIndexes) {
                    if (index == combination[0]) {
                        found = true;
                        break;
                    }
                }

                var result = found ? 0 : 1;
                var constant = new ExprConstantNodeImpl(result, typeof(int?));
                if (groupingNodePair.First != null) {
                    ExprNodeUtilityModify.ReplaceChildNode(groupingNodePair.First, groupingNodePair.Second, constant);
                }
                else {
                    exprNode = constant;
                }
            }

            // rewrite grouping id expressions
            foreach (var groupingIdNodePair in groupingVisitor.GroupingIdNodes) {
                var combination = GetGroupExprCombination(groupByNodes, groupingIdNodePair.Second.ChildNodes);

                var result = 0;
                for (var i = 0; i < combination.Length; i++) {
                    var index = combination[i];

                    var found = false;
                    var rollupIndexes = level.IsAggregationTop ? new int[0] : level.RollupKeys;
                    foreach (var rollupIndex in rollupIndexes) {
                        if (index == rollupIndex) {
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        result = result + Pow2(combination.Length - i - 1);
                    }
                }

                var constant = new ExprConstantNodeImpl(result, typeof(int?));
                if (groupingIdNodePair.First != null) {
                    ExprNodeUtilityModify.ReplaceChildNode(
                        groupingIdNodePair.First, groupingIdNodePair.Second, constant);
                }
                else {
                    exprNode = constant;
                }
            }

            // rewrite properties
            var identVisitor = new ExprNodeIdentifierCollectVisitorWContainer();
            exprNode.Accept(identVisitor);
            foreach (var node in identVisitor.ExprProperties) {
                var rewrite = false;

                var firstRollupNonPropExpr = rolledupProps.FirstExpression;
                if (firstRollupNonPropExpr != null) {
                    throw new ExprValidationException("Invalid rollup expression " + firstRollupNonPropExpr.Textual);
                }

                foreach (ExprNodePropOrStreamDesc rolledupProp in rolledupProps.Properties) {
                    var prop = (ExprNodePropOrStreamPropDesc) rolledupProp;
                    if (rolledupProp.StreamNum == node.Second.StreamId &&
                        prop.PropertyName.Equals(node.Second.ResolvedPropertyName)) {
                        rewrite = true;
                        break;
                    }
                }

                if (node.First != null && (node.First is ExprPreviousNode || node.First is ExprPriorNode)) {
                    rewrite = false;
                }

                if (!rewrite) {
                    continue;
                }

                var constant = new ExprConstantNodeImpl(null, node.Second.Forge.EvaluationType);
                if (node.First != null) {
                    ExprNodeUtilityModify.ReplaceChildNode(node.First, node.Second, constant);
                }
                else {
                    exprNode = constant;
                }
            }

            return ExprNodeUtilityValidate.GetValidatedSubtree(exprNodeOrigin, exprNode, validationContext);
        }

        private static int[] GetGroupExprCombination(
            ExprNode[] groupByNodes,
            ExprNode[] childNodes)
        {
            ISet<int> indexes = new SortedSet<int>();
            foreach (var child in childNodes) {
                var found = false;

                for (var i = 0; i < groupByNodes.Length; i++) {
                    if (ExprNodeUtilityCompare.DeepEquals(child, groupByNodes[i], false)) {
                        if (indexes.Contains(i)) {
                            throw new ExprValidationException(
                                "Duplicate expression '" +
                                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(child) +
                                "' among grouping function parameters");
                        }

                        indexes.Add(i);
                        found = true;
                    }
                }

                if (!found) {
                    throw new ExprValidationException(
                        "Failed to find expression '" +
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(child) +
                        "' among group-by expressions");
                }
            }

            return CollectionUtil.IntArray(indexes);
        }

        private static void ValidateSelectAssignColNames(
            SelectClauseSpecCompiled selectClauseSpec,
            IList<SelectClauseExprCompiledSpec> namedSelectionList,
            ExprValidationContext validationContext)
        {
            for (var i = 0; i < selectClauseSpec.SelectExprList.Length; i++) {
                // validate element
                var element = selectClauseSpec.SelectExprList[i];
                if (element is SelectClauseExprCompiledSpec) {
                    var expr = (SelectClauseExprCompiledSpec) element;
                    var validatedExpression = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.SELECT, expr.SelectExpression, validationContext);

                    // determine an element name if none assigned
                    var asName = expr.AssignedName;
                    if (asName == null) {
                        asName = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(validatedExpression);
                    }

                    expr.AssignedName = asName;
                    expr.SelectExpression = validatedExpression;
                    namedSelectionList.Add(expr);
                }
            }
        }

        private static void ValidateHaving(
            ExprNodePropOrStreamSet propertiesGroupedBy,
            ExprNode havingNode)
        {
            IList<ExprAggregateNode> aggregateNodesHaving = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(havingNode, aggregateNodesHaving);

            // Any non-aggregated properties must occur in the group-by clause (if there is one)
            if (!propertiesGroupedBy.IsEmpty()) {
                var visitor = new ExprNodeIdentifierAndStreamRefVisitor(true);
                havingNode.Accept(visitor);
                var allPropertiesHaving = visitor.Refs;
                var aggPropertiesHaving =
                    ExprNodeUtilityAggregation.GetAggregatedProperties(aggregateNodesHaving);

                aggPropertiesHaving.RemoveFromList(allPropertiesHaving);
                propertiesGroupedBy.RemoveFromList(allPropertiesHaving);

                if (!allPropertiesHaving.IsEmpty()) {
                    var desc = allPropertiesHaving.First();
                    throw new ExprValidationException(
                        "Non-aggregated " + desc.Textual + " in the HAVING clause must occur in the group-by clause");
                }
            }
        }

        private static int Pow2(int exponent)
        {
            if (exponent == 0) {
                return 1;
            }

            var result = 2;
            for (var i = 0; i < exponent - 1; i++) {
                result = 2 * result;
            }

            return result;
        }

        private static IList<OrderByItem> ExpandColumnNames(
            SelectClauseElementCompiled[] selectionList,
            IList<OrderByItem> orderByUnexpanded)
        {
            if (orderByUnexpanded == null || orderByUnexpanded.IsEmpty()) {
                return Collections.GetEmptyList<OrderByItem>();
            }

            // copy list to modify
            IList<OrderByItem> expanded = new List<OrderByItem>();
            foreach (var item in orderByUnexpanded) {
                expanded.Add(item.Copy());
            }

            // expand
            foreach (var selectElement in selectionList) {
                // process only expressions
                if (!(selectElement is SelectClauseExprCompiledSpec)) {
                    continue;
                }

                var selectExpr = (SelectClauseExprCompiledSpec) selectElement;

                var name = selectExpr.AssignedName;
                if (name != null) {
                    var fullExpr = selectExpr.SelectExpression;

                    for (var ii = 0; ii < expanded.Count; ii++) {
                        var orderByElement = expanded[ii];
                        var swapped = ColumnNamedNodeSwapper.Swap(orderByElement.ExprNode, name, fullExpr);
                        expanded[ii] = new OrderByItem(swapped, orderByElement.IsDescending);
                    }
                }
            }

            return expanded;
        }
    }
} // end of namespace