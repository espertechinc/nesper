///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.stage;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
	/// <summary>
	/// Route execution work unit.
	/// </summary>
	public class RouteUnitMultipleStaged : RouteUnitRunnable
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly EPStageEventServiceImpl epRuntime;
		private readonly EventBean theEvent;
		private object callbackList;
		private EPStatementAgentInstanceHandle handle;
		private readonly long filterVersion;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="epRuntime">runtime to process</param>
		/// <param name="callbackList">callback list</param>
		/// <param name="theEvent">event to pass</param>
		/// <param name="handle">statement handle</param>
		/// <param name="filterVersion">version of filter</param>
		public RouteUnitMultipleStaged(
			EPStageEventServiceImpl epRuntime,
			object callbackList,
			EventBean theEvent,
			EPStatementAgentInstanceHandle handle,
			long filterVersion)
		{
			this.epRuntime = epRuntime;
			this.callbackList = callbackList;
			this.theEvent = theEvent;
			this.handle = handle;
			this.filterVersion = filterVersion;
		}

		public void Run()
		{
			try {
				epRuntime.ProcessStatementFilterMultiple(handle, callbackList, theEvent, filterVersion, 0);

				epRuntime.Dispatch();

				epRuntime.ProcessThreadWorkQueue();
			}
			catch (Exception e) {
				log.Error("Unexpected error processing multiple route execution: " + e.Message, e);
			}
		}
	}
} // end of namespace
