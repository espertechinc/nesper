///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.util
{
    /// <summary>
    ///     - Each local-group-by gets its own access state factory, shared between same-local-group-by for compatible states
    /// </summary>
    public class AggregationGroupByLocalGroupByAnalyzer
    {
        public static AggregationLocalGroupByPlan Analyze(
            ExprEvaluator[] evaluators,
            AggregationMethodFactory[] prototypes,
            AggregationStateFactory[] accessAggregations,
            AggregationGroupByLocalGroupDesc localGroupDesc,
            ExprNode[] groupByExpressions,
            AggregationAccessorSlotPair[] accessors)
        {
            if (groupByExpressions == null)
            {
                groupByExpressions = ExprNodeUtility.EMPTY_EXPR_ARRAY;
            }

            var columns = new AggregationLocalGroupByColumn[localGroupDesc.NumColumns];
            IList<AggregationLocalGroupByLevel> levelsList = new List<AggregationLocalGroupByLevel>();
            AggregationLocalGroupByLevel optionalTopLevel = null;

            // determine optional top level (level number is -1)
            for (int i = 0; i < localGroupDesc.Levels.Length; i++)
            {
                AggregationGroupByLocalGroupLevel levelDesc = localGroupDesc.Levels[i];
                if (levelDesc.PartitionExpr.Length == 0)
                {
                    optionalTopLevel = GetLevel(
                        -1, levelDesc, evaluators, prototypes, accessAggregations, columns,
                        groupByExpressions.Length == 0, accessors);
                }
            }

            // determine default (same as group-by) level, if any, assign level number 0
            int levelNumber = 0;
            for (int i = 0; i < localGroupDesc.Levels.Length; i++)
            {
                AggregationGroupByLocalGroupLevel levelDesc = localGroupDesc.Levels[i];
                if (levelDesc.PartitionExpr.Length == 0)
                {
                    continue;
                }
                var isDefaultLevel = groupByExpressions != null && ExprNodeUtility.DeepEqualsIgnoreDupAndOrder(groupByExpressions, levelDesc.PartitionExpr);
                if (isDefaultLevel)
                {
                    AggregationLocalGroupByLevel level = GetLevel(
                        0, levelDesc, evaluators, prototypes, accessAggregations, columns, isDefaultLevel, accessors);
                    levelsList.Add(level);
                    levelNumber++;
                    break;
                }
            }

            // determine all other levels, assign level numbers
            for (int i = 0; i < localGroupDesc.Levels.Length; i++)
            {
                AggregationGroupByLocalGroupLevel levelDesc = localGroupDesc.Levels[i];
                if (levelDesc.PartitionExpr.Length == 0)
                {
                    continue;
                }
                bool isDefaultLevel = groupByExpressions != null &&
                                      ExprNodeUtility.DeepEqualsIgnoreDupAndOrder(
                                          groupByExpressions, levelDesc.PartitionExpr);
                if (isDefaultLevel)
                {
                    continue;
                }
                AggregationLocalGroupByLevel level = GetLevel(
                    levelNumber, levelDesc, evaluators, prototypes, accessAggregations, columns, isDefaultLevel,
                    accessors);
                levelsList.Add(level);
                levelNumber++;
            }

            // totals
            int numMethods = 0;
            int numAccesses = 0;
            if (optionalTopLevel != null)
            {
                numMethods += optionalTopLevel.MethodFactories.Length;
                numAccesses += optionalTopLevel.StateFactories.Length;
            }
            foreach (AggregationLocalGroupByLevel level in levelsList)
            {
                numMethods += level.MethodFactories.Length;
                numAccesses += level.StateFactories.Length;
            }

            AggregationLocalGroupByLevel[] levels = levelsList.ToArray();
            return new AggregationLocalGroupByPlan(numMethods, numAccesses, columns, optionalTopLevel, levels);
        }

        // Obtain those method and state factories for each level
        private static AggregationLocalGroupByLevel GetLevel(
            int levelNumber,
            AggregationGroupByLocalGroupLevel level,
            ExprEvaluator[] methodEvaluatorsAll,
            AggregationMethodFactory[] methodFactoriesAll,
            AggregationStateFactory[] stateFactoriesAll,
            AggregationLocalGroupByColumn[] columns,
            bool defaultLevel,
            AggregationAccessorSlotPair[] accessors)
        {
            ExprNode[] partitionExpr = level.PartitionExpr;
            ExprEvaluator[] partitionEvaluators = ExprNodeUtility.GetEvaluators(partitionExpr);

            IList<ExprEvaluator> methodEvaluators = new List<ExprEvaluator>();
            IList<AggregationMethodFactory> methodFactories = new List<AggregationMethodFactory>();
            IList<AggregationStateFactory> stateFactories = new List<AggregationStateFactory>();

            foreach (AggregationServiceAggExpressionDesc expr in level.Expressions)
            {
                int column = expr.ColumnNum.Value;
                int methodOffset = -1;
                bool methodAgg = true;
                AggregationAccessorSlotPair pair = null;

                if (column < methodEvaluatorsAll.Length)
                {
                    methodEvaluators.Add(methodEvaluatorsAll[column]);
                    methodFactories.Add(methodFactoriesAll[column]);
                    methodOffset = methodFactories.Count - 1;
                }
                else
                {
                    // slot gives us the number of the state factory
                    int absoluteSlot = accessors[column - methodEvaluatorsAll.Length].Slot;
                    AggregationAccessor accessor = accessors[column - methodEvaluatorsAll.Length].Accessor;
                    AggregationStateFactory factory = stateFactoriesAll[absoluteSlot];
                    int relativeSlot = stateFactories.IndexOf(factory);
                    if (relativeSlot == -1)
                    {
                        stateFactories.Add(factory);
                        relativeSlot = stateFactories.Count - 1;
                    }
                    methodAgg = false;
                    pair = new AggregationAccessorSlotPair(relativeSlot, accessor);
                }
                columns[column] = new AggregationLocalGroupByColumn(
                    defaultLevel, partitionEvaluators, methodOffset, methodAgg, pair, levelNumber);
            }

            return new AggregationLocalGroupByLevel(
                methodEvaluators.ToArray(),
                methodFactories.ToArray(),
                stateFactories.ToArray(), partitionEvaluators, defaultLevel);
        }
    }
} // end of namespace