///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly EPStageEventServiceImpl _epRuntime;
		private readonly EventBean _theEvent;
		private object _callbackList;
		private EPStatementAgentInstanceHandle _handle;
		private readonly long _filterVersion;

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
			this._epRuntime = epRuntime;
			this._callbackList = callbackList;
			this._theEvent = theEvent;
			this._handle = handle;
			this._filterVersion = filterVersion;
		}

		public void Run()
		{
			try {
				_epRuntime.ProcessStatementFilterMultiple(_handle, _callbackList, _theEvent, _filterVersion, 0);

				_epRuntime.Dispatch();

				_epRuntime.ProcessThreadWorkQueue();
			}
			catch (Exception e) {
				Log.Error("Unexpected error processing multiple route execution: " + e.Message, e);
			}
		}
	}
} // end of namespace
