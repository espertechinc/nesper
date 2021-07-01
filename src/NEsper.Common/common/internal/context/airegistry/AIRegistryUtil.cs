///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryUtil
    {
        public static StatementAIResourceRegistry AllocateRegistries(
            AIRegistryRequirements registryRequirements,
            AIRegistryFactory factory)
        {
            AIRegistryPriorEvalStrategy[] priorEvalStrategies = null;
            if (registryRequirements.PriorFlagsPerStream != null) {
                bool[] priorFlagPerStream = registryRequirements.PriorFlagsPerStream;
                priorEvalStrategies = new AIRegistryPriorEvalStrategy[priorFlagPerStream.Length];
                for (var i = 0; i < priorEvalStrategies.Length; i++) {
                    if (priorFlagPerStream[i]) {
                        priorEvalStrategies[i] = factory.MakePrior();
                    }
                }
            }

            AIRegistryPreviousGetterStrategy[] previousGetterStrategies = null;
            if (registryRequirements.PreviousFlagsPerStream != null) {
                bool[] previousFlagPerStream = registryRequirements.PreviousFlagsPerStream;
                previousGetterStrategies = new AIRegistryPreviousGetterStrategy[previousFlagPerStream.Length];
                for (var i = 0; i < previousGetterStrategies.Length; i++) {
                    if (previousFlagPerStream[i]) {
                        previousGetterStrategies[i] = factory.MakePrevious();
                    }
                }
            }

            IDictionary<int, AIRegistrySubqueryEntry> subselects = null;
            if (registryRequirements.Subqueries != null) {
                AIRegistryRequirementSubquery[] requirements = registryRequirements.Subqueries;
                subselects = new Dictionary<int, AIRegistrySubqueryEntry>();
                for (var i = 0; i < requirements.Length; i++) {
                    var lookup = factory.MakeSubqueryLookup(requirements[i].LookupStrategyDesc);
                    var aggregation = requirements[i].HasAggregation ? factory.MakeAggregation() : null;
                    var prior = requirements[i].HasPrior ? factory.MakePrior() : null;
                    var prev = requirements[i].HasPrev ? factory.MakePrevious() : null;
                    subselects.Put(i, new AIRegistrySubqueryEntry(lookup, aggregation, prior, prev));
                }
            }

            IDictionary<int, AIRegistryTableAccess> tableAccesses = null;
            if (registryRequirements.TableAccessCount > 0) {
                tableAccesses = new Dictionary<int, AIRegistryTableAccess>();
                for (var i = 0; i < registryRequirements.TableAccessCount; i++) {
                    var strategy = factory.MakeTableAccess();
                    tableAccesses.Put(i, strategy);
                }
            }

            AIRegistryRowRecogPreviousStrategy rowRecogPreviousStrategy = null;
            if (registryRequirements.IsRowRecogWithPrevious) {
                rowRecogPreviousStrategy = factory.MakeRowRecogPreviousStrategy();
            }

            return new StatementAIResourceRegistry(
                factory.MakeAggregation(),
                priorEvalStrategies,
                subselects,
                tableAccesses,
                previousGetterStrategies,
                rowRecogPreviousStrategy);
        }

        public static void AssignFutures(
            StatementAIResourceRegistry aiResourceRegistry,
            int agentInstanceId,
            AggregationService optionalAggegationService,
            PriorEvalStrategy[] optionalPriorStrategies,
            PreviousGetterStrategy[] optionalPreviousGetters,
            IDictionary<int, SubSelectFactoryResult> subselects,
            IDictionary<int, ExprTableEvalStrategy> tableAccessStrategies,
            RowRecogPreviousStrategy rowRecogPreviousStrategy)
        {
            // assign aggregation service
            if (optionalAggegationService != null) {
                aiResourceRegistry.AgentInstanceAggregationService.AssignService(
                    agentInstanceId,
                    optionalAggegationService);
            }

            // assign prior-strategies
            if (optionalPriorStrategies != null) {
                for (var i = 0; i < optionalPriorStrategies.Length; i++) {
                    if (optionalPriorStrategies[i] != null) {
                        aiResourceRegistry.AgentInstancePriorEvalStrategies[i]
                            .AssignService(
                                agentInstanceId,
                                optionalPriorStrategies[i]);
                    }
                }
            }

            // assign prior-strategies
            if (optionalPreviousGetters != null) {
                for (var i = 0; i < optionalPreviousGetters.Length; i++) {
                    if (optionalPreviousGetters[i] != null) {
                        aiResourceRegistry.AgentInstancePreviousGetterStrategies[i]
                            .AssignService(
                                agentInstanceId,
                                optionalPreviousGetters[i]);
                    }
                }
            }

            // assign subqueries
            foreach (var subselectEntry in subselects) {
                var registryEntry = aiResourceRegistry.AgentInstanceSubselects.Get(subselectEntry.Key);
                var subq = subselectEntry.Value;
                registryEntry.Assign(
                    agentInstanceId,
                    subq.LookupStrategy,
                    subq.AggregationService,
                    subq.PriorStrategy,
                    subq.PreviousStrategy);
            }

            // assign table access strategies
            foreach (var tableEntry in tableAccessStrategies) {
                var registryEntry = aiResourceRegistry.AgentInstanceTableAccesses.Get(tableEntry.Key);
                var evalStrategy = tableEntry.Value;
                registryEntry.AssignService(agentInstanceId, evalStrategy);
            }

            // assign match-recognize previous strategy
            if (rowRecogPreviousStrategy != null) {
                aiResourceRegistry.AgentInstanceRowRecogPreviousStrategy.AssignService(
                    agentInstanceId,
                    rowRecogPreviousStrategy);
            }
        }

        public static void CheckExpand<T>(
            int serviceId,
            ArrayWrap<T> services)
        {
            if (serviceId > services.Array.Length - 1) {
                var delta = serviceId - services.Array.Length + 1;
                services.Expand(delta);
            }
        }
    }
} // end of namespace