///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    /// <summary>
    ///     - Each local-group-by gets its own access state factory, shared between same-local-group-by for compatible states
    /// </summary>
    public class AggregationGroupByLocalGroupByAnalyzer
    {
        public static AggregationLocalGroupByPlanForge Analyze(
            ExprForge[][] methodForges, AggregationForgeFactory[] methodFactories,
            AggregationStateFactoryForge[] accessAggregations, AggregationGroupByLocalGroupDesc localGroupDesc,
            ExprNode[] groupByExpressions, AggregationAccessorSlotPairForge[] accessors,
            ImportService importService, bool fireAndForget, string statementName)
        {
            if (groupByExpressions == null) {
                groupByExpressions = ExprNodeUtilityQuery.EMPTY_EXPR_ARRAY;
            }

            var columns = new AggregationLocalGroupByColumnForge[localGroupDesc.NumColumns];
            IList<AggregationLocalGroupByLevelForge> levelsList = new List<AggregationLocalGroupByLevelForge>();
            AggregationLocalGroupByLevelForge optionalTopLevel = null;

            // determine optional top level (level number is -1)
            for (var i = 0; i < localGroupDesc.Levels.Length; i++) {
                var levelDesc = localGroupDesc.Levels[i];
                if (levelDesc.PartitionExpr.Length == 0) {
                    optionalTopLevel = GetLevel(
                        -1, levelDesc, methodForges, methodFactories, accessAggregations, columns,
                        groupByExpressions.Length == 0, accessors, importService, fireAndForget,
                        statementName);
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
                                         groupByExpressions, levelDesc.PartitionExpr);
                if (isDefaultLevel) {
                    var level = GetLevel(
                        0, levelDesc, methodForges, methodFactories, accessAggregations, columns, isDefaultLevel,
                        accessors, importService, fireAndForget, statementName);
                    levelsList.Add(level);
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
                                         groupByExpressions, levelDesc.PartitionExpr);
                if (isDefaultLevel) {
                    continue;
                }

                var level = GetLevel(
                    levelNumber, levelDesc, methodForges, methodFactories, accessAggregations, columns, isDefaultLevel,
                    accessors, importService, fireAndForget, statementName);
                levelsList.Add(level);
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

            AggregationLocalGroupByLevelForge[] levels = levelsList.ToArray();
            return new AggregationLocalGroupByPlanForge(numMethods, numAccesses, columns, optionalTopLevel, levels);
        }

        // Obtain those method and state factories for each level
        private static AggregationLocalGroupByLevelForge GetLevel(
            int levelNumber, AggregationGroupByLocalGroupLevel level, ExprForge[][] methodForgesAll,
            AggregationForgeFactory[] methodFactoriesAll, AggregationStateFactoryForge[] accessForges,
            AggregationLocalGroupByColumnForge[] columns, bool defaultLevel,
            AggregationAccessorSlotPairForge[] accessors, ImportService importService,
            bool isFireAndForget, string statementName)
        {
            var partitionExpr = level.PartitionExpr;
            ExprForge[] partitionForges = ExprNodeUtilityQuery.GetForges(partitionExpr);

            IList<ExprForge[]> methodForges = new List<ExprForge[]>();
            IList<AggregationForgeFactory> methodFactories = new List<AggregationForgeFactory>();
            IList<AggregationStateFactoryForge> stateFactories = new List<AggregationStateFactoryForge>();

            foreach (var expr in level.Expressions) {
                int column = expr.AggregationNode.Column;
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
                    int absoluteSlot = accessors[column - methodForgesAll.Length].Slot;
                    AggregationAccessorForge accessor = accessors[column - methodForgesAll.Length].AccessorForge;
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
                    defaultLevel, partitionForges, methodOffset, methodAgg, pair, levelNumber);
            }

            return new AggregationLocalGroupByLevelForge(
                methodForges.ToArray(),
                methodFactories.ToArray(),
                stateFactories.ToArray(), partitionForges, defaultLevel);
        }
    }
} // end of namespace