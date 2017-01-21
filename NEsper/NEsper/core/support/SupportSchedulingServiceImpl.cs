///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.support
{
	public class SupportSchedulingServiceImpl : SchedulingService
	{
	    private readonly IDictionary<long, ScheduleHandle> _added = new Dictionary<long, ScheduleHandle>();
	    private long _currentTime;

	    public IDictionary<long, ScheduleHandle> Added
	    {
	        get { return _added; }
	    }

	    public void EvaluateLock()
	    {
	    }

	    public void EvaluateUnLock()
	    {
	    }

	    public void Add(long afterMSec, ScheduleHandle callback, long slot)
	    {
	        Log.Debug(".add Not implemented, afterMSec=" + afterMSec + " callback=" + callback.GetType().Name);
	        _added.Put(afterMSec, callback);
	    }

	    public void Remove(ScheduleHandle callback, long scheduleSlot)
	    {
	        Log.Debug(".remove Not implemented, callback=" + callback.GetType().Name);
	    }

	    public long Time
	    {
	        get
	        {
	            Log.Debug(".getTime Time is " + _currentTime);
	            return this._currentTime;
	        }
	        set
	        {
	            Log.Debug(".setTime Setting new time, currentTime=" + value);
	            this._currentTime = value;
	        }
	    }

	    public void Evaluate(ICollection<ScheduleHandle> handles)
	    {
	        Log.Debug(".evaluate Not implemented");
	    }

	    public ScheduleBucket AllocateBucket()
	    {
	        return new ScheduleBucket(0);
	    }

	    public static void EvaluateSchedule(SchedulingService service)
	    {
	        ICollection<ScheduleHandle> handles = new LinkedList<ScheduleHandle>();
	        service.Evaluate(handles);

	        foreach (ScheduleHandle handle in handles)
	        {
	            if (handle is EPStatementHandleCallback)
	            {
	                EPStatementHandleCallback callback = (EPStatementHandleCallback) handle;
	                callback.ScheduleCallback.ScheduledTrigger(null);
	            }
	            else
	            {
	                ScheduleHandleCallback cb = (ScheduleHandleCallback) handle;
	                cb.ScheduledTrigger(null);
	            }
	        }
	    }

	    public void Dispose()
	    {
	    }

	    public int TimeHandleCount
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public long? FurthestTimeHandle
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public int ScheduleHandleCount
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public bool IsScheduled(ScheduleHandle scheduleHandle)
	    {
	        return false;
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
