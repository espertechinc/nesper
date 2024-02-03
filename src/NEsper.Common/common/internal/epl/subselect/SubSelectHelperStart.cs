///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectHelperStart
    {
        public static IDictionary<int, SubSelectFactoryResult> StartSubselects(
            IDictionary<int, SubSelectFactory> subselects,
            ExprEvaluatorContext exprEvaluatorContext,
            AgentInstanceContext agentInstanceContextOpt,
            IList<AgentInstanceMgmtCallback> stopCallbacks,
            bool isRecoveringResilient)
        {
            if (subselects == null || subselects.IsEmpty()) {
                return EmptyDictionary<int, SubSelectFactoryResult>.Instance;
            }

            IDictionary<int, SubSelectFactoryResult>
                subselectStrategies = new Dictionary<int, SubSelectFactoryResult>();

            foreach (var subselectEntry in subselects) {
                var factory = subselectEntry.Value;

                // activate viewable
                var subselectActivationResult = factory.Activator.Activate(
                    agentInstanceContextOpt,
                    true,
                    isRecoveringResilient);
                stopCallbacks.Add(subselectActivationResult.StopCallback);

                // apply returning the strategy instance
                var realization = factory.StrategyFactory.Instantiate(
                    subselectActivationResult.Viewable,
                    exprEvaluatorContext,
                    stopCallbacks,
                    subselectEntry.Key,
                    isRecoveringResilient);

                // set aggregation
                var lookupStrategyDefault = realization.LookupStrategy;
                var aggregationPreprocessor = realization.SubselectAggregationPreprocessor;

                // determine strategy
                var lookupStrategy = lookupStrategyDefault;
                if (aggregationPreprocessor != null) {
                    lookupStrategy = new ProxySubordTableLookupStrategy {
                        ProcLookup = (
                            events,
                            context) => {
                            var matchingEvents = lookupStrategyDefault.Lookup(events, context);
                            aggregationPreprocessor.Evaluate(events, matchingEvents, context);
                            return CollectionUtil.SINGLE_NULL_ROW_EVENT_SET;
                        },

                        ProcToQueryPlan = () => { return lookupStrategyDefault.ToQueryPlan(); },

                        ProcStrategyDesc = () => { return lookupStrategyDefault.StrategyDesc; }
                    };
                }

                var instance = new SubSelectFactoryResult(subselectActivationResult, realization, lookupStrategy);
                subselectStrategies.Put(subselectEntry.Key, instance);
            }

            return subselectStrategies;
        }
    }
} // end of namespace