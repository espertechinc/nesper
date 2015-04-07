///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client.annotation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    public class SchedulingServiceAudit : SchedulingServiceSPI
    {
        private readonly String _engineUri;
        private readonly String _statementName;
        private readonly SchedulingServiceSPI _spi;
    
        public SchedulingServiceAudit(String engineUri, String statementName, SchedulingServiceSPI spi)
        {
            _engineUri = engineUri;
            _statementName = statementName;
            _spi = spi;
        }
    
        public bool IsScheduled(ScheduleHandle handle)
        {
            return _spi.IsScheduled(handle);
        }
    
        public ScheduleSet Take(ICollection<String> statementId)
        {
            return _spi.Take(statementId);
        }
    
        public void Apply(ScheduleSet scheduleSet)
        {
            _spi.Apply(scheduleSet);
        }

        public long? NearestTimeHandle
        {
            get { return _spi.NearestTimeHandle; }
        }

        public void VisitSchedules(ScheduleVisitor visitor)
        {
            _spi.VisitSchedules(visitor);
        }
    
        public void Add(long afterMSec, ScheduleHandle handle, ScheduleSlot slot)
        {
            if (AuditPath.IsInfoEnabled) {
                StringWriter message = new StringWriter();
                message.Write("after ");
                message.Write(afterMSec);
                message.Write(" handle ");
                PrintHandle(message, handle);
    
                AuditPath.AuditLog(_engineUri, _statementName, AuditEnum.SCHEDULE, message.ToString());
    
                ModifyCreateProxy(handle);
            }
            _spi.Add(afterMSec, handle, slot);
        }
    
        public void Remove(ScheduleHandle handle, ScheduleSlot slot)
        {
            if (AuditPath.IsInfoEnabled) {
                StringWriter message = new StringWriter();
                message.Write("remove handle ");
                PrintHandle(message, handle);
    
                AuditPath.AuditLog(_engineUri, _statementName, AuditEnum.SCHEDULE, message.ToString());
            }
            _spi.Remove(handle, slot);
        }

        public long Time
        {
            set { _spi.Time = value; }
            get { return _spi.Time; }
        }

        public void Evaluate(ICollection<ScheduleHandle> handles)
        {
            _spi.Evaluate(handles);
        }
    
        public void Dispose()
        {
            _spi.Dispose();
        }

        public int TimeHandleCount
        {
            get { return _spi.TimeHandleCount; }
        }

        public long? FurthestTimeHandle
        {
            get { return _spi.FurthestTimeHandle; }
        }

        public int ScheduleHandleCount
        {
            get { return _spi.ScheduleHandleCount; }
        }

        private void PrintHandle(StringWriter message, ScheduleHandle handle)
        {
            if (handle is EPStatementHandleCallback)
            {
                var callback = (EPStatementHandleCallback) handle;
                TypeHelper.WriteInstance(message, callback.ScheduleCallback, true);
            }
            else {
                TypeHelper.WriteInstance(message, handle, true);
            }
        }
    
        private void ModifyCreateProxy(ScheduleHandle handle)
        {
            if (!(handle is EPStatementHandleCallback))
            {
                return;
            }
            var callback = (EPStatementHandleCallback) handle;
            var sc = (ScheduleHandleCallback) ScheduleHandleCallbackProxy.NewInstance(_engineUri, _statementName, callback.ScheduleCallback);
            callback.ScheduleCallback = sc;
        }
    }
}
