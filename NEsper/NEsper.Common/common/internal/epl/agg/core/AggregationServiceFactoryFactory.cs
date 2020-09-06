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

using Antlr4.Runtime.Misc;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.groupall;
using com.espertech.esper.common.@internal.epl.agg.groupby;
using com.espertech.esper.common.@internal.epl.agg.groupbylocal;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.agg.table;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    /// <summary>
    ///     Factory for aggregation service instances.
    ///     <para />
    ///     Consolidates aggregation nodes such that result futures point to a single instance and
    ///     no re-evaluation of the same result occurs.
    /// </summary>
    public class AggregationServiceFactoryFactory
    {
        public static AggregationServiceForgeDesc GetService(
            IList<ExprAggregateNode> selectAggregateExprNodes,
            IDictionary<ExprNode, string> selectClauseNamedNodes,
            IList<ExprDeclaredNode> declaredExpressions,
            ExprNode[] groupByNodes,
            MultiKeyClassRef groupByMultiKey,
            IList<ExprAggregateNode> havingAggregateExprNodes,
            IList<ExprAggregateNode> orderByAggregateExprNodes,
            IList<ExprAggregateNodeGroupKey> groupKeyExpressions,
            bool hasGroupByClause,
            Attribute[] annotations,
            VariableCompileTimeResolver variableCompileTimeResolver,
            bool isDisallowNoReclaim,
            ExprNode whereClause,
            ExprNode havingClause,
            EventType[] typesPerStream,
            AggregationGroupByRollupDescForge groupByRollupDesc,
            string optionalContextName,
            IntoTableSpec intoTableSpec,
            TableCompileTimeResolver tableCompileTimeResolver,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect,
            ImportServiceCompileTime importService,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            // No aggregates used, we do not need this service
            if (selectAggregateExprNodes.IsEmpty() && havingAggregateExprNodes.IsEmpty()) {
                if (intoTableSpec != null) {
                    throw new ExprValidationException("Into-table requires at least one aggregation function");
                }

                return new AggregationServiceForgeDesc(
                    AggregationServiceNullFactory.INSTANCE,
                    EmptyList<AggregationServiceAggExpressionDesc>.Instance,
                    EmptyList<ExprAggregateNodeGroupKey>.Instance,
                    EmptyList<StmtClassForgeableFactory>.Instance);
            }

            // Validate the absence of "prev" function in where-clause:
            // Since the "previous" function does not post remove stream results, disallow when used with aggregations.
            if (whereClause != null || havingClause != null) {
                var visitor = new ExprNodePreviousVisitorWParent();
                whereClause?.Accept(visitor);

                havingClause?.Accept(visitor);

                if (visitor.Previous != null && !visitor.Previous.IsEmpty()) {
                    string funcname = visitor.Previous[0]
                        .Second.PreviousType.ToString()
                        .ToLowerInvariant();
                    throw new ExprValidationException(
                        "The '" +
                        funcname +
                        "' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead");
                }
            }

            // Compile a map of aggregation nodes and equivalent-to aggregation nodes.
            // Equivalent-to functions are for example "select sum(a*b), 5*sum(a*b)".
            // Reducing the total number of aggregation functions.
            var aggregations = new List<AggregationServiceAggExpressionDesc>();
            var intoTableNonRollup = groupByRollupDesc == null && intoTableSpec != null;
            foreach (var selectAggNode in selectAggregateExprNodes) {
                AddEquivalent(selectAggNode, aggregations, intoTableNonRollup);
            }

            foreach (var havingAggNode in havingAggregateExprNodes) {
                AddEquivalent(havingAggNode, aggregations, intoTableNonRollup);
            }

            foreach (var orderByAggNode in orderByAggregateExprNodes) {
                AddEquivalent(orderByAggNode, aggregations, intoTableNonRollup);
            }

            // Construct a list of evaluation node for the aggregation functions (regular agg).
            // For example "sum(2 * 3)" would make the sum an evaluation node.
            IList<ExprForge[]> methodAggForgesList = new List<ExprForge[]>();
            foreach (var aggregation in aggregations) {
                var aggregateNode = aggregation.AggregationNode;
                if (!aggregateNode.Factory.IsAccessAggregation) {
                    var forges = aggregateNode.Factory.GetMethodAggregationForge(
                        typesPerStream.Length > 1,
                        typesPerStream);
                    methodAggForgesList.Add(forges);
                }
            }

            // determine local group-by, report when hook provided
            var localGroupDesc = AnalyzeLocalGroupBy(aggregations, groupByNodes, groupByRollupDesc, intoTableSpec);

            // determine binding
            if (intoTableSpec != null) {
                // obtain metadata
                var metadata = tableCompileTimeResolver.Resolve(intoTableSpec.Name);
                if (metadata == null) {
                    throw new ExprValidationException(
                        "Invalid into-table clause: Failed to find table by name '" + intoTableSpec.Name + "'");
                }

                EPLValidationUtil.ValidateContextName(
                    true,
                    intoTableSpec.Name,
                    metadata.OptionalContextName,
                    optionalContextName,
                    false);

                // validate group keys
                var groupByTypes = ExprNodeUtilityQuery.GetExprResultTypes(groupByNodes);
                var keyTypes = metadata.IsKeyed ? metadata.KeyTypes : new Type[0];
                ExprTableNodeUtil.ValidateExpressions(
                    intoTableSpec.Name,
                    groupByTypes,
                    "group-by",
                    groupByNodes,
                    keyTypes,
                    "group-by");

                // determine how this binds to existing aggregations, assign column numbers
                var bindingMatchResult = MatchBindingsAssignColumnNumbers(
                    intoTableSpec,
                    metadata,
                    aggregations,
                    selectClauseNamedNodes,
                    methodAggForgesList,
                    declaredExpressions,
                    importService,
                    raw.StatementName);

                // return factory
                AggregationServiceFactoryForge serviceForgeX = new AggregationServiceFactoryForgeTable(
                    metadata,
                    bindingMatchResult.MethodPairs,
                    bindingMatchResult.TargetStates,
                    bindingMatchResult.Agents,
                    groupByRollupDesc);
                return new AggregationServiceForgeDesc(serviceForgeX, aggregations, groupKeyExpressions, EmptyList<StmtClassForgeableFactory>.Instance);
            }

            // Assign a column number to each aggregation node. The regular aggregation goes first followed by access-aggregation.
            var columnNumber = 0;
            foreach (var entry in aggregations) {
                if (!entry.Factory.IsAccessAggregation) {
                    entry.SetColumnNum(columnNumber++);
                }
            }

            foreach (var entry in aggregations) {
                if (entry.Factory.IsAccessAggregation) {
                    entry.SetColumnNum(columnNumber++);
                }
            }

            // determine method aggregation factories and evaluators(non-access)
            var methodAggForges = methodAggForgesList.ToArray();
            var methodAggFactories = new AggregationForgeFactory[methodAggForges.Length];
            var count = 0;
            foreach (var aggregation in aggregations) {
                var aggregateNode = aggregation.AggregationNode;
                if (!aggregateNode.Factory.IsAccessAggregation) {
                    methodAggFactories[count] = aggregateNode.Factory;
                    count++;
                }
            }

            // handle access aggregations
            var multiFunctionAggPlan = AggregationMultiFunctionAnalysisHelper.AnalyzeAccessAggregations(
                aggregations,
                importService,
                isFireAndForget,
                raw.StatementName,
                groupByNodes);
            var accessorPairsForge = multiFunctionAggPlan.AccessorPairsForge;
            var accessFactories = multiFunctionAggPlan.StateFactoryForges;
            var hasAccessAgg = accessorPairsForge.Length > 0;
            var hasMethodAgg = methodAggFactories.Length > 0;

            AggregationServiceFactoryForge serviceForge;
            var useFlags = new AggregationUseFlags(isUnidirectional, isFireAndForget, isOnSelect);
            var additionalForgeables = new List<StmtClassForgeableFactory>();

            // analyze local group by
            AggregationLocalGroupByPlanForge localGroupByPlan = null;
            if (localGroupDesc != null) {
                AggregationLocalGroupByPlanDesc plan = AggregationGroupByLocalGroupByAnalyzer.Analyze(
                    methodAggForges,
                    methodAggFactories,
                    accessFactories,
                    localGroupDesc,
                    groupByNodes,
                    groupByMultiKey,
                    accessorPairsForge,
                    raw,
                    serdeResolver);
                localGroupByPlan = plan.Forge;
                additionalForgeables.AddAll(plan.AdditionalForgeables);

                try {
                    var hook = (AggregationLocalLevelHook) ImportUtil.GetAnnotationHook(
                        annotations,
                        HookType.INTERNAL_AGGLOCALLEVEL,
                        typeof(AggregationLocalLevelHook),
                        importService);
                    hook?.Planned(localGroupDesc, localGroupByPlan);
                }
                catch (ExprValidationException) {
                    throw new EPException("Failed to obtain hook for " + HookType.INTERNAL_AGGLOCALLEVEL);
                }
            }

            // Handle without a group-by clause: we group all into the same pot
            var rowStateDesc = new AggregationRowStateForgeDesc(
                hasMethodAgg ? methodAggFactories : null,
                hasMethodAgg ? methodAggForges : null,
                hasAccessAgg ? accessFactories : null,
                hasAccessAgg ? accessorPairsForge : null,
                useFlags);
            if (!hasGroupByClause) {
                if (localGroupByPlan != null) {
                    serviceForge = new AggSvcLocalGroupByForge(false, localGroupByPlan, useFlags);
                }
                else {
                    serviceForge = new AggregationServiceGroupAllForge(rowStateDesc);
                }
            }
            else {
                var groupDesc = new AggGroupByDesc(
                    rowStateDesc,
                    isUnidirectional,
                    isFireAndForget,
                    isOnSelect,
                    groupByNodes,
                    groupByMultiKey);
                var hasNoReclaim = HintEnum.DISABLE_RECLAIM_GROUP.GetHint(annotations) != null;
                var reclaimGroupAged = HintEnum.RECLAIM_GROUP_AGED.GetHint(annotations);
                var reclaimGroupFrequency = HintEnum.RECLAIM_GROUP_AGED.GetHint(annotations);
                if (localGroupByPlan != null) {
                    serviceForge = new AggSvcLocalGroupByForge(true, localGroupByPlan, useFlags);
                }
                else {
                    if (!isDisallowNoReclaim && hasNoReclaim) {
                        if (groupByRollupDesc != null) {
                            throw GetRollupReclaimEx();
                        }

                        serviceForge = new AggregationServiceGroupByForge(groupDesc, importService.TimeAbacus);
                    }
                    else if (!isDisallowNoReclaim && reclaimGroupAged != null) {
                        if (groupByRollupDesc != null) {
                            throw GetRollupReclaimEx();
                        }

                        CompileReclaim(
                            groupDesc,
                            reclaimGroupAged,
                            reclaimGroupFrequency,
                            variableCompileTimeResolver,
                            optionalContextName);
                        serviceForge = new AggregationServiceGroupByForge(groupDesc, importService.TimeAbacus);
                    }
                    else if (groupByRollupDesc != null) {
                        serviceForge = new AggSvcGroupByRollupForge(rowStateDesc, groupByRollupDesc, groupByNodes);
                    }
                    else {
                        groupDesc.IsRefcounted = true;
                        serviceForge = new AggregationServiceGroupByForge(groupDesc, importService.TimeAbacus);
                    }
                }
            }

            return new AggregationServiceForgeDesc(serviceForge, aggregations, groupKeyExpressions, additionalForgeables);
        }

        private static void AddEquivalent(
            ExprAggregateNode aggNodeToAdd,
            IList<AggregationServiceAggExpressionDesc> equivalencyList,
            bool intoTableNonRollup)
        {
            // Check any same aggregation nodes among all aggregation clauses
            var foundEquivalent = false;
            foreach (var existing in equivalencyList) {
                var aggNode = existing.AggregationNode;

                // we have equivalence when:
                // (a) equals on node returns true
                // (b) positional parameters are the same
                // (c) non-positional (group-by over, if present, are the same ignoring duplicates)
                if (!aggNode.EqualsNode(aggNodeToAdd, false)) {
                    continue;
                }

                if (!ExprNodeUtilityCompare.DeepEquals(
                    aggNode.PositionalParams,
                    aggNodeToAdd.PositionalParams,
                    false)) {
                    continue;
                }

                if (!ExprNodeUtilityCompare.DeepEqualsNullChecked(
                    aggNode.OptionalFilter,
                    aggNodeToAdd.OptionalFilter,
                    false)) {
                    continue;
                }

                if (aggNode.OptionalLocalGroupBy != null || aggNodeToAdd.OptionalLocalGroupBy != null) {
                    if (aggNode.OptionalLocalGroupBy == null && aggNodeToAdd.OptionalLocalGroupBy != null ||
                        aggNode.OptionalLocalGroupBy != null && aggNodeToAdd.OptionalLocalGroupBy == null) {
                        continue;
                    }

                    if (!ExprNodeUtilityCompare.DeepEqualsIgnoreDupAndOrder(
                        aggNode.OptionalLocalGroupBy.PartitionExpressions,
                        aggNodeToAdd.OptionalLocalGroupBy.PartitionExpressions)) {
                        continue;
                    }
                }

                existing.AddEquivalent(aggNodeToAdd);
                foundEquivalent = true;
                break;
            }

            if (!foundEquivalent || intoTableNonRollup) {
                equivalencyList.Add(new AggregationServiceAggExpressionDesc(aggNodeToAdd, aggNodeToAdd.Factory));
            }
        }

        private static void CompileReclaim(
            AggGroupByDesc groupDesc,
            HintAttribute reclaimGroupAged,
            HintAttribute reclaimGroupFrequency,
            VariableCompileTimeResolver variableCompileTimeResolver,
            string optionalContextName)
        {
            var hintValueMaxAge = HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(reclaimGroupAged);
            if (hintValueMaxAge == null) {
                throw new ExprValidationException(
                    "Required hint value for hint '" + HintEnum.RECLAIM_GROUP_AGED + "' has not been provided");
            }

            var evaluationFunctionMaxAge = GetEvaluationFunction(
                variableCompileTimeResolver,
                hintValueMaxAge,
                optionalContextName);
            groupDesc.IsReclaimAged = true;
            groupDesc.SetReclaimEvaluationFunctionMaxAge(evaluationFunctionMaxAge);

            var hintValueFrequency = HintEnum.RECLAIM_GROUP_FREQ.GetHintAssignedValue(reclaimGroupAged);
            AggSvcGroupByReclaimAgedEvalFuncFactoryForge evaluationFunctionFrequency;
            if (reclaimGroupFrequency == null || hintValueFrequency == null) {
                evaluationFunctionFrequency = evaluationFunctionMaxAge;
            }
            else {
                evaluationFunctionFrequency = GetEvaluationFunction(
                    variableCompileTimeResolver,
                    hintValueFrequency,
                    optionalContextName);
            }

            groupDesc.SetReclaimEvaluationFunctionFrequency(evaluationFunctionFrequency);
        }

        private static AggregationGroupByLocalGroupDesc AnalyzeLocalGroupBy(
            IList<AggregationServiceAggExpressionDesc> aggregations,
            ExprNode[] groupByNodes,
            AggregationGroupByRollupDescForge groupByRollupDesc,
            IntoTableSpec intoTableSpec)
        {
            var hasOver = false;
            foreach (var desc in aggregations) {
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

            IList<AggregationGroupByLocalGroupLevel> partitions = new List<AggregationGroupByLocalGroupLevel>();
            foreach (var desc in aggregations) {
                var localGroupBy = desc.AggregationNode.OptionalLocalGroupBy;

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
            if (partitions.Count == 1 &&
                ExprNodeUtilityCompare.DeepEqualsIgnoreDupAndOrder(partitions[0].PartitionExpr, groupByNodes)) {
                return null;
            }

            return new AggregationGroupByLocalGroupDesc(aggregations.Count, partitions.ToArray());
        }

        private static IList<AggregationServiceAggExpressionDesc> FindPartition(
            IList<AggregationGroupByLocalGroupLevel> partitions,
            ExprNode[] partitionExpressions)
        {
            foreach (var level in partitions) {
                if (ExprNodeUtilityCompare.DeepEqualsIgnoreDupAndOrder(level.PartitionExpr, partitionExpressions)) {
                    return level.Expressions;
                }
            }

            return null;
        }

        private static BindingMatchResult MatchBindingsAssignColumnNumbers(
            IntoTableSpec bindings,
            TableMetaData metadata,
            IList<AggregationServiceAggExpressionDesc> aggregations,
            IDictionary<ExprNode, string> selectClauseNamedNodes,
            IList<ExprForge[]> methodAggForgesList,
            IList<ExprDeclaredNode> declaredExpressions,
            ImportService importService,
            string statementName)
        {
            IDictionary<AggregationServiceAggExpressionDesc, TableMetadataColumnAggregation> methodAggs =
                new LinkedHashMap<AggregationServiceAggExpressionDesc, TableMetadataColumnAggregation>();
            IDictionary<AggregationServiceAggExpressionDesc, TableMetadataColumnAggregation> accessAggs =
                new LinkedHashMap<AggregationServiceAggExpressionDesc, TableMetadataColumnAggregation>();
            foreach (var aggDesc in aggregations) {
                // determine assigned name
                var columnName = FindColumnNameForAggregation(
                    selectClauseNamedNodes,
                    declaredExpressions,
                    aggDesc.AggregationNode);
                if (columnName == null) {
                    throw new ExprValidationException(
                        "Failed to find an expression among the select-clause expressions for expression '" +
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(aggDesc.AggregationNode) +
                        "'");
                }

                // determine binding metadata
                var columnMetadata = (TableMetadataColumnAggregation) metadata.Columns.Get(columnName);
                if (columnMetadata == null) {
                    throw new ExprValidationException(
                        "Failed to find name '" + columnName + "' among the columns for table '" + bindings.Name + "'");
                }

                // validate compatible
                ValidateIntoTableCompatible(bindings.Name, columnName, columnMetadata, aggDesc);

                if (columnMetadata.IsMethodAgg) {
                    methodAggs.Put(aggDesc, columnMetadata);
                }
                else {
                    accessAggs.Put(aggDesc, columnMetadata);
                }
            }

            // handle method-aggs
            var methodPairs = new TableColumnMethodPairForge[methodAggForgesList.Count];
            var methodIndex = -1;
            foreach (var methodEntry in methodAggs) {
                methodIndex++;
                var column = methodEntry.Value.Column;
                ExprForge[] forges = methodAggForgesList[methodIndex];
                methodPairs[methodIndex] = new TableColumnMethodPairForge(
                    forges,
                    column,
                    methodEntry.Key.AggregationNode);
                methodEntry.Key.SetColumnNum(column);
            }

            // handle access-aggs
            IDictionary<int, ExprNode> accessSlots = new LinkedHashMap<int, ExprNode>();
            IList<AggregationAccessorSlotPairForge> accessReadPairs = new List<AggregationAccessorSlotPairForge>();
            var accessIndex = -1;
            IList<AggregationAgentForge> agents = new List<AggregationAgentForge>();
            foreach (var accessEntry in accessAggs) {
                accessIndex++;
                var column = accessEntry.Value.Column; // Slot is zero-based as we enter with zero-offset
                var aggregationMethodFactory = accessEntry.Key.Factory;
                var accessorForge = aggregationMethodFactory.AccessorForge;
                accessSlots.Put(column, accessEntry.Key.AggregationNode);
                accessReadPairs.Add(new AggregationAccessorSlotPairForge(column, accessorForge));
                accessEntry.Key.SetColumnNum(column);
                agents.Add(aggregationMethodFactory.GetAggregationStateAgent(importService, statementName));
            }

            var agentArr = agents.ToArray();
            var accessReads = accessReadPairs.ToArray();

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
            var factoryProvided = aggDesc.Factory.AggregationPortableValidation;
            var factoryRequired = columnMetadata.AggregationPortableValidation;

            try {
                factoryRequired.ValidateIntoTableCompatible(
                    columnMetadata.AggregationExpression,
                    factoryProvided,
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(aggDesc.AggregationNode),
                    aggDesc.Factory);
            }
            catch (ExprValidationException ex) {
                var text = GetMessage(
                    tableName,
                    columnName,
                    columnMetadata.AggregationExpression,
                    aggDesc.Factory.AggregationExpression);
                throw new ExprValidationException(text + ": " + ex.Message, ex);
            }
        }

        private static string GetMessage(
            string tableName,
            string columnName,
            string aggregationRequired,
            ExprAggregateNodeBase aggregationProvided)
        {
            return "Incompatible aggregation function for table '" +
                   tableName +
                   "' column '" +
                   columnName +
                   "', expecting '" +
                   aggregationRequired +
                   "' and received '" +
                   ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(aggregationProvided) +
                   "'";
        }

        private static AggSvcGroupByReclaimAgedEvalFuncFactoryForge GetEvaluationFunction(
            VariableCompileTimeResolver variableCompileTimeResolver,
            string hintValue,
            string optionalContextName)
        {
            var variableMetaData = variableCompileTimeResolver.Resolve(hintValue);
            if (variableMetaData != null) {
                if (!variableMetaData.Type.IsNumeric()) {
                    throw new ExprValidationException(
                        "Variable type of variable '" + variableMetaData.VariableName + "' is not numeric");
                }

                var message = VariableUtil.CheckVariableContextName(optionalContextName, variableMetaData);
                if (message != null) {
                    throw new ExprValidationException(message);
                }

                return new AggSvcGroupByReclaimAgedEvalFuncFactoryVariableForge(variableMetaData);
            }

            double valueDouble;
            try {
                valueDouble = Double.Parse(hintValue);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception) {
                throw new ExprValidationException(
                    "Failed to parse hint parameter value '" +
                    hintValue +
                    "' as a double-typed seconds value or variable name");
            }

            if (valueDouble <= 0) {
                throw new ExprValidationException(
                    "Hint parameter value '" +
                    hintValue +
                    "' is an invalid value, expecting a double-typed seconds value or variable name");
            }

            return new AggSvcGroupByReclaimAgedEvalFuncFactoryConstForge(valueDouble);
        }

        public static ExprValidationException GetRollupReclaimEx()
        {
            return new ExprValidationException("Reclaim hints are not available with rollup");
        }

        private class BindingMatchResult
        {
            public BindingMatchResult(
                TableColumnMethodPairForge[] methodPairs,
                AggregationAccessorSlotPairForge[] accessors,
                int[] targetStates,
                ExprNode[] accessStateExpr,
                AggregationAgentForge[] agents)
            {
                MethodPairs = methodPairs;
                Accessors = accessors;
                TargetStates = targetStates;
                AccessStateExpr = accessStateExpr;
                Agents = agents;
            }

            public TableColumnMethodPairForge[] MethodPairs { get; }

            public AggregationAccessorSlotPairForge[] Accessors { get; }

            public int[] TargetStates { get; }

            public AggregationAgentForge[] Agents { get; }

            public ExprNode[] AccessStateExpr { get; }
        }
    }
} // end of namespace