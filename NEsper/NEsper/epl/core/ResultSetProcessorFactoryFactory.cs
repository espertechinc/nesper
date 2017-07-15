///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.view;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Factory for output processors. Output processors process the result set of a join or of a view
    /// and apply aggregation/grouping, having and some output limiting logic.
    /// <para>
    /// The instance produced by the factory depends on the presence of aggregation functions in the select list,
    /// the presence and nature of the group-by clause.
    /// </para>
    /// <para>
    /// In case (1) and (2) there are no aggregation functions in the select clause.
    /// </para>
    /// <para>
    /// Case (3) is without group-by and with aggregation functions and without non-aggregated properties
    /// in the select list: <pre>select Sum(volume) </pre>.
    /// Always produces one row for new and old data, aggregates without grouping.
    /// </para>
    /// <para>
    /// Case (4) is without group-by and with aggregation functions but with non-aggregated properties
    /// in the select list: <pre>select price, Sum(volume) </pre>.
    /// Produces a row for each event, aggregates without grouping.
    /// </para>
    /// <para>
    /// Case (5) is with group-by and with aggregation functions and all selected properties are grouped-by.
    /// in the select list: <pre>select customerId, Sum(volume) group by customerId</pre>.
    /// Produces a old and new data row for each group changed, aggregates with grouping, see
    /// <seealso cref="ResultSetProcessorRowPerGroup" />
    /// </para>
    /// <para>
    /// Case (6) is with group-by and with aggregation functions and only some selected properties are grouped-by.
    /// in the select list: <pre>select customerId, supplierId, Sum(volume) group by customerId</pre>.
    /// Produces row for each event, aggregates with grouping.
    /// </para>
    /// </summary>
    public class ResultSetProcessorFactoryFactory {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static ResultSetProcessorFactoryDesc GetProcessorPrototype(StatementSpecCompiled statementSpec,
                                                                          StatementContext stmtContext,
                                                                          StreamTypeService typeService,
                                                                          ViewResourceDelegateUnverified viewResourceDelegate,
                                                                          bool[] isUnidirectionalStream,
                                                                          bool allowAggregation,
                                                                          ContextPropertyRegistry contextPropertyRegistry,
                                                                          SelectExprProcessorDeliveryCallback selectExprProcessorCallback,
                                                                          ConfigurationInformation configurationInformation,
                                                                          ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
                                                                          bool isFireAndForget,
                                                                          bool isOnSelect
        )
                {
            OrderByItem[] orderByListUnexpanded = statementSpec.OrderByList;
            SelectClauseSpecCompiled selectClauseSpec = statementSpec.SelectClauseSpec;
            InsertIntoDesc insertIntoDesc = statementSpec.InsertIntoDesc;
            ExprNode optionalHavingNode = statementSpec.HavingExprRootNode;
            OutputLimitSpec outputLimitSpec = statementSpec.OutputLimitSpec;
            var declaredNodes = new List<ExprDeclaredNode>();
    
            // validate output limit spec
            ValidateOutputLimit(outputLimitSpec, stmtContext);
    
            // determine unidirectional
            bool isUnidirectional = false;
            for (int i = 0; i < isUnidirectionalStream.Length; i++) {
                isUnidirectional |= isUnidirectionalStream[i];
            }
    
            // determine single-stream historical
            bool isHistoricalOnly = false;
            if (statementSpec.StreamSpecs.Length == 1) {
                StreamSpecCompiled spec = statementSpec.StreamSpecs[0];
                if (spec is DBStatementStreamSpec || spec is MethodStreamSpec || spec is TableQueryStreamSpec) {
                    isHistoricalOnly = true;
                }
            }
    
            // determine join or number of streams
            int numStreams = typeService.EventTypes.Length;
    
            // Expand any instances of select-clause names in the
            // order-by clause with the full expression
            List<OrderByItem> orderByList = ExpandColumnNames(selectClauseSpec.SelectExprList, orderByListUnexpanded);
    
            // Validate selection expressions, if any (could be wildcard i.e. empty list)
            var namedSelectionList = new LinkedList<SelectClauseExprCompiledSpec>();
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(stmtContext, false);
            bool allowRollup = statementSpec.GroupByExpressions != null && statementSpec.GroupByExpressions.GroupByRollupLevels != null;
            bool resettableAggs = isUnidirectional || statementSpec.OnTriggerDesc != null;
            string intoTableName = statementSpec.IntoTableSpec == null ? null : statementSpec.IntoTableSpec.Name;
            var validationContext = new ExprValidationContext(typeService, stmtContext.EngineImportService, stmtContext.StatementExtensionServicesContext, viewResourceDelegate, stmtContext.SchedulingService, stmtContext.VariableService, stmtContext.TableService, evaluatorContextStmt, stmtContext.EventAdapterService, stmtContext.StatementName, stmtContext.StatementId, stmtContext.Annotations, stmtContext.ContextDescriptor, false, allowRollup, true, resettableAggs, intoTableName, false);
    
            ValidateSelectAssignColNames(selectClauseSpec, namedSelectionList, validationContext);
            if (statementSpec.GroupByExpressions != null && statementSpec.GroupByExpressions.SelectClausePerLevel != null) {
                ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.GROUPBY, statementSpec.GroupByExpressions.SelectClausePerLevel, validationContext);
            }
            bool isUsingWildcard = selectClauseSpec.IsUsingWildcard;
    
            // Validate stream selections, if any (such as stream.*)
            bool isUsingStreamSelect = false;
            foreach (SelectClauseElementCompiled compiled in selectClauseSpec.SelectExprList) {
                if (!(compiled is SelectClauseStreamCompiledSpec)) {
                    continue;
                }
                SelectClauseStreamCompiledSpec streamSelectSpec = (SelectClauseStreamCompiledSpec) compiled;
                int streamNum = int?.MIN_VALUE;
                bool isFragmentEvent = false;
                bool isProperty = false;
                Type propertyType = null;
                isUsingStreamSelect = true;
                for (int i = 0; i < typeService.StreamNames.Length; i++) {
                    string streamName = streamSelectSpec.StreamName;
                    if (typeService.StreamNames[i].Equals(streamName)) {
                        streamNum = i;
                        break;
                    }
    
                    // see if the stream name is known as a nested event type
                    EventType candidateProviderOfFragments = typeService.EventTypes[i];
                    // for the native event type we don't need to fragment, we simply use the property itself since all wrappers understand Java objects
                    if (!(candidateProviderOfFragments is NativeEventType) && (candidateProviderOfFragments.GetFragmentType(streamName) != null)) {
                        streamNum = i;
                        isFragmentEvent = true;
                        break;
                    }
                }
    
                // stream name not found
                if (streamNum == int?.MIN_VALUE) {
                    // see if the stream name specified resolves as a property
                    PropertyResolutionDescriptor desc = null;
                    try {
                        desc = typeService.ResolveByPropertyName(streamSelectSpec.StreamName, false);
                    } catch (StreamTypesException e) {
                        // not handled
                    }
    
                    if (desc == null) {
                        throw new ExprValidationException("Stream selector '" + streamSelectSpec.StreamName + ".*' does not match any stream name in the from clause");
                    }
                    isProperty = true;
                    propertyType = desc.PropertyType;
                    streamNum = desc.StreamNum;
                }
    
                streamSelectSpec.StreamNumber = streamNum;
                streamSelectSpec.FragmentEvent = isFragmentEvent;
                streamSelectSpec.Property = isProperty, propertyType;
    
                if (streamNum >= 0) {
                    TableMetadata tableMetadata = stmtContext.TableService.GetTableMetadataFromEventType(typeService.EventTypes[streamNum]);
                    streamSelectSpec.TableMetadata = tableMetadata;
                }
            }
    
            // Validate having clause, if present
            if (optionalHavingNode != null) {
                optionalHavingNode = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.HAVING, optionalHavingNode, validationContext);
                if (statementSpec.GroupByExpressions != null) {
                    ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.GROUPBY, statementSpec.GroupByExpressions.OptHavingNodePerLevel, validationContext);
                }
            }
    
            // Validate order-by expressions, if any (could be empty list for no order-by)
            for (int i = 0; i < orderByList.Count; i++) {
                ExprNode orderByNode = orderByList.Get(i).ExprNode;
    
                // Ensure there is no subselects
                var visitor = new ExprNodeSubselectDeclaredDotVisitor();
                orderByNode.Accept(visitor);
                if (visitor.Subselects.Count > 0) {
                    throw new ExprValidationException("Subselects not allowed within order-by clause");
                }
    
                bool? isDescending = orderByList.Get(i).IsDescending;
                var validatedOrderBy = new OrderByItem(ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.ORDERBY, orderByNode, validationContext), isDescending);
                orderByList.Set(i, validatedOrderBy);
    
                if (statementSpec.GroupByExpressions != null && statementSpec.GroupByExpressions.OptOrderByPerLevel != null) {
                    ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.GROUPBY, statementSpec.GroupByExpressions.OptOrderByPerLevel, validationContext);
                }
            }
    
            // Get the select expression nodes
            var selectNodes = new List<ExprNode>();
            foreach (SelectClauseExprCompiledSpec element in namedSelectionList) {
                selectNodes.Add(element.SelectExpression);
            }
    
            // Get the order-by expression nodes
            var orderByNodes = new List<ExprNode>();
            foreach (OrderByItem element in orderByList) {
                orderByNodes.Add(element.ExprNode);
            }
    
            // Determine aggregate functions used in select, if any
            var selectAggregateExprNodes = new LinkedList<ExprAggregateNode>();
            var selectAggregationNodesNamed = new Dictionary<ExprNode, string>();
            var declaredNodeVisitor = new ExprNodeDeclaredVisitor();
            foreach (SelectClauseExprCompiledSpec element in namedSelectionList) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(element.SelectExpression, selectAggregateExprNodes);
                if (element.ProvidedName != null) {
                    selectAggregationNodesNamed.Put(element.SelectExpression, element.ProvidedName);
                }
                element.SelectExpression.Accept(declaredNodeVisitor);
                declaredNodes.AddAll(declaredNodeVisitor.DeclaredExpressions);
                declaredNodeVisitor.Clear();
            }
            if (statementSpec.GroupByExpressions != null) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(statementSpec.GroupByExpressions.SelectClausePerLevel, selectAggregateExprNodes);
            }
            if (!allowAggregation && !selectAggregateExprNodes.IsEmpty()) {
                throw new ExprValidationException("Aggregation functions are not allowed in this context");
            }
    
            // Determine if we have a having clause with aggregation
            var havingAggregateExprNodes = new LinkedList<ExprAggregateNode>();
            var propertiesAggregatedHaving = new ExprNodePropOrStreamSet();
            if (optionalHavingNode != null) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(optionalHavingNode, havingAggregateExprNodes);
                if (statementSpec.GroupByExpressions != null) {
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(statementSpec.GroupByExpressions.OptHavingNodePerLevel, havingAggregateExprNodes);
                }
                propertiesAggregatedHaving = ExprNodeUtility.GetAggregatedProperties(havingAggregateExprNodes);
            }
            if (!allowAggregation && !havingAggregateExprNodes.IsEmpty()) {
                throw new ExprValidationException("Aggregation functions are not allowed in this context");
            }
    
            // Determine if we have a order-by clause with aggregation
            var orderByAggregateExprNodes = new LinkedList<ExprAggregateNode>();
            if (orderByNodes != null && !orderByNodes.IsEmpty()) {
                foreach (ExprNode orderByNode in orderByNodes) {
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(orderByNode, orderByAggregateExprNodes);
                }
                if (statementSpec.GroupByExpressions != null) {
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(statementSpec.GroupByExpressions.OptOrderByPerLevel, orderByAggregateExprNodes);
                }
                if (!allowAggregation && !orderByAggregateExprNodes.IsEmpty()) {
                    throw new ExprValidationException("Aggregation functions are not allowed in this context");
                }
            }
    
            // Analyze rollup
            GroupByRollupInfo groupByRollupInfo = AnalyzeValidateGroupBy(statementSpec.GroupByExpressions, validationContext);
            ExprNode[] groupByNodesValidated = groupByRollupInfo == null ? new ExprNode[0] : groupByRollupInfo.ExprNodes;
            AggregationGroupByRollupDesc groupByRollupDesc = groupByRollupInfo == null ? null : groupByRollupInfo.RollupDesc;
    
            // Construct the appropriate aggregation service
            bool hasGroupBy = groupByNodesValidated.Length > 0;
            AggregationServiceFactoryDesc aggregationServiceFactory = AggregationServiceFactoryFactory.GetService(
                    selectAggregateExprNodes, selectAggregationNodesNamed, declaredNodes, groupByNodesValidated, havingAggregateExprNodes, orderByAggregateExprNodes, Collections.<ExprAggregateNodeGroupKey>EmptyList(), hasGroupBy, statementSpec.Annotations, stmtContext.VariableService, typeService.EventTypes.Length > 1, false,
                    statementSpec.FilterRootNode, statementSpec.HavingExprRootNode,
                    stmtContext.AggregationServiceFactoryService, typeService.EventTypes, groupByRollupDesc,
                    statementSpec.OptionalContextName, statementSpec.IntoTableSpec, stmtContext.TableService, isUnidirectional, isFireAndForget, isOnSelect, stmtContext.EngineImportService);
    
            // Compare local-aggregation versus group-by
            bool localGroupByMatchesGroupBy = AnalyzeLocalGroupBy(groupByNodesValidated, selectAggregateExprNodes, havingAggregateExprNodes, orderByAggregateExprNodes);
    
            bool useCollatorSort = false;
            if (stmtContext.ConfigSnapshot != null) {
                useCollatorSort = stmtContext.ConfigSnapshot.EngineDefaults.Language.IsSortUsingCollator;
            }
    
            // Construct the processor for sorting output events
            OrderByProcessorFactory orderByProcessorFactory = OrderByProcessorFactoryFactory.GetProcessor(namedSelectionList,
                    groupByNodesValidated, orderByList, statementSpec.RowLimitSpec, stmtContext.VariableService, useCollatorSort, statementSpec.OptionalContextName);
    
            // Construct the processor for evaluating the select clause
            var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry(stmtContext.StatementName, stmtContext.StatementEventTypeRef);
            SelectExprProcessor selectExprProcessor = SelectExprProcessorFactory.GetProcessor(Collections.<int?>EmptyList(), selectClauseSpec.SelectExprList, isUsingWildcard, insertIntoDesc, null, statementSpec.ForClauseSpec, typeService, stmtContext.EventAdapterService, stmtContext.StatementResultService, stmtContext.ValueAddEventService, selectExprEventTypeRegistry, stmtContext.EngineImportService, evaluatorContextStmt,
                    stmtContext.VariableService, stmtContext.TableService, stmtContext.TimeProvider, stmtContext.EngineURI, stmtContext.StatementId, stmtContext.StatementName, stmtContext.Annotations, stmtContext.ContextDescriptor, stmtContext.ConfigSnapshot, selectExprProcessorCallback, stmtContext.NamedWindowMgmtService, statementSpec.IntoTableSpec, groupByRollupInfo, stmtContext.StatementExtensionServicesContext);
    
            // Get a list of event properties being aggregated in the select clause, if any
            ExprNodePropOrStreamSet propertiesGroupBy = ExprNodeUtility.GetGroupByPropertiesValidateHasOne(groupByNodesValidated);
            // Figure out all non-aggregated event properties in the select clause (props not under a sum/avg/max aggregation node)
            ExprNodePropOrStreamSet nonAggregatedPropsSelect = ExprNodeUtility.GetNonAggregatedProps(typeService.EventTypes, selectNodes, contextPropertyRegistry);
            if (optionalHavingNode != null) {
                ExprNodeUtility.AddNonAggregatedProps(optionalHavingNode, nonAggregatedPropsSelect, typeService.EventTypes, contextPropertyRegistry);
            }
    
            // Validate the having-clause (selected aggregate nodes and all in group-by are allowed)
            bool hasAggregation = (!selectAggregateExprNodes.IsEmpty()) || (!havingAggregateExprNodes.IsEmpty()) || (!orderByAggregateExprNodes.IsEmpty()) || (!propertiesAggregatedHaving.IsEmpty());
            if (optionalHavingNode != null && hasAggregation) {
                ValidateHaving(propertiesGroupBy, optionalHavingNode);
            }
    
            // We only generate Remove-Stream events if they are explicitly selected, or the insert-into requires them
            bool isSelectRStream = statementSpec.SelectStreamSelectorEnum == SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH
                    || statementSpec.SelectStreamSelectorEnum == SelectClauseStreamSelectorEnum.RSTREAM_ONLY;
            if ((statementSpec.InsertIntoDesc != null) && (statementSpec.InsertIntoDesc.StreamSelector.IsSelectsRStream)) {
                isSelectRStream = true;
            }
    
            ExprEvaluator optionHavingEval = optionalHavingNode == null ? null : optionalHavingNode.ExprEvaluator;
            bool hasOutputLimitOptHint = HintEnum.ENABLE_OUTPUTLIMIT_OPT.GetHint(statementSpec.Annotations) != null;
    
            // Determine output-first condition factory
            OutputConditionPolledFactory optionalOutputFirstConditionFactory = null;
            if (outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST) {
                optionalOutputFirstConditionFactory = OutputConditionPolledFactoryFactory.CreateConditionFactory(outputLimitSpec, stmtContext);
            }
    
            // (1)
            // There is no group-by clause and no aggregate functions with event properties in the select clause and having clause (simplest case)
            if ((groupByNodesValidated.Length == 0) && (selectAggregateExprNodes.IsEmpty()) && (havingAggregateExprNodes.IsEmpty())) {
                // Determine if any output rate limiting must be performed early while processing results
                // Snapshot output does not count in terms of limiting output for grouping/aggregation purposes
                bool isOutputLimitingNoSnapshot = (outputLimitSpec != null) && (outputLimitSpec.DisplayLimit != OutputLimitLimitType.SNAPSHOT);
    
                // (1a)
                // There is no need to perform select expression processing, the single view itself (no join) generates
                // events in the desired format, therefore there is no output processor. There are no order-by expressions.
                if (orderByNodes.IsEmpty() && optionalHavingNode == null && !isOutputLimitingNoSnapshot && statementSpec.RowLimitSpec == null) {
                    Log.Debug(".getProcessor Using no result processor");
                    var factory = new ResultSetProcessorHandThroughFactory(selectExprProcessor, isSelectRStream);
                    return new ResultSetProcessorFactoryDesc(factory, orderByProcessorFactory, aggregationServiceFactory);
                }
    
                // (1b)
                // We need to process the select expression in a simple fashion, with each event (old and new)
                // directly generating one row, and no need to update aggregate state since there is no aggregate function.
                // There might be some order-by expressions.
                Log.Debug(".getProcessor Using ResultSetProcessorSimple");
                var factory = new ResultSetProcessorSimpleFactory(selectExprProcessor, optionHavingEval, isSelectRStream, outputLimitSpec, hasOutputLimitOptHint, resultSetProcessorHelperFactory, numStreams);
                return new ResultSetProcessorFactoryDesc(factory, orderByProcessorFactory, aggregationServiceFactory);
            }
    
            // (2)
            // A wildcard select-clause has been specified and the group-by is ignored since no aggregation functions are used, and no having clause
            bool isLast = statementSpec.OutputLimitSpec != null && statementSpec.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;
            bool isFirst = statementSpec.OutputLimitSpec != null && statementSpec.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST;
            if ((namedSelectionList.IsEmpty()) && (propertiesAggregatedHaving.IsEmpty()) && (havingAggregateExprNodes.IsEmpty()) && !isLast && !isFirst) {
                Log.Debug(".getProcessor Using ResultSetProcessorSimple");
                var factory = new ResultSetProcessorSimpleFactory(selectExprProcessor, optionHavingEval, isSelectRStream, outputLimitSpec, hasOutputLimitOptHint, resultSetProcessorHelperFactory, numStreams);
                return new ResultSetProcessorFactoryDesc(factory, orderByProcessorFactory, aggregationServiceFactory);
            }
    
            if ((groupByNodesValidated.Length == 0) && hasAggregation) {
                // (3)
                // There is no group-by clause and there are aggregate functions with event properties in the select clause (aggregation case)
                // or having class, and all event properties are aggregated (all properties are under aggregation functions).
                bool hasStreamSelect = ExprNodeUtility.HasStreamSelect(selectNodes);
                if ((nonAggregatedPropsSelect.IsEmpty()) && !hasStreamSelect && !isUsingWildcard && !isUsingStreamSelect && localGroupByMatchesGroupBy && (viewResourceDelegate == null || viewResourceDelegate.PreviousRequests.IsEmpty())) {
                    Log.Debug(".getProcessor Using ResultSetProcessorRowForAll");
                    var factory = new ResultSetProcessorRowForAllFactory(selectExprProcessor, optionHavingEval, isSelectRStream, isUnidirectional, isHistoricalOnly, outputLimitSpec, resultSetProcessorHelperFactory);
                    return new ResultSetProcessorFactoryDesc(factory, orderByProcessorFactory, aggregationServiceFactory);
                }
    
                // (4)
                // There is no group-by clause but there are aggregate functions with event properties in the select clause (aggregation case)
                // or having clause and not all event properties are aggregated (some properties are not under aggregation functions).
                Log.Debug(".getProcessor Using ResultSetProcessorAggregateAll");
                var factory = new ResultSetProcessorAggregateAllFactory(selectExprProcessor, optionHavingEval, isSelectRStream, isUnidirectional, isHistoricalOnly, outputLimitSpec, hasOutputLimitOptHint, resultSetProcessorHelperFactory);
                return new ResultSetProcessorFactoryDesc(factory, orderByProcessorFactory, aggregationServiceFactory);
            }
    
            // Handle group-by cases
            if (groupByNodesValidated.Length == 0) {
                throw new IllegalStateException("Unexpected empty group-by expression list");
            }
    
            // Figure out if all non-aggregated event properties in the select clause are listed in the group by
            bool allInGroupBy = true;
            string notInGroupByReason = null;
            if (isUsingStreamSelect) {
                allInGroupBy = false;
                notInGroupByReason = "stream select";
            }
    
            string reasonMessage = propertiesGroupBy.NotContainsAll(nonAggregatedPropsSelect);
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
            ExprNodePropOrStreamSet nonAggregatedPropsOrderBy = ExprNodeUtility.GetNonAggregatedProps(typeService.EventTypes, orderByNodes, contextPropertyRegistry);
    
            reasonMessage = nonAggregatedPropsSelect.NotContainsAll(nonAggregatedPropsOrderBy);
            bool allInSelect = reasonMessage == null;
    
            // Wildcard select-clause means that all order-by props in the select expression
            if (isUsingWildcard) {
                allInSelect = true;
            }
    
            // (4)
            // There is a group-by clause, and all event properties in the select clause that are not under an aggregation
            // function are listed in the group-by clause, and if there is an order-by clause, all non-aggregated properties
            // referred to in the order-by clause also appear in the select (output one row per group, not one row per event)
            ExprEvaluator[] groupByEval = ExprNodeUtility.GetEvaluators(groupByNodesValidated);
            if (allInGroupBy && allInSelect && localGroupByMatchesGroupBy) {
                bool noDataWindowSingleStream = typeService.IStreamOnly[0] && typeService.EventTypes.Length < 2;
                bool iterableUnboundConfig = configurationInformation.EngineDefaults.ViewResources.IsIterableUnbound;
                bool iterateUnbounded = noDataWindowSingleStream && (iterableUnboundConfig || AnnotationUtil.FindAnnotation(statementSpec.Annotations, typeof(IterableUnbound)) != null);
    
                Log.Debug(".getProcessor Using ResultSetProcessorRowPerGroup");
                ResultSetProcessorFactory factory;
                if (groupByRollupDesc != null) {
                    GroupByRollupPerLevelExpression perLevelExpression = GetRollUpPerLevelExpressions(statementSpec, groupByNodesValidated, groupByRollupDesc, stmtContext, selectExprEventTypeRegistry, evaluatorContextStmt, insertIntoDesc, typeService, validationContext, groupByRollupInfo);
                    factory = new ResultSetProcessorRowPerGroupRollupFactory(perLevelExpression, groupByNodesValidated, groupByEval, isSelectRStream, isUnidirectional, outputLimitSpec, orderByProcessorFactory != null, noDataWindowSingleStream, groupByRollupDesc, typeService.EventTypes.Length > 1, isHistoricalOnly, iterateUnbounded, optionalOutputFirstConditionFactory, resultSetProcessorHelperFactory, hasOutputLimitOptHint, numStreams);
                } else {
                    factory = new ResultSetProcessorRowPerGroupFactory(selectExprProcessor, groupByNodesValidated, groupByEval, optionHavingEval, isSelectRStream, isUnidirectional, outputLimitSpec, orderByProcessorFactory != null, noDataWindowSingleStream, isHistoricalOnly, iterateUnbounded, resultSetProcessorHelperFactory, hasOutputLimitOptHint, numStreams, optionalOutputFirstConditionFactory);
                }
                return new ResultSetProcessorFactoryDesc(factory, orderByProcessorFactory, aggregationServiceFactory);
            }
    
            if (groupByRollupDesc != null) {
                throw new ExprValidationException("Group-by with rollup requires a fully-aggregated query, the query is not full-aggregated because of " + notInGroupByReason);
            }
    
            // (6)
            // There is a group-by clause, and one or more event properties in the select clause that are not under an aggregation
            // function are not listed in the group-by clause (output one row per event, not one row per group)
            Log.Debug(".getProcessor Using ResultSetProcessorAggregateGrouped");
            var factory = new ResultSetProcessorAggregateGroupedFactory(selectExprProcessor, groupByNodesValidated, groupByEval, optionHavingEval, isSelectRStream, isUnidirectional, outputLimitSpec, orderByProcessorFactory != null, isHistoricalOnly, resultSetProcessorHelperFactory, optionalOutputFirstConditionFactory, hasOutputLimitOptHint, numStreams);
            return new ResultSetProcessorFactoryDesc(factory, orderByProcessorFactory, aggregationServiceFactory);
        }
    
        private static void ValidateOutputLimit(OutputLimitSpec outputLimitSpec, StatementContext statementContext) {
            if (outputLimitSpec == null) {
                return;
            }
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
            var validationContext = new ExprValidationContext(new StreamTypeServiceImpl(statementContext.EngineURI, false), statementContext.EngineImportService, statementContext.StatementExtensionServicesContext, null, statementContext.TimeProvider, statementContext.VariableService, statementContext.TableService, evaluatorContextStmt, statementContext.EventAdapterService, statementContext.StatementName, statementContext.StatementId, statementContext.Annotations, statementContext.ContextDescriptor, false, false, false, false, null, false);
            if (outputLimitSpec.AfterTimePeriodExpr != null) {
                ExprTimePeriod timePeriodExpr = (ExprTimePeriod) ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.OUTPUTLIMIT, outputLimitSpec.AfterTimePeriodExpr, validationContext);
                outputLimitSpec.AfterTimePeriodExpr = timePeriodExpr;
            }
            if (outputLimitSpec.TimePeriodExpr != null) {
                ExprTimePeriod timePeriodExpr = (ExprTimePeriod) ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.OUTPUTLIMIT, outputLimitSpec.TimePeriodExpr, validationContext);
                outputLimitSpec.TimePeriodExpr = timePeriodExpr;
                if (timePeriodExpr.IsConstantResult && timePeriodExpr.EvaluateAsSeconds(null, true, new ExprEvaluatorContextStatement(statementContext, false)) <= 0) {
                    throw new ExprValidationException("Invalid time period expression returns a zero or negative time interval");
                }
            }
        }
    
        private static bool AnalyzeLocalGroupBy(ExprNode[] groupByNodesValidated, List<ExprAggregateNode> selectAggregateExprNodes, List<ExprAggregateNode> havingAggregateExprNodes, List<ExprAggregateNode> orderByAggregateExprNodes) {
            bool localGroupByMatchesGroupBy = AnalyzeLocalGroupBy(groupByNodesValidated, selectAggregateExprNodes);
            localGroupByMatchesGroupBy = localGroupByMatchesGroupBy && AnalyzeLocalGroupBy(groupByNodesValidated, havingAggregateExprNodes);
            localGroupByMatchesGroupBy = localGroupByMatchesGroupBy && AnalyzeLocalGroupBy(groupByNodesValidated, orderByAggregateExprNodes);
            return localGroupByMatchesGroupBy;
        }
    
        private static bool AnalyzeLocalGroupBy(ExprNode[] groupByNodesValidated, List<ExprAggregateNode> aggNodes) {
            foreach (ExprAggregateNode agg in aggNodes) {
                if (agg.OptionalLocalGroupBy != null) {
                    if (!ExprNodeUtility.DeepEqualsIsSubset(agg.OptionalLocalGroupBy.PartitionExpressions, groupByNodesValidated)) {
                        return false;
                    }
                }
            }
            return true;
        }
    
        private static GroupByRollupInfo AnalyzeValidateGroupBy(GroupByClauseExpressions groupBy, ExprValidationContext validationContext)
                {
            if (groupBy == null) {
                return null;
            }
    
            // validate that group-by expressions are somewhat-plain expressions
            ExprNodeUtility.ValidateNoSpecialsGroupByExpressions(groupBy.GroupByNodes);
    
            // validate each expression
            var validated = new ExprNode[groupBy.GroupByNodes.Length];
            for (int i = 0; i < validated.Length; i++) {
                validated[i] = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.GROUPBY, groupBy.GroupByNodes[i], validationContext);
            }
    
            if (groupBy.GroupByRollupLevels == null) {
                return new GroupByRollupInfo(validated, null);
            }
    
            AggregationGroupByRollupDesc rollup = AggregationGroupByRollupDesc.Make(groupBy.GroupByRollupLevels);
    
            // callback when hook reporting enabled
            try {
                GroupByRollupPlanHook hook = (GroupByRollupPlanHook) TypeHelper.GetAnnotationHook(validationContext.Annotations, HookType.INTERNAL_GROUPROLLUP_PLAN, typeof(GroupByRollupPlanHook), validationContext.EngineImportService);
                if (hook != null) {
                    hook.Query(new GroupByRollupPlanDesc(validated, rollup));
                }
            } catch (ExprValidationException e) {
                throw new EPException("Failed to obtain hook for " + HookType.INTERNAL_QUERY_PLAN);
            }
    
            return new GroupByRollupInfo(validated, rollup);
        }
    
        private static GroupByRollupPerLevelExpression GetRollUpPerLevelExpressions(StatementSpecCompiled statementSpec, ExprNode[] groupByNodesValidated, AggregationGroupByRollupDesc groupByRollupDesc, StatementContext stmtContext, SelectExprEventTypeRegistry selectExprEventTypeRegistry, ExprEvaluatorContextStatement evaluatorContextStmt, InsertIntoDesc insertIntoDesc, StreamTypeService typeService, ExprValidationContext validationContext, GroupByRollupInfo groupByRollupInfo)
                {
            int numLevels = groupByRollupDesc.Levels.Length;
            GroupByClauseExpressions groupByExpressions = statementSpec.GroupByExpressions;
    
            // allocate
            var processors = new SelectExprProcessor[numLevels];
            ExprEvaluator[] havingClauses = null;
            if (groupByExpressions.OptHavingNodePerLevel != null) {
                havingClauses = new ExprEvaluator[numLevels];
            }
            OrderByElement[][] orderByElements = null;
            if (groupByExpressions.OptOrderByPerLevel != null) {
                orderByElements = new OrderByElement[numLevels][];
            }
    
            // for each expression in the group-by clause determine which properties it refers to
            var propsPerGroupByExpr = new ExprNodePropOrStreamSet[groupByNodesValidated.Length];
            for (int i = 0; i < groupByNodesValidated.Length; i++) {
                propsPerGroupByExpr[i] = ExprNodeUtility.GetGroupByPropertiesValidateHasOne(new ExprNode[]{groupByNodesValidated[i]});
            }
    
            // for each level obtain a separate select expression processor
            for (int i = 0; i < numLevels; i++) {
                AggregationGroupByRollupLevel level = groupByRollupDesc.Levels[i];
    
                // determine properties rolled up for this level
                ExprNodePropOrStreamSet rolledupProps = GetRollupProperties(level, propsPerGroupByExpr);
    
                ExprNode[] selectClauseLevel = groupByExpressions.SelectClausePerLevel[i];
                SelectClauseElementCompiled[] selectClause = GetRollUpSelectClause(statementSpec.SelectClauseSpec, selectClauseLevel, level, rolledupProps, groupByNodesValidated, validationContext);
                processors[i] = SelectExprProcessorFactory.GetProcessor(Collections.<int?>EmptyList(), selectClause, false, insertIntoDesc, null, statementSpec.ForClauseSpec, typeService, stmtContext.EventAdapterService, stmtContext.StatementResultService, stmtContext.ValueAddEventService, selectExprEventTypeRegistry, stmtContext.EngineImportService, evaluatorContextStmt,
                        stmtContext.VariableService, stmtContext.TableService, stmtContext.TimeProvider, stmtContext.EngineURI, stmtContext.StatementId, stmtContext.StatementName, stmtContext.Annotations, stmtContext.ContextDescriptor, stmtContext.ConfigSnapshot, null, stmtContext.NamedWindowMgmtService, statementSpec.IntoTableSpec, groupByRollupInfo, stmtContext.StatementExtensionServicesContext);
    
                if (havingClauses != null) {
                    havingClauses[i] = RewriteRollupValidateExpression(ExprNodeOrigin.HAVING, groupByExpressions.OptHavingNodePerLevel[i], validationContext, rolledupProps, groupByNodesValidated, level).ExprEvaluator;
                }
    
                if (orderByElements != null) {
                    orderByElements[i] = RewriteRollupOrderBy(statementSpec.OrderByList, groupByExpressions.OptOrderByPerLevel[i], validationContext, rolledupProps, groupByNodesValidated, level);
                }
            }
    
            return new GroupByRollupPerLevelExpression(processors, havingClauses, orderByElements);
        }
    
        private static OrderByElement[] RewriteRollupOrderBy(OrderByItem[] items, ExprNode[] orderByList, ExprValidationContext validationContext, ExprNodePropOrStreamSet rolledupProps, ExprNode[] groupByNodes, AggregationGroupByRollupLevel level)
                {
            var elements = new OrderByElement[orderByList.Length];
            for (int i = 0; i < orderByList.Length; i++) {
                ExprNode validated = RewriteRollupValidateExpression(ExprNodeOrigin.ORDERBY, orderByList[i], validationContext, rolledupProps, groupByNodes, level);
                elements[i] = new OrderByElement(validated, validated.ExprEvaluator, items[i].IsDescending);
            }
            return elements;
        }
    
        private static ExprNodePropOrStreamSet GetRollupProperties(AggregationGroupByRollupLevel level, ExprNodePropOrStreamSet[] propsPerGroupByExpr) {
            // determine properties rolled up for this level
            var rolledupProps = new ExprNodePropOrStreamSet();
            for (int i = 0; i < propsPerGroupByExpr.Length; i++) {
                if (level.IsAggregationTop) {
                    rolledupProps.AddAll(propsPerGroupByExpr[i]);
                } else {
                    bool rollupContainsGroupExpr = false;
                    foreach (int num in level.RollupKeys) {
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
    
        private static SelectClauseElementCompiled[] GetRollUpSelectClause(SelectClauseSpecCompiled selectClauseSpec, ExprNode[] selectClauseLevel, AggregationGroupByRollupLevel level, ExprNodePropOrStreamSet rolledupProps, ExprNode[] groupByNodesValidated, ExprValidationContext validationContext)
                {
            var rewritten = new SelectClauseElementCompiled[selectClauseSpec.SelectExprList.Length];
            for (int i = 0; i < rewritten.Length; i++) {
                SelectClauseElementCompiled spec = selectClauseSpec.SelectExprList[i];
                if (!(spec is SelectClauseExprCompiledSpec)) {
                    throw new ExprValidationException("Group-by clause with roll-up does not allow wildcard");
                }
    
                SelectClauseExprCompiledSpec exprSpec = (SelectClauseExprCompiledSpec) spec;
                ExprNode validated = RewriteRollupValidateExpression(ExprNodeOrigin.SELECT, selectClauseLevel[i], validationContext, rolledupProps, groupByNodesValidated, level);
                rewritten[i] = new SelectClauseExprCompiledSpec(validated, exprSpec.AssignedName, exprSpec.ProvidedName, exprSpec.IsEvents);
            }
            return rewritten;
        }
    
        private static ExprNode RewriteRollupValidateExpression(ExprNodeOrigin exprNodeOrigin,
                                                                ExprNode exprNode,
                                                                ExprValidationContext validationContext,
                                                                ExprNodePropOrStreamSet rolledupProps,
                                                                ExprNode[] groupByNodes,
                                                                AggregationGroupByRollupLevel level)
                {
            // rewrite grouping expressions
            var groupingVisitor = new ExprNodeGroupingVisitorWParent();
            exprNode.Accept(groupingVisitor);
            for (Pair<ExprNode, ExprGroupingNode> groupingNodePair : groupingVisitor.GroupingNodes) {
                // obtain combination - always a single one as grouping nodes cannot have
                int[] combination = GetGroupExprCombination(groupByNodes, groupingNodePair.Second.ChildNodes);
    
                bool found = false;
                int[] rollupIndexes = level.IsAggregationTop ? new int[0] : level.RollupKeys;
                foreach (int index in rollupIndexes) {
                    if (index == combination[0]) {
                        found = true;
                        break;
                    }
                }
    
                int result = found ? 0 : 1;
                var constant = new ExprConstantNodeImpl(result, typeof(int?));
                if (groupingNodePair.First != null) {
                    ExprNodeUtility.ReplaceChildNode(groupingNodePair.First, groupingNodePair.Second, constant);
                } else {
                    exprNode = constant;
                }
            }
    
            // rewrite grouping id expressions
            for (Pair<ExprNode, ExprGroupingIdNode> groupingIdNodePair : groupingVisitor.GroupingIdNodes) {
                int[] combination = GetGroupExprCombination(groupByNodes, groupingIdNodePair.Second.ChildNodes);
    
                int result = 0;
                for (int i = 0; i < combination.Length; i++) {
                    int index = combination[i];
    
                    bool found = false;
                    int[] rollupIndexes = level.IsAggregationTop ? new int[0] : level.RollupKeys;
                    foreach (int rollupIndex in rollupIndexes) {
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
                    ExprNodeUtility.ReplaceChildNode(groupingIdNodePair.First, groupingIdNodePair.Second, constant);
                } else {
                    exprNode = constant;
                }
            }
    
            // rewrite properties
            var identVisitor = new ExprNodeIdentifierCollectVisitorWContainer();
            exprNode.Accept(identVisitor);
            for (Pair<ExprNode, ExprIdentNode> node : identVisitor.ExprProperties) {
                bool rewrite = false;
    
                ExprNodePropOrStreamExprDesc firstRollupNonPropExpr = rolledupProps.FirstExpression;
                if (firstRollupNonPropExpr != null) {
                    throw new ExprValidationException("Invalid rollup expression " + firstRollupNonPropExpr.Textual);
                }
    
                foreach (ExprNodePropOrStreamDesc rolledupProp in rolledupProps.Properties) {
                    ExprNodePropOrStreamPropDesc prop = (ExprNodePropOrStreamPropDesc) rolledupProp;
                    if (rolledupProp.StreamNum == node.Second.StreamId && prop.PropertyName.Equals(node.Second.ResolvedPropertyName)) {
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
    
                var constant = new ExprConstantNodeImpl(null, node.Second.ExprEvaluator.Type);
                if (node.First != null) {
                    ExprNodeUtility.ReplaceChildNode(node.First, node.Second, constant);
                } else {
                    exprNode = constant;
                }
            }
    
            return ExprNodeUtility.GetValidatedSubtree(exprNodeOrigin, exprNode, validationContext);
        }
    
        private static int[] GetGroupExprCombination(ExprNode[] groupByNodes, ExprNode[] childNodes)
                {
            var indexes = new TreeSet<int?>();
            foreach (ExprNode child in childNodes) {
                bool found = false;
    
                for (int i = 0; i < groupByNodes.Length; i++) {
                    if (ExprNodeUtility.DeepEquals(child, groupByNodes[i])) {
                        if (indexes.Contains(i)) {
                            throw new ExprValidationException("Duplicate expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(child) + "' among grouping function parameters");
                        }
                        indexes.Add(i);
                        found = true;
                    }
                }
    
                if (!found) {
                    throw new ExprValidationException("Failed to find expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(child) + "' among group-by expressions");
                }
            }
            return CollectionUtil.IntArray(indexes);
        }
    
        private static void ValidateSelectAssignColNames(SelectClauseSpecCompiled selectClauseSpec, List<SelectClauseExprCompiledSpec> namedSelectionList, ExprValidationContext validationContext)
                {
            for (int i = 0; i < selectClauseSpec.SelectExprList.Length; i++) {
                // validate element
                SelectClauseElementCompiled element = selectClauseSpec.SelectExprList[i];
                if (element is SelectClauseExprCompiledSpec) {
                    SelectClauseExprCompiledSpec expr = (SelectClauseExprCompiledSpec) element;
                    ExprNode validatedExpression = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, expr.SelectExpression, validationContext);
    
                    // determine an element name if none assigned
                    string asName = expr.AssignedName;
                    if (asName == null) {
                        asName = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validatedExpression);
                    }
    
                    expr.AssignedName = asName;
                    expr.SelectExpression = validatedExpression;
                    namedSelectionList.Add(expr);
                }
            }
        }
    
        private static void ValidateHaving(ExprNodePropOrStreamSet propertiesGroupedBy,
                                           ExprNode havingNode)
                {
            var aggregateNodesHaving = new LinkedList<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(havingNode, aggregateNodesHaving);
    
            // Any non-aggregated properties must occur in the group-by clause (if there is one)
            if (!propertiesGroupedBy.IsEmpty()) {
                var visitor = new ExprNodeIdentifierAndStreamRefVisitor(true);
                havingNode.Accept(visitor);
                List<ExprNodePropOrStreamDesc> allPropertiesHaving = visitor.Refs;
                ExprNodePropOrStreamSet aggPropertiesHaving = ExprNodeUtility.GetAggregatedProperties(aggregateNodesHaving);
    
                aggPropertiesHaving.RemoveFromList(allPropertiesHaving);
                propertiesGroupedBy.RemoveFromList(allPropertiesHaving);
    
                if (!allPropertiesHaving.IsEmpty()) {
                    ExprNodePropOrStreamDesc desc = allPropertiesHaving.GetEnumerator().Next();
                    throw new ExprValidationException("Non-aggregated " + desc.Textual + " in the HAVING clause must occur in the group-by clause");
                }
            }
        }
    
        private static List<OrderByItem> ExpandColumnNames(SelectClauseElementCompiled[] selectionList, OrderByItem[] orderByUnexpanded) {
            if (orderByUnexpanded.Length == 0) {
                return Collections.EmptyList();
            }
    
            // copy list to modify
            var expanded = new List<OrderByItem>();
            foreach (OrderByItem item in orderByUnexpanded) {
                expanded.Add(item.Copy());
            }
    
            // expand
            foreach (SelectClauseElementCompiled selectElement in selectionList) {
                // process only expressions
                if (!(selectElement is SelectClauseExprCompiledSpec)) {
                    continue;
                }
                SelectClauseExprCompiledSpec selectExpr = (SelectClauseExprCompiledSpec) selectElement;
    
                string name = selectExpr.AssignedName;
                if (name != null) {
                    ExprNode fullExpr = selectExpr.SelectExpression;
                    for (ListIterator<OrderByItem> iterator = expanded.ListGetEnumerator(); iterator.HasNext(); ) {
                        OrderByItem orderByElement = iterator.Next();
                        ExprNode swapped = ColumnNamedNodeSwapper.Swap(orderByElement.ExprNode, name, fullExpr);
                        var newOrderByElement = new OrderByItem(swapped, orderByElement.IsDescending);
                        iterator.Set(newOrderByElement);
                    }
                }
            }
    
            return expanded;
        }
    
        private static int Pow2(int exponent) {
            if (exponent == 0) {
                return 1;
            }
            int result = 2;
            for (int i = 0; i < exponent - 1; i++) {
                result = 2 * result;
            }
            return result;
        }
    }
} // end of namespace
