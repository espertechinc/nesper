///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.util;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.agg.service
{
	/// <summary>
	/// Factory for aggregation service instances.
	/// <para />Consolidates aggregation nodes such that result futures point to a single instance and
	/// no re-evaluation of the same result occurs.
	/// </summary>
	public class AggregationServiceFactoryFactory
	{
        /// <summary>
        /// Produces an aggregation service for use with match-recognice.
        /// </summary>
        /// <param name="numStreams">number of streams</param>
        /// <param name="measureExprNodesPerStream">measure nodes</param>
        /// <param name="typesPerStream">The types per stream.</param>
        /// <returns>
        /// service
        /// </returns>
	    public static AggregationServiceMatchRecognizeFactoryDesc GetServiceMatchRecognize(
	        int numStreams,
	        IDictionary<int, IList<ExprAggregateNode>> measureExprNodesPerStream,
	        EventType[] typesPerStream)
	    {
	        var equivalencyListPerStream = new SortedDictionary<int, IList<AggregationServiceAggExpressionDesc>>();

	        foreach (var entry in measureExprNodesPerStream)
	        {
	            IList<AggregationServiceAggExpressionDesc> equivalencyList = new List<AggregationServiceAggExpressionDesc>();
	            equivalencyListPerStream.Put(entry.Key, equivalencyList);
	            foreach (var selectAggNode in entry.Value)
	            {
	                AddEquivalent(selectAggNode, equivalencyList);
	            }
	        }

	        var aggregatorsPerStream = new LinkedHashMap<int, AggregationMethodFactory[]>();
	        var evaluatorsPerStream = new Dictionary<int, ExprEvaluator[]>();

	        foreach (var equivalencyPerStream in equivalencyListPerStream)
	        {
	            var index = 0;
	            int stream = equivalencyPerStream.Key;

	            var aggregators = new AggregationMethodFactory[equivalencyPerStream.Value.Count];
	            aggregatorsPerStream.Put(stream, aggregators);

	            var evaluators = new ExprEvaluator[equivalencyPerStream.Value.Count];
	            evaluatorsPerStream.Put(stream, evaluators);

	            foreach (var aggregation in equivalencyPerStream.Value)
	            {
	                var aggregateNode = aggregation.AggregationNode;
	                if (aggregateNode.ChildNodes.Length > 1)
	                {
                        evaluators[index] = ExprMethodAggUtil.GetMultiNodeEvaluator(aggregateNode.ChildNodes, typesPerStream.Length > 1, typesPerStream);
	                }
	                else if (aggregateNode.ChildNodes.Length > 0)
	                {
	                    // Use the evaluation node under the aggregation node to obtain the aggregation value
	                    evaluators[index] = aggregateNode.ChildNodes[0].ExprEvaluator;
	                }
	                // For aggregation that doesn't evaluate any particular sub-expression, return null on evaluation
	                else
	                {
	                    evaluators[index] = new ProxyExprEvaluator
                        {
	                        ProcEvaluate = args => null,
	                        ProcReturnType = () => null
	                    };
	                }

	                aggregators[index] = aggregateNode.Factory;
	                index++;
	            }
	        }

	        // Assign a column number to each aggregation node. The regular aggregation goes first followed by access-aggregation.
	        var columnNumber = 0;
	        IList<AggregationServiceAggExpressionDesc> allExpressions = new List<AggregationServiceAggExpressionDesc>();
	        foreach (var equivalencyPerStream in equivalencyListPerStream)
	        {
	            foreach (var entry in equivalencyPerStream.Value)
	            {
	                entry.ColumnNum = columnNumber++;
	            }
	            allExpressions.AddAll(equivalencyPerStream.Value);
	        }

	        AggregationServiceMatchRecognizeFactory factory = new AggregationServiceMatchRecognizeFactoryImpl(numStreams, aggregatorsPerStream, evaluatorsPerStream);
	        return new AggregationServiceMatchRecognizeFactoryDesc(factory, allExpressions);
	    }

	    /// <summary>
	    /// Returns an instance to handle the aggregation required by the aggregation expression nodes, depending on
	    /// whether there are any group-by nodes.
	    /// </summary>
	    /// <param name="selectAggregateExprNodes">aggregation nodes extracted out of the select expression</param>
	    /// <param name="selectClauseNamedNodes">The select clause named nodes.</param>
	    /// <param name="declaredExpressions">The declared expressions.</param>
	    /// <param name="groupByNodes">The group by nodes.</param>
	    /// <param name="havingAggregateExprNodes">aggregation nodes extracted out of the select expression</param>
	    /// <param name="orderByAggregateExprNodes">aggregation nodes extracted out of the select expression</param>
	    /// <param name="groupKeyExpressions">The group key expressions.</param>
	    /// <param name="hasGroupByClause">indicator on whethere there is group-by required, or group-all</param>
	    /// <param name="annotations">statement annotations</param>
	    /// <param name="variableService">variable</param>
	    /// <param name="isJoin">true for joins</param>
	    /// <param name="isDisallowNoReclaim">if set to <c>true</c> [is disallow no reclaim].</param>
	    /// <param name="whereClause">the where-clause function if any</param>
	    /// <param name="havingClause">the having-clause function if any</param>
	    /// <param name="factoryService">The factory service.</param>
	    /// <param name="typesPerStream">The types per stream.</param>
	    /// <param name="groupByRollupDesc">The group by rollup desc.</param>
	    /// <param name="optionalContextName">Name of the optional context.</param>
	    /// <param name="intoTableSpec">The into table spec.</param>
	    /// <param name="tableService">The table service.</param>
	    /// <param name="isUnidirectional">if set to <c>true</c> [is unidirectional].</param>
	    /// <param name="isFireAndForget">if set to <c>true</c> [is fire and forget].</param>
	    /// <param name="isOnSelect">if set to <c>true</c> [is on select].</param>
	    /// <returns>
	    /// instance for aggregation handling
	    /// </returns>
	    /// <exception cref="ExprValidationException">
	    /// Into-table requires at least one aggregation function
	    /// or
	    /// The ' + funcname + ' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead
	    /// or
	    /// Invalid into-table clause: Failed to find table by name ' + intoTableSpec.Name + '
	    /// </exception>
	    /// <exception cref="EPException">Failed to obtain hook for  + HookType.INTERNAL_AGGLOCALLEVEL</exception>
	    /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException if validation fails</throws>
	    public static AggregationServiceFactoryDesc GetService(IList<ExprAggregateNode> selectAggregateExprNodes, IDictionary<ExprNode, string> selectClauseNamedNodes, IList<ExprDeclaredNode> declaredExpressions, ExprNode[] groupByNodes, IList<ExprAggregateNode> havingAggregateExprNodes, IList<ExprAggregateNode> orderByAggregateExprNodes, IList<ExprAggregateNodeGroupKey> groupKeyExpressions, bool hasGroupByClause, Attribute[] annotations, VariableService variableService, bool isJoin, bool isDisallowNoReclaim, ExprNode whereClause, ExprNode havingClause, AggregationServiceFactoryService factoryService, EventType[] typesPerStream, AggregationGroupByRollupDesc groupByRollupDesc, string optionalContextName, IntoTableSpec intoTableSpec, TableService tableService, bool isUnidirectional, bool isFireAndForget, bool isOnSelect)
	    {
	        // No aggregates used, we do not need this service
	        if ((selectAggregateExprNodes.IsEmpty()) && (havingAggregateExprNodes.IsEmpty()))
	        {
	            if (intoTableSpec != null) {
	                throw new ExprValidationException("Into-table requires at least one aggregation function");
	            }

	            return new AggregationServiceFactoryDesc(
	                factoryService.GetNullAggregationService(),
	                Collections.GetEmptyList<AggregationServiceAggExpressionDesc>(),
	                Collections.GetEmptyList<ExprAggregateNodeGroupKey>());
	        }

	        // Validate the absence of "prev" function in where-clause:
	        // Since the "previous" function does not post remove stream results, disallow when used with aggregations.
	        if ((whereClause != null) || (havingClause != null)) {
	            var visitor = new ExprNodePreviousVisitorWParent();
	            if (whereClause != null) {
	                whereClause.Accept(visitor);
	            }
	            if (havingClause != null) {
	                havingClause.Accept(visitor);
	            }
	            if ((visitor.Previous != null) && (!visitor.Previous.IsEmpty())) {
	                string funcname = visitor.Previous[0].Second.PreviousType.ToString().ToLower();
	                throw new ExprValidationException("The '" + funcname + "' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead");
	            }
	        }

	        // Compile a map of aggregation nodes and equivalent-to aggregation nodes.
	        // Equivalent-to functions are for example "select sum(a*b), 5*sum(a*b)".
	        // Reducing the total number of aggregation functions.
	        IList<AggregationServiceAggExpressionDesc> aggregations = new List<AggregationServiceAggExpressionDesc>();
	        foreach (var selectAggNode in selectAggregateExprNodes)
	        {
	            AddEquivalent(selectAggNode, aggregations);
	        }
	        foreach (var havingAggNode in havingAggregateExprNodes)
	        {
	            AddEquivalent(havingAggNode, aggregations);
	        }
	        foreach (var orderByAggNode in orderByAggregateExprNodes)
	        {
	            AddEquivalent(orderByAggNode, aggregations);
	        }

	        // Construct a list of evaluation node for the aggregation functions (regular agg).
	        // For example "sum(2 * 3)" would make the sum an evaluation node.
	        IList<ExprEvaluator> methodAggEvaluatorsList = new List<ExprEvaluator>();
	        foreach (var aggregation in aggregations) {
	            var aggregateNode = aggregation.AggregationNode;
	            if (!aggregateNode.Factory.IsAccessAggregation) {
                    var evaluator = aggregateNode.Factory.GetMethodAggregationEvaluator(
                        typesPerStream.Length > 1, typesPerStream);
	                methodAggEvaluatorsList.Add(evaluator);
	            }
	        }

            // determine local group-by, report when hook provided
            AggregationGroupByLocalGroupDesc localGroupDesc = AnalyzeLocalGroupBy(aggregations, groupByNodes, groupByRollupDesc, intoTableSpec);

	        // determine binding
	        if (intoTableSpec != null) {
	            // obtain metadata
	            var metadata = tableService.GetTableMetadata(intoTableSpec.Name);
	            if (metadata == null) {
	                throw new ExprValidationException("Invalid into-table clause: Failed to find table by name '" + intoTableSpec.Name + "'");
	            }

	            EPLValidationUtil.ValidateContextName(true, intoTableSpec.Name, metadata.ContextName, optionalContextName, false);

	            // validate group keys
	            var groupByTypes = ExprNodeUtility.GetExprResultTypes(groupByNodes);
	            ExprTableNodeUtil.ValidateExpressions(intoTableSpec.Name, groupByTypes, "group-by", groupByNodes,
	                    metadata.KeyTypes, "group-by");

	            // determine how this binds to existing aggregations, assign column numbers
	            var bindingMatchResult = MatchBindingsAssignColumnNumbers(intoTableSpec, metadata, aggregations, selectClauseNamedNodes, methodAggEvaluatorsList, declaredExpressions);

	            // return factory
	            AggregationServiceFactory aggregationServiceFactory;
	            if (!hasGroupByClause) {
	                aggregationServiceFactory = factoryService.GetNoGroupWBinding(bindingMatchResult.Accessors, isJoin, bindingMatchResult.MethodPairs, intoTableSpec.Name, bindingMatchResult.TargetStates, bindingMatchResult.AccessStateExpr, bindingMatchResult.Agents);
	            }
	            else {
	                aggregationServiceFactory = factoryService.GetGroupWBinding(metadata, bindingMatchResult.MethodPairs, bindingMatchResult.Accessors, isJoin, intoTableSpec, bindingMatchResult.TargetStates, bindingMatchResult.AccessStateExpr, bindingMatchResult.Agents, groupByRollupDesc);
	            }
	            return new AggregationServiceFactoryDesc(aggregationServiceFactory, aggregations, groupKeyExpressions);
	        }

	        // Assign a column number to each aggregation node. The regular aggregation goes first followed by access-aggregation.
	        var columnNumber = 0;
	        foreach (var entry in aggregations)
	        {
	            if (!entry.Factory.IsAccessAggregation) {
	                entry.ColumnNum = columnNumber++;
	            }
	        }
	        foreach (var entry in aggregations)
	        {
	            if (entry.Factory.IsAccessAggregation) {
	                entry.ColumnNum = columnNumber++;
	            }
	        }

	        // determine method aggregation factories and evaluators(non-access)
	        ExprEvaluator[] methodAggEvaluators = methodAggEvaluatorsList.ToArray();
	        var methodAggFactories = new AggregationMethodFactory[methodAggEvaluators.Length];
	        var count = 0;
	        foreach (var aggregation in aggregations) {
	            var aggregateNode = aggregation.AggregationNode;
	            if (!aggregateNode.Factory.IsAccessAggregation) {
	                methodAggFactories[count] = aggregateNode.Factory;
	                count++;
	            }
	        }

	        // handle access aggregations
	        var multiFunctionAggPlan = AggregationMultiFunctionAnalysisHelper.AnalyzeAccessAggregations(aggregations);
	        var accessorPairs = multiFunctionAggPlan.AccessorPairs;
	        var accessAggregations = multiFunctionAggPlan.StateFactories;

	        AggregationServiceFactory serviceFactory;

            // analyze local group by
            AggregationLocalGroupByPlan localGroupByPlan = null;
            if (localGroupDesc != null) {
                localGroupByPlan = AggregationGroupByLocalGroupByAnalyzer.Analyze(methodAggEvaluators, methodAggFactories, accessAggregations, localGroupDesc, groupByNodes, accessorPairs);
                try {
                    AggregationLocalLevelHook hook = (AggregationLocalLevelHook) TypeHelper.GetAnnotationHook(annotations, HookType.INTERNAL_AGGLOCALLEVEL, typeof(AggregationLocalLevelHook), null);
                    if (hook != null) {
                        hook.Planned(localGroupDesc, localGroupByPlan);
                    }
                }
                catch (ExprValidationException e) {
                    throw new EPException("Failed to obtain hook for " + HookType.INTERNAL_AGGLOCALLEVEL);
                }
            }

	        // Handle without a group-by clause: we group all into the same pot
	        if (!hasGroupByClause) {
                if (localGroupByPlan != null) {
                    serviceFactory = factoryService.GetNoGroupLocalGroupBy(isJoin, localGroupByPlan, isUnidirectional, isFireAndForget, isOnSelect);
                }
	            else if ((methodAggEvaluators.Length > 0) && (accessorPairs.Length == 0)) {
                    serviceFactory = factoryService.GetNoGroupNoAccess(methodAggEvaluators, methodAggFactories, isUnidirectional, isFireAndForget, isOnSelect);
	            }
	            else if ((methodAggEvaluators.Length == 0) && (accessorPairs.Length > 0)) {
                    serviceFactory = factoryService.GetNoGroupAccessOnly(accessorPairs, accessAggregations, isJoin, isUnidirectional, isFireAndForget, isOnSelect);
	            }
	            else {
                    serviceFactory = factoryService.GetNoGroupAccessMixed(methodAggEvaluators, methodAggFactories, accessorPairs, accessAggregations, isJoin, isUnidirectional, isFireAndForget, isOnSelect);
	            }
	        }
	        else {
	            var hasNoReclaim = HintEnum.DISABLE_RECLAIM_GROUP.GetHint(annotations) != null;
	            var reclaimGroupAged = HintEnum.RECLAIM_GROUP_AGED.GetHint(annotations);
	            var reclaimGroupFrequency = HintEnum.RECLAIM_GROUP_AGED.GetHint(annotations);
                if (localGroupByPlan != null) {
                    serviceFactory = factoryService.GetGroupLocalGroupBy(isJoin, localGroupByPlan, isUnidirectional, isFireAndForget, isOnSelect);
                }
                else
                {
                    if (!isDisallowNoReclaim && hasNoReclaim)
                    {
                        if (groupByRollupDesc != null)
                        {
                            throw GetRollupReclaimEx();
                        }
                        if ((methodAggEvaluators.Length > 0) && (accessorPairs.Length == 0))
                        {
                            serviceFactory = factoryService.GetGroupedNoReclaimNoAccess(groupByNodes, methodAggEvaluators, methodAggFactories, isUnidirectional, isFireAndForget, isOnSelect);
                        }
                        else if ((methodAggEvaluators.Length == 0) && (accessorPairs.Length > 0))
                        {
                            serviceFactory = factoryService.GetGroupNoReclaimAccessOnly(groupByNodes, accessorPairs, accessAggregations, isJoin, isUnidirectional, isFireAndForget, isOnSelect);
                        }
                        else
                        {
                            serviceFactory = factoryService.GetGroupNoReclaimMixed(groupByNodes, methodAggEvaluators, methodAggFactories, accessorPairs, accessAggregations, isJoin, isUnidirectional, isFireAndForget, isOnSelect);
                        }
                    }
                    else if (!isDisallowNoReclaim && reclaimGroupAged != null)
                    {
                        if (groupByRollupDesc != null)
                        {
                            throw GetRollupReclaimEx();
                        }
                        serviceFactory = factoryService.GetGroupReclaimAged(groupByNodes, methodAggEvaluators, methodAggFactories, reclaimGroupAged, reclaimGroupFrequency, variableService, accessorPairs, accessAggregations, isJoin, optionalContextName, isUnidirectional, isFireAndForget, isOnSelect);
                    }
                    else if (groupByRollupDesc != null)
                    {
                        serviceFactory = factoryService.GetGroupReclaimMixableRollup(groupByNodes, groupByRollupDesc, methodAggEvaluators, methodAggFactories, accessorPairs, accessAggregations, isJoin, groupByRollupDesc, isUnidirectional, isFireAndForget, isOnSelect);
                    }
                    else
                    {
                        if ((methodAggEvaluators.Length > 0) && (accessorPairs.Length == 0))
                        {
                            serviceFactory = factoryService.GetGroupReclaimNoAccess(groupByNodes, methodAggEvaluators, methodAggFactories, accessorPairs, accessAggregations, isJoin, isUnidirectional, isFireAndForget, isOnSelect);
                        }
                        else
                        {
                            serviceFactory = factoryService.GetGroupReclaimMixable(groupByNodes, methodAggEvaluators, methodAggFactories, accessorPairs, accessAggregations, isJoin, isUnidirectional, isFireAndForget, isOnSelect);
                        }
                    }
                }
	        }

	        return new AggregationServiceFactoryDesc(serviceFactory, aggregations, groupKeyExpressions);
	    }

	    private static AggregationGroupByLocalGroupDesc AnalyzeLocalGroupBy(
	        IList<AggregationServiceAggExpressionDesc> aggregations,
	        ExprNode[] groupByNodes,
	        AggregationGroupByRollupDesc groupByRollupDesc,
	        IntoTableSpec intoTableSpec)
        {
            var hasOver = false;
            foreach (AggregationServiceAggExpressionDesc desc in aggregations) {
                if (desc.AggregationNode.OptionalLocalGroupBy != null) {
                    hasOver = true;
                    break;
                }
            }
            if (!hasOver) {
                return null;
            }
            if (groupByRollupDesc != null) {
                throw new ExprValidationException("Roll-up and group-by parameters cannot be combined");
            }
            if (intoTableSpec != null) {
                throw new ExprValidationException("Into-table and group-by parameters cannot be combined");
            }

            var partitions = new List<AggregationGroupByLocalGroupLevel>();
            foreach (AggregationServiceAggExpressionDesc desc in aggregations) {
                ExprAggregateLocalGroupByDesc localGroupBy = desc.AggregationNode.OptionalLocalGroupBy;

                var partitionExpressions = localGroupBy == null ? groupByNodes : localGroupBy.PartitionExpressions;
                var found = FindPartition(partitions, partitionExpressions);
                if (found == null) {
                    found = new List<AggregationServiceAggExpressionDesc>();
                    var level = new AggregationGroupByLocalGroupLevel(partitionExpressions, found);
                    partitions.Add(level);
                }
                found.Add(desc);
            }

            // check single group-by partition and it matches the group-by clause
            if (partitions.Count == 1 && ExprNodeUtility.DeepEqualsIgnoreDupAndOrder(partitions[0].PartitionExpr, groupByNodes)) {
                return null;
            }
            return new AggregationGroupByLocalGroupDesc(aggregations.Count, partitions.ToArray());
        }

	    private static IList<AggregationServiceAggExpressionDesc> FindPartition(
	        IEnumerable<AggregationGroupByLocalGroupLevel> partitions,
	        ExprNode[] partitionExpressions)
        {
            foreach (AggregationGroupByLocalGroupLevel level in partitions) {
                if (ExprNodeUtility.DeepEqualsIgnoreDupAndOrder(level.PartitionExpr, partitionExpressions)) {
                    return level.Expressions;
                }
            }
            return null;
        }

	    private static BindingMatchResult MatchBindingsAssignColumnNumbers(
	        IntoTableSpec bindings,
	        TableMetadata metadata,
	        IList<AggregationServiceAggExpressionDesc> aggregations,
	        IDictionary<ExprNode, string> selectClauseNamedNodes,
	        IList<ExprEvaluator> methodAggEvaluatorsList,
	        IList<ExprDeclaredNode> declaredExpressions)
	    {
	        IDictionary<AggregationServiceAggExpressionDesc, TableMetadataColumnAggregation> methodAggs = new LinkedHashMap<AggregationServiceAggExpressionDesc, TableMetadataColumnAggregation>();
	        IDictionary<AggregationServiceAggExpressionDesc, TableMetadataColumnAggregation> accessAggs = new LinkedHashMap<AggregationServiceAggExpressionDesc, TableMetadataColumnAggregation>();
	        foreach (var aggDesc in aggregations) {

	            // determine assigned name
	            var columnName = FindColumnNameForAggregation(selectClauseNamedNodes, declaredExpressions, aggDesc.AggregationNode);
	            if (columnName == null) {
	                throw new ExprValidationException("Failed to find an expression among the select-clause expressions for expression '" + aggDesc.AggregationNode.ToExpressionStringMinPrecedenceSafe() + "'");
	            }

	            // determine binding metadata
	            var columnMetadata = (TableMetadataColumnAggregation) metadata.TableColumns.Get(columnName);
	            if (columnMetadata == null) {
	                throw new ExprValidationException("Failed to find name '" + columnName + "' among the columns for table '" + bindings.Name + "'");
	            }

	            // validate compatible
	            ValidateIntoTableCompatible(bindings.Name, columnName, columnMetadata, aggDesc);

	            if (!columnMetadata.Factory.IsAccessAggregation) {
	                methodAggs.Put(aggDesc, columnMetadata);
	            }
	            else {
	                accessAggs.Put(aggDesc, columnMetadata);
	            }
	        }

	        // handle method-aggs
	        var methodPairs = new TableColumnMethodPair[methodAggEvaluatorsList.Count];
	        var methodIndex = -1;
	        foreach (var methodEntry in methodAggs) {
	            methodIndex++;
	            var targetIndex = methodEntry.Value.MethodOffset;
	            methodPairs[methodIndex] = new TableColumnMethodPair(methodAggEvaluatorsList[methodIndex], targetIndex, methodEntry.Key.AggregationNode);
	            methodEntry.Key.ColumnNum = targetIndex;
	        }

	        // handle access-aggs
	        var accessSlots = new LinkedHashMap<int, ExprNode>();
	        var accessReadPairs = new List<AggregationAccessorSlotPair>();
	        var accessIndex = -1;
	        var agents = new List<AggregationAgent>();
	        foreach (var accessEntry in accessAggs) {
	            accessIndex++;
	            var slot = accessEntry.Value.AccessAccessorSlotPair.Slot;
	            var aggregationMethodFactory = accessEntry.Key.Factory;
	            var accessor = aggregationMethodFactory.Accessor;
	            accessSlots.Put(slot, accessEntry.Key.AggregationNode);
	            accessReadPairs.Add(new AggregationAccessorSlotPair(slot, accessor));
	            accessEntry.Key.ColumnNum = metadata.NumberMethodAggregations + accessIndex;
	            agents.Add(aggregationMethodFactory.AggregationStateAgent);
	        }
	        AggregationAgent[] agentArr= agents.ToArray();
	        AggregationAccessorSlotPair[] accessReads = accessReadPairs.ToArray();

	        var targetStates = new int[accessSlots.Count];
	        var accessStateExpr = new ExprNode[accessSlots.Count];
	        var count = 0;
	        foreach (var entry in accessSlots) {
	            targetStates[count] = entry.Key;
	            accessStateExpr[count] = entry.Value;
	            count++;
	        }

	        return new BindingMatchResult(methodPairs, accessReads, targetStates, accessStateExpr, agentArr);
	    }

	    private static string FindColumnNameForAggregation(
	        IDictionary<ExprNode, string> selectClauseNamedNodes,
	        IList<ExprDeclaredNode> declaredExpressions,
	        ExprAggregateNode aggregationNode)
        {
	        if (selectClauseNamedNodes.ContainsKey(aggregationNode)) {
	            return selectClauseNamedNodes.Get(aggregationNode);
	        }

	        foreach (var node in declaredExpressions) {
	            if (node.Body == aggregationNode) {
	                return node.Prototype.Name;
	            }
	        }

	        return null;
	    }

	    private static void ValidateIntoTableCompatible(
	        string tableName,
	        string columnName,
	        TableMetadataColumnAggregation columnMetadata,
	        AggregationServiceAggExpressionDesc aggDesc)
	    {
	        var factoryProvided = aggDesc.Factory;
	        var factoryRequired = columnMetadata.Factory;

	        try {
	            factoryRequired.ValidateIntoTableCompatible(factoryProvided);
	        }
	        catch (ExprValidationException ex) {
	            var text = GetMessage(tableName, columnName, factoryRequired.AggregationExpression, factoryProvided.AggregationExpression);
	            throw new ExprValidationException(text + ": " + ex.Message, ex);
	        }
	    }

	    private static string GetMessage(
	        string tableName,
	        string columnName,
	        ExprAggregateNodeBase aggregationRequired,
	        ExprAggregateNodeBase aggregationProvided)
	    {
	        return "Incompatible aggregation function for table '" +
	            tableName +
	            "' column '" +
	            columnName + "', expecting '" +
	            aggregationRequired.ToExpressionStringMinPrecedenceSafe() +
	            "' and received '" +
	            aggregationProvided.ToExpressionStringMinPrecedenceSafe() +
	            "'";
	    }

	    private static void AddEquivalent(ExprAggregateNode aggNodeToAdd, IList<AggregationServiceAggExpressionDesc> equivalencyList)
	    {
	        // Check any same aggregation nodes among all aggregation clauses
	        var foundEquivalent = false;
	        foreach (var existing in equivalencyList)
	        {
	            var aggNode = existing.AggregationNode;

                // we have equivalence when:
                // (a) equals on node returns true
                // (b) positional parameters are the same
                // (c) non-positional (group-by over, if present, are the same ignoring duplicates)

                if (!aggNode.EqualsNode(aggNodeToAdd))
                {
                    continue;
                }
                if (!ExprNodeUtility.DeepEquals(aggNode.PositionalParams, aggNodeToAdd.PositionalParams))
                {
                    continue;
                }
                if (aggNode.OptionalLocalGroupBy != null || aggNodeToAdd.OptionalLocalGroupBy != null)
                {
                    if ((aggNode.OptionalLocalGroupBy == null && aggNodeToAdd.OptionalLocalGroupBy != null) ||
                        (aggNode.OptionalLocalGroupBy != null && aggNodeToAdd.OptionalLocalGroupBy == null))
                    {
                        continue;
                    }
                    if (!ExprNodeUtility.DeepEqualsIgnoreDupAndOrder(aggNode.OptionalLocalGroupBy.PartitionExpressions, aggNodeToAdd.OptionalLocalGroupBy.PartitionExpressions))
                    {
                        continue;
                    }
                }

                existing.AddEquivalent(aggNodeToAdd);
                foundEquivalent = true;
                break;
	        }

	        if (!foundEquivalent)
	        {
	            equivalencyList.Add(new AggregationServiceAggExpressionDesc(aggNodeToAdd, aggNodeToAdd.Factory));
	        }
	    }

	    public static ExprValidationException GetRollupReclaimEx()
        {
	        return new ExprValidationException("Reclaim hints are not available with rollup");
	    }

	    internal class BindingMatchResult
        {
	        internal BindingMatchResult(TableColumnMethodPair[] methodPairs, AggregationAccessorSlotPair[] accessors, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents) 
            {
	            MethodPairs = methodPairs;
	            Accessors = accessors;
	            TargetStates = targetStates;
	            AccessStateExpr = accessStateExpr;
	            Agents = agents;
	        }

	        public TableColumnMethodPair[] MethodPairs { get; private set; }

	        public AggregationAccessorSlotPair[] Accessors { get; private set; }

	        public int[] TargetStates { get; private set; }

	        public AggregationAgent[] Agents { get; private set; }

	        public ExprNode[] AccessStateExpr { get; private set; }
        }
	}
} // end of namespace
