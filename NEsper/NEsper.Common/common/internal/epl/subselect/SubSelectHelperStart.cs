///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.subselect
{
	public class SubSelectHelperStart {

	    public static IDictionary<int, SubSelectFactoryResult> StartSubselects(IDictionary<int, SubSelectFactory> subselects, AgentInstanceContext agentInstanceContext, IList<AgentInstanceStopCallback> stopCallbacks, bool isRecoveringResilient) {
	        if (subselects == null || subselects.IsEmpty()) {
	            return Collections.EmptyMap();
	        }

	        IDictionary<int, SubSelectFactoryResult> subselectStrategies = new Dictionary<int,  SubSelectFactoryResult>();

	        foreach (KeyValuePair<int, SubSelectFactory> subselectEntry in subselects) {

	            SubSelectFactory factory = subselectEntry.Value;

	            // activate viewable
	            ViewableActivationResult subselectActivationResult = factory.Activator.Activate(agentInstanceContext, true, isRecoveringResilient);
	            stopCallbacks.Add(subselectActivationResult.StopCallback);

	            // apply returning the strategy instance
	            SubSelectStrategyRealization realization = factory.StrategyFactory.Instantiate(subselectActivationResult.Viewable, agentInstanceContext, stopCallbacks, subselectEntry.Key, isRecoveringResilient);

	            // set aggregation
	            SubordTableLookupStrategy lookupStrategyDefault = realization.LookupStrategy;
	            SubselectAggregationPreprocessorBase aggregationPreprocessor = realization.SubselectAggregationPreprocessor;

	            // determine strategy
	            SubordTableLookupStrategy lookupStrategy = lookupStrategyDefault;
	            if (aggregationPreprocessor != null) {
	                lookupStrategy = new ProxySubordTableLookupStrategy() {
	                    I<EventBean> ProcLookup = (events, context) =>  {
	                        ICollection<EventBean> matchingEvents = lookupStrategyDefault.Lookup(events, context);
	                        aggregationPreprocessor.Evaluate(events, matchingEvents, context);
	                        return CollectionUtil.SINGLE_NULL_ROW_EVENT_SET;
	                    },

	                    ProcToQueryPlan = () =>  {
	                        return lookupStrategyDefault.ToQueryPlan();
	                    },

	                    ProcGetStrategyDesc = () =>  {
	                        return lookupStrategyDefault.StrategyDesc;
	                    },
	                };
	            }

	            SubSelectFactoryResult instance = new SubSelectFactoryResult(subselectActivationResult, realization, lookupStrategy);
	            subselectStrategies.Put(subselectEntry.Key, instance);
	        }

	        return subselectStrategies;
	    }
	}
} // end of namespace