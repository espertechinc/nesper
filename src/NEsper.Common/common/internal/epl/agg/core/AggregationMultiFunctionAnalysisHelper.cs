///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationMultiFunctionAnalysisHelper
    {
        // handle accessor aggregation (direct data window by-group access to properties)
        public static AggregationMultiFunctionAnalysisResult AnalyzeAccessAggregations(
            IList<AggregationServiceAggExpressionDesc> aggregations,
            ExprNode[] groupByNodes,
            bool join)
        {
            var currentSlot = 0;
            Deque<AggregationMFIdentifier> accessProviderSlots = new ArrayDeque<AggregationMFIdentifier>();
            IList<AggregationAccessorSlotPairForge> accessorPairsForges = new List<AggregationAccessorSlotPairForge>();
            IList<AggregationStateFactoryForge> stateFactoryForges = new List<AggregationStateFactoryForge>();

            foreach (var aggregation in aggregations) {
                var aggregateNode = aggregation.AggregationNode;
                if (!aggregateNode.Factory.IsAccessAggregation) {
                    continue;
                }

                var providerKey = aggregateNode.Factory.GetAggregationStateKey(false);
                var existing = FindExisting(
                    accessProviderSlots,
                    providerKey,
                    aggregateNode.OptionalLocalGroupBy,
                    groupByNodes);

                int slot;
                if (existing == null) {
                    accessProviderSlots.Add(
                        new AggregationMFIdentifier(providerKey, aggregateNode.OptionalLocalGroupBy, currentSlot));
                    slot = currentSlot++;
                    var
                        providerForge = aggregateNode.Factory.GetAggregationStateFactory(false, join);
                    stateFactoryForges.Add(providerForge);
                }
                else {
                    slot = existing.Slot;
                }

                var accessorForge = aggregateNode.Factory.AccessorForge;
                accessorPairsForges.Add(new AggregationAccessorSlotPairForge(slot, accessorForge));
            }

            var forges = accessorPairsForges.ToArray();
            var accessForges = stateFactoryForges.ToArray();
            return new AggregationMultiFunctionAnalysisResult(forges, accessForges);
        }

        private static AggregationMFIdentifier FindExisting(
            Deque<AggregationMFIdentifier> accessProviderSlots,
            AggregationMultiFunctionStateKey providerKey,
            ExprAggregateLocalGroupByDesc optionalOver,
            ExprNode[] groupByNodes)
        {
            foreach (var ident in accessProviderSlots) {
                if (!providerKey.Equals(ident.AggregationStateKey)) {
                    continue;
                }

                // if there is no local-group by, but there is group-by-clause, and the ident-over matches, use that
                if (optionalOver == null &&
                    groupByNodes.Length > 0 &&
                    ident.OptionalLocalGroupBy != null &&
                    ExprNodeUtilityCompare.DeepEqualsIgnoreDupAndOrder(
                        groupByNodes,
                        ident.OptionalLocalGroupBy.PartitionExpressions)) {
                    return ident;
                }

                if (optionalOver == null && ident.OptionalLocalGroupBy == null) {
                    return ident;
                }

                if (optionalOver != null &&
                    ident.OptionalLocalGroupBy != null &&
                    ExprNodeUtilityCompare.DeepEqualsIgnoreDupAndOrder(
                        optionalOver.PartitionExpressions,
                        ident.OptionalLocalGroupBy.PartitionExpressions)) {
                    return ident;
                }
            }

            return null;
        }

        private class AggregationMFIdentifier
        {
            internal AggregationMFIdentifier(
                AggregationMultiFunctionStateKey aggregationStateKey,
                ExprAggregateLocalGroupByDesc optionalLocalGroupBy,
                int slot)
            {
                AggregationStateKey = aggregationStateKey;
                OptionalLocalGroupBy = optionalLocalGroupBy;
                Slot = slot;
            }

            public AggregationMultiFunctionStateKey AggregationStateKey { get; }

            public ExprAggregateLocalGroupByDesc OptionalLocalGroupBy { get; }

            public int Slot { get; }
        }
    }
} // end of namespace