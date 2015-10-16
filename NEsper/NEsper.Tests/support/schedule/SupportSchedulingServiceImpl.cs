///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.compat.logging;
using com.espertech.esper.schedule;

namespace com.espertech.esper.support.schedule
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
            //To change body of implemented methods use File | Settings | File Templates.
        }
    
        public void EvaluateUnLock()
        {
            //To change body of implemented methods use File | Settings | File Templates.
        }
    
        public void Add(long afterMSec, ScheduleHandle callback, ScheduleSlot slot)
        {
            log.Debug(".add Not implemented, afterMSec=" + afterMSec + " callback=" + callback.GetType().FullName);
            _added.Put(afterMSec, callback);
        }
    
        public void Remove(ScheduleHandle callback, ScheduleSlot slot)
        {
            log.Debug(".remove Not implemented, callback=" + callback.GetType().FullName);
        }

        public long Time
        {
            get
            {
                log.Debug(".getTime Time is " + _currentTime);
                return this._currentTime;
            }
            set
            {
                log.Debug(".setTime Setting new time, currentTime=" + value);
                this._currentTime = value;
            }
        }

        public void Evaluate(ICollection<ScheduleHandle> handles)
        {
            log.Debug(".evaluate Not implemented");
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
            ;
        }

        public int TimeHandleCount
        {
            get { throw new ApplicationException("not implemented"); }
        }

        public long? FurthestTimeHandle
        {
            get { throw new ApplicationException("not implemented"); }
        }

        public int ScheduleHandleCount
        {
            get { throw new ApplicationException("not implemented"); }
        }

        public bool IsScheduled(ScheduleHandle scheduleHandle)
        {
            return false;  //To change body of implemented methods use File | Settings | File Templates.
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
