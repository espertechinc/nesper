///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    /// <summary>
    ///     - Each local-group-by gets its own access state factory, shared between same-local-group-by for compatible states
    /// </summary>
    public class AggregationGroupByLocalGroupByAnalyzer
    {
        public static AggregationLocalGroupByPlanDesc Analyze(
            ExprForge[][] methodForges,
            AggregationForgeFactory[] methodFactories,
            AggregationStateFactoryForge[] accessAggregations,
            AggregationGroupByLocalGroupDesc localGroupDesc,
            ExprNode[] groupByExpressions,
            MultiKeyClassRef groupByMultiKey,
            AggregationAccessorSlotPairForge[] accessors,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            if (groupByExpressions == null) {
                groupByExpressions = ExprNodeUtilityQuery.EMPTY_EXPR_ARRAY;
            }

            var columns = new AggregationLocalGroupByColumnForge[localGroupDesc.NumColumns];
            var levelsList = new List<AggregationLocalGroupByLevelForge>();
            AggregationLocalGroupByLevelForge optionalTopLevel = null;
            var additionalForgeables = new List<StmtClassForgeableFactory>();

            // determine optional top level (level number is -1)
            for (var i = 0; i < localGroupDesc.Levels.Length; i++) {
                var levelDesc = localGroupDesc.Levels[i];
                if (levelDesc.PartitionExpr.Length == 0) {
                    var top = GetLevel(
                        -1,
                        levelDesc,
                        methodForges,
                        methodFactories,
                        accessAggregations,
                        columns,
                        groupByExpressions.Length == 0,
                        accessors,
                        groupByExpressions,
                        groupByMultiKey,
                        raw,
                        serdeResolver);
                    optionalTopLevel = top.Forge;
                    additionalForgeables.AddRange(top.AdditionalForgeables);
                }
            }

            // determine default (same as group-by) level, if any, assign level number 0
            var levelNumber = 0;
            for (var i = 0; i < localGroupDesc.Levels.Length; i++) {
                var levelDesc = localGroupDesc.Levels[i];
                if (levelDesc.PartitionExpr.Length == 0) {
                    continue;
                }

                var isDefaultLevel = groupByExpressions != null &&
                                     ExprNodeUtilityCompare.DeepEqualsIgnoreDupAndOrder(
                                         groupByExpressions,
                                         levelDesc.PartitionExpr);

                if (isDefaultLevel) {
                    var level = GetLevel(
                        0,
                        levelDesc,
                        methodForges,
                        methodFactories,
                        accessAggregations,
                        columns,
                        isDefaultLevel,
                        accessors,
                        groupByExpressions,
                        groupByMultiKey,
                        raw,
                        serdeResolver);
                    additionalForgeables.AddRange(level.AdditionalForgeables);
                    levelsList.Add(level.Forge);
                    levelNumber++;
                    break;
                }
            }

            // determine all other levels, assign level numbers
            for (var i = 0; i < localGroupDesc.Levels.Length; i++) {
                var levelDesc = localGroupDesc.Levels[i];
                if (levelDesc.PartitionExpr.Length == 0) {
                    continue;
                }

                var isDefaultLevel = groupByExpressions != null &&
                                     ExprNodeUtilityCompare.DeepEqualsIgnoreDupAndOrder(
                                         groupByExpressions,
                                         levelDesc.PartitionExpr);
                if (isDefaultLevel) {
                    continue;
                }

                var level = GetLevel(
                    levelNumber,
                    levelDesc,
                    methodForges,
                    methodFactories,
                    accessAggregations,
                    columns,
                    isDefaultLevel,
                    accessors,
                    groupByExpressions,
                    groupByMultiKey,
                    raw,
                    serdeResolver);
                levelsList.Add(level.Forge);
                additionalForgeables.AddRange(level.AdditionalForgeables);
                levelNumber++;
            }

            // totals
            var numMethods = 0;
            var numAccesses = 0;
            if (optionalTopLevel != null) {
                numMethods += optionalTopLevel.MethodFactories.Length;
                numAccesses += optionalTopLevel.AccessStateForges.Length;
            }

            foreach (var level in levelsList) {
                numMethods += level.MethodFactories.Length;
                numAccesses += level.AccessStateForges.Length;
            }

            var levels = levelsList.ToArray();
            var forge = new AggregationLocalGroupByPlanForge(
                numMethods,
                numAccesses,
                columns,
                optionalTopLevel,
                levels);
            return new AggregationLocalGroupByPlanDesc(forge, additionalForgeables);
        }

        // Obtain those method and state factories for each level
        private static AggregationGroupByLocalGroupLevelDesc GetLevel(
            int levelNumber,
            AggregationGroupByLocalGroupLevel level,
            ExprForge[][] methodForgesAll,
            AggregationForgeFactory[] methodFactoriesAll,
            AggregationStateFactoryForge[] accessForges,
            AggregationLocalGroupByColumnForge[] columns,
            bool defaultLevel,
            AggregationAccessorSlotPairForge[] accessors,
            ExprNode[] groupByExpressions,
            MultiKeyClassRef optionalGroupByMultiKey,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            var partitionExpr = level.PartitionExpr;
            MultiKeyPlan multiKeyPlan;
            if (defaultLevel && optionalGroupByMultiKey != null) { // use default multi-key that is already generated
                multiKeyPlan = new MultiKeyPlan(EmptyList<StmtClassForgeableFactory>.Instance, optionalGroupByMultiKey);
                partitionExpr = groupByExpressions;
            }
            else {
                multiKeyPlan = MultiKeyPlanner.PlanMultiKey(partitionExpr, false, raw, serdeResolver);
            }

            IList<ExprForge[]> methodForges = new List<ExprForge[]>();
            IList<AggregationForgeFactory> methodFactories = new List<AggregationForgeFactory>();
            IList<AggregationStateFactoryForge> stateFactories = new List<AggregationStateFactoryForge>();

            foreach (var expr in level.Expressions) {
                var column = expr.AggregationNode.Column;
                var methodOffset = -1;
                var methodAgg = true;
                AggregationAccessorSlotPairForge pair = null;

                if (column < methodForgesAll.Length) {
                    methodForges.Add(methodForgesAll[column]);
                    methodFactories.Add(methodFactoriesAll[column]);
                    methodOffset = methodFactories.Count - 1;
                }
                else {
                    // slot gives us the number of the state factory
                    var absoluteSlot = accessors[column - methodForgesAll.Length].Slot;
                    var accessor = accessors[column - methodForgesAll.Length].AccessorForge;
                    var factory = accessForges[absoluteSlot];
                    var relativeSlot = stateFactories.IndexOf(factory);
                    if (relativeSlot == -1) {
                        stateFactories.Add(factory);
                        relativeSlot = stateFactories.Count - 1;
                    }

                    methodAgg = false;
                    pair = new AggregationAccessorSlotPairForge(relativeSlot, accessor);
                }

                columns[column] = new AggregationLocalGroupByColumnForge(
                    defaultLevel,
                    partitionExpr,
                    methodOffset,
                    methodAgg,
                    pair,
                    levelNumber);
            }

            var forge = new AggregationLocalGroupByLevelForge(
                methodForges.ToArray(),
                methodFactories.ToArray(),
                stateFactories.ToArray(),
                partitionExpr,
                multiKeyPlan.ClassRef,
                defaultLevel);

            // NOTE: The original code tests for multiKeyPlan being null, but if it was null it would have caused
            // a null pointer exception on the previous statement since it dereferences ClassRef.
            var additionalForgeables =
                multiKeyPlan?.MultiKeyForgeables ?? EmptyList<StmtClassForgeableFactory>.Instance;

            return new AggregationGroupByLocalGroupLevelDesc(forge, additionalForgeables);
        }
    }
} // end of namespace