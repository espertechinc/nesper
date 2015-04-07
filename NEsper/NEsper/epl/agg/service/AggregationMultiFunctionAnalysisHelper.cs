///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationMultiFunctionAnalysisHelper
    {
        // handle accessor aggregation (direct data window by-group access to properties)
        public static AggregationMultiFunctionAnalysisResult AnalyzeAccessAggregations(IList<AggregationServiceAggExpressionDesc> aggregations)
        {
            var currentSlot = 0;
            var accessProviderSlots = new ArrayDeque<AggregationMFIdentifier>();
            var accessorPairs = new List<AggregationAccessorSlotPair>();
            var stateFactories = new List<AggregationStateFactory>();
    
            foreach (var aggregation in aggregations)
            {
                var aggregateNode = aggregation.AggregationNode;
                if (!aggregateNode.Factory.IsAccessAggregation) {
                    continue;
                }
    
                var providerKey = aggregateNode.Factory.GetAggregationStateKey(false);
                var existing = FindExisting(accessProviderSlots, providerKey, aggregateNode.OptionalLocalGroupBy);

                int slot;
                if (existing == null)
                {
                    accessProviderSlots.Add(new AggregationMFIdentifier(providerKey, aggregateNode.OptionalLocalGroupBy, currentSlot));
                    slot = currentSlot++;
                    AggregationStateFactory providerFactory = aggregateNode.Factory.GetAggregationStateFactory(false);
                    stateFactories.Add(providerFactory);
                }
                else
                {
                    slot = existing.Slot;
                }

                var accessor = aggregateNode.Factory.Accessor;    
                accessorPairs.Add(new AggregationAccessorSlotPair(slot, accessor));
            }

            var pairs = accessorPairs.ToArray();
            var accessAggregations = stateFactories.ToArray();
            return new AggregationMultiFunctionAnalysisResult(pairs, accessAggregations);
        }

        internal static AggregationMFIdentifier FindExisting(IEnumerable<AggregationMFIdentifier> accessProviderSlots, AggregationStateKey providerKey, ExprAggregateLocalGroupByDesc optionalOver)
        {
            foreach (AggregationMFIdentifier ident in accessProviderSlots)
            {
                if (!Equals(providerKey, ident.AggregationStateKey)) {
                    continue;
                }
                if (optionalOver == null && ident.OptionalLocalGroupBy == null) {
                    return ident;
                }
                if (optionalOver != null &&
                    ident.OptionalLocalGroupBy != null &&
                    ExprNodeUtility.DeepEqualsIgnoreDupAndOrder(optionalOver.PartitionExpressions, ident.OptionalLocalGroupBy.PartitionExpressions)) {
                    return ident;
                }
            }
            return null;
        }

        internal class AggregationMFIdentifier
        {
            internal readonly AggregationStateKey AggregationStateKey;
            internal readonly ExprAggregateLocalGroupByDesc OptionalLocalGroupBy;
            internal readonly int Slot;

            internal AggregationMFIdentifier(AggregationStateKey aggregationStateKey, ExprAggregateLocalGroupByDesc optionalLocalGroupBy, int slot)
            {
                AggregationStateKey = aggregationStateKey;
                OptionalLocalGroupBy = optionalLocalGroupBy;
                Slot = slot;
            }
        }
    }
}
