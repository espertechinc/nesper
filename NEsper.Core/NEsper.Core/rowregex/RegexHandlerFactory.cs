///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.spec;
using com.espertech.esper.view;

namespace com.espertech.esper.rowregex
{
	/// <summary>
	/// Service for creating match-recognize factory and state services.
	/// </summary>
	public interface RegexHandlerFactory
	{
	    EventRowRegexNFAViewFactory MakeViewFactory(ViewFactoryChain viewFactoryChain, MatchRecognizeSpec matchRecognizeSpec, AgentInstanceContext agentInstanceContext, bool isUnbound, Attribute[] annotations, ConfigurationEngineDefaults.MatchRecognizeConfig matchRecognizeConfigs) ;
	    RegexPartitionStateRepo MakeSingle(RegexPartitionStateRandomAccessGetter prevGetter, AgentInstanceContext agentInstanceContext, EventRowRegexNFAView view, bool keepScheduleState, RegexPartitionTerminationStateComparator terminationStateCompare);
	    RegexPartitionStateRepo MakePartitioned(RegexPartitionStateRandomAccessGetter prevGetter, RegexPartitionStateRepoGroupMeta stateRepoGroupMeta, AgentInstanceContext agentInstanceContext, EventRowRegexNFAView view, bool keepScheduleState, RegexPartitionTerminationStateComparator terminationStateCompare);
	}
} // end of namespace
