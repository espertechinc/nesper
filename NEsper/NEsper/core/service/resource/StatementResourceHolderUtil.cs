///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.pattern;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service.resource
{
	public class StatementResourceHolderUtil
    {
	    public static StatementResourceHolder PopulateHolder(StatementAgentInstanceFactoryResult startResult)
        {
	        StatementResourceHolder holder = new StatementResourceHolder(startResult.AgentInstanceContext);

	        if (startResult is StatementAgentInstanceFactorySelectResult) {
	            StatementAgentInstanceFactorySelectResult selectResult = (StatementAgentInstanceFactorySelectResult) startResult;
	            holder.TopViewables = selectResult.TopViews;
	            holder.EventStreamViewables = selectResult.EventStreamViewables;
	            holder.PatternRoots = selectResult.PatternRoots;
	            holder.AggregationService = selectResult.OptionalAggegationService;
	            holder.SubselectStrategies = selectResult.SubselectStrategies;
	            holder.PostLoad = selectResult.OptionalPostLoadJoin;
	        }
	        else if (startResult is StatementAgentInstanceFactoryCreateWindowResult) {
	            StatementAgentInstanceFactoryCreateWindowResult createResult = (StatementAgentInstanceFactoryCreateWindowResult) startResult;
	            holder.TopViewables = new Viewable[] {createResult.TopView};
	            holder.PostLoad = createResult.PostLoad;
	            holder.NamedWindowProcessorInstance = createResult.ProcessorInstance;
	        }
	        else if (startResult is StatementAgentInstanceFactoryCreateTableResult) {
	            StatementAgentInstanceFactoryCreateTableResult createResult = (StatementAgentInstanceFactoryCreateTableResult) startResult;
	            holder.TopViewables = new Viewable[] {createResult.FinalView};
	            holder.AggregationService = createResult.OptionalAggegationService;
	        }
	        else if (startResult is StatementAgentInstanceFactoryOnTriggerResult) {
	            StatementAgentInstanceFactoryOnTriggerResult onTriggerResult = (StatementAgentInstanceFactoryOnTriggerResult) startResult;
	            holder.PatternRoots = new EvalRootState[] {onTriggerResult.OptPatternRoot};
	            holder.AggregationService = onTriggerResult.OptionalAggegationService;
	            holder.SubselectStrategies = onTriggerResult.SubselectStrategies;
	        }
	        return holder;
	    }
	}
} // end of namespace
