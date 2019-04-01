using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.client.metric
{
	///////////////////////////////////////////////////////////////////////////////////////
	// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
	// http://esper.codehaus.org                                                          /
	// ---------------------------------------------------------------------------------- /
	// The software in this package is published under the terms of the GPL license       /
	// a copy of which has been included with this distribution in the license.txt file.  /
	///////////////////////////////////////////////////////////////////////////////////////

	/*
	 ***************************************************************************************
	 *  Copyright (C) 2006 EsperTech, Inc. All rights reserved.                            *
	 *  http://www.espertech.com/esper                                                     *
	 *  http://www.espertech.com                                                           *
	 *  ---------------------------------------------------------------------------------- *
	 *  The software in this package is published under the terms of the GPL license       *
	 *  a copy of which has been included with this distribution in the license.txt file.  *
	 ***************************************************************************************
	 */

	/// <summary>
	/// Reports runtime-level instrumentation values.
	/// </summary>
	public class RuntimeMetric : MetricEvent {
	    private readonly long timestamp;
	    private readonly long inputCount;
	    private readonly long inputCountDelta;
	    private readonly long scheduleDepth;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="runtimeURI">runtime URI</param>
	    /// <param name="timestamp">runtime timestamp</param>
	    /// <param name="inputCount">number of input events</param>
	    /// <param name="inputCountDelta">number of input events since last</param>
	    /// <param name="scheduleDepth">schedule depth</param>
	    public RuntimeMetric(string runtimeURI, long timestamp, long inputCount, long inputCountDelta, long scheduleDepth)
	    	 : base(runtimeURI)
	    {
	        this.timestamp = timestamp;
	        this.inputCount = inputCount;
	        this.inputCountDelta = inputCountDelta;
	        this.scheduleDepth = scheduleDepth;
	    }

	    /// <summary>
	    /// Returns input count since runtime initialization, cumulative.
	    /// </summary>
	    /// <returns>input count</returns>
	    public long GetInputCount() {
	        return inputCount;
	    }

	    /// <summary>
	    /// Returns schedule depth.
	    /// </summary>
	    /// <returns>schedule depth</returns>
	    public long GetScheduleDepth() {
	        return scheduleDepth;
	    }

	    /// <summary>
	    /// Returns runtime timestamp.
	    /// </summary>
	    /// <returns>timestamp</returns>
	    public long GetTimestamp() {
	        return timestamp;
	    }

	    /// <summary>
	    /// Returns input count since last reporting period.
	    /// </summary>
	    /// <returns>input count</returns>
	    public long GetInputCountDelta() {
	        return inputCountDelta;
	    }
	}
} // end of namespace
