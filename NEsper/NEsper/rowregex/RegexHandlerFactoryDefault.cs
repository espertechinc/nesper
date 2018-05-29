///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.spec;
using com.espertech.esper.view;

namespace com.espertech.esper.rowregex
{
	public class RegexHandlerFactoryDefault : RegexHandlerFactory
	{
	    private readonly IContainer _container;

	    public RegexHandlerFactoryDefault(IContainer container)
	    {
	        _container = container;
	    }

	    public EventRowRegexNFAViewFactory MakeViewFactory(
	        ViewFactoryChain viewFactoryChain,
	        MatchRecognizeSpec matchRecognizeSpec,
	        AgentInstanceContext agentInstanceContext,
	        bool isUnbound, 
	        Attribute[] annotations,
	        ConfigurationEngineDefaults.MatchRecognizeConfig matchRecognizeConfigs)
        {
	        return new EventRowRegexNFAViewFactory(
	            _container, 
	            viewFactoryChain, 
	            matchRecognizeSpec,
	            agentInstanceContext, 
	            isUnbound, 
	            annotations, 
	            matchRecognizeConfigs);
	    }

	    public RegexPartitionStateRepo MakeSingle(RegexPartitionStateRandomAccessGetter prevGetter, AgentInstanceContext agentInstanceContext, EventRowRegexNFAView view, bool keepScheduleState, RegexPartitionTerminationStateComparator terminationStateCompare)
        {
	        return new RegexPartitionStateRepoNoGroup(prevGetter, keepScheduleState, terminationStateCompare);
	    }

	    public RegexPartitionStateRepo MakePartitioned(RegexPartitionStateRandomAccessGetter prevGetter, RegexPartitionStateRepoGroupMeta stateRepoGroupMeta, AgentInstanceContext agentInstanceContext, EventRowRegexNFAView view, bool keepScheduleState, RegexPartitionTerminationStateComparator terminationStateCompare)
        {
	        return new RegexPartitionStateRepoGroup(prevGetter, stateRepoGroupMeta, keepScheduleState, terminationStateCompare);
	    }
	}
} // end of namespace
