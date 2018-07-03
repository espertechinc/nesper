///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.metric;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.timer;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// Service to manage named window dispatches, locks and processors on an engine level.
	/// </summary>
	public interface NamedWindowDispatchService
	{
	    NamedWindowProcessor CreateProcessor(
	        string name,
	        NamedWindowMgmtServiceImpl namedWindowMgmtService,
	        NamedWindowDispatchService namedWindowDispatchService, 
	        string contextName, 
	        EventType eventType, 
	        StatementResultService statementResultService, 
	        ValueAddEventProcessor revisionProcessor, 
	        string eplExpression, 
	        string statementName,
	        bool isPrioritized, 
	        bool isEnableSubqueryIndexShare, 
	        bool enableQueryPlanLog, 
	        MetricReportingService metricReportingService, 
	        bool isBatchingDataWindow, 
	        bool isVirtualDataWindow, 
	        ICollection<string> optionalUniqueKeyProps, 
	        string eventTypeAsName, 
	        StatementContext statementContextCreateWindow,
	        ILockManager lockManager);

	    NamedWindowTailView CreateTailView(
	        EventType eventType, 
	        NamedWindowMgmtService namedWindowMgmtService,
	        NamedWindowDispatchService namedWindowDispatchService,
	        StatementResultService statementResultService,
	        ValueAddEventProcessor revisionProcessor,
	        bool prioritized,
	        bool parentBatchWindow,
	        string contextName, 
	        TimeSourceService timeSourceService,
	        ConfigurationEngineDefaults.ThreadingConfig threadingConfig);

	    /// <summary>
	    /// Dispatch events of the insert and remove stream of named windows to consumers, as part of the
	    /// main event processing or dispatch loop.
	    /// </summary>
	    /// <returns>send events to consuming statements</returns>
	    bool Dispatch();

        /// <summary>
        /// For use to add a result of a named window that must be dispatched to consuming views.
        /// </summary>
        /// <param name="latchFactory">The latch factory.</param>
        /// <param name="delta">is the result to dispatch</param>
        /// <param name="consumers">is the destination of the dispatch, a map of statements to one or more consuming views</param>
        void AddDispatch(NamedWindowConsumerLatchFactory latchFactory, NamedWindowDeltaData delta, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumers);

	    /// <summary>
	    /// Dispose service.
	    /// </summary>
	    void Dispose();
	}
} // end of namespace
