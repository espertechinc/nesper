///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.schedulesvcimpl
{
    /// <summary>
    /// Implements the schedule service by simply keeping a sorted set of long millisecond
    /// values and a set of handles for each.
    /// <para>
    /// Synchronized since statement creation and event evaluation by multiple (event send) threads
    /// can lead to callbacks added/removed asynchronously.
    /// </para>
    /// </summary>
    public class SchedulingServiceImpl : SchedulingServiceSPI
    {
        private readonly int _stageId;

        // Map of time and handle
        private readonly OrderedDictionary<long, IDictionary<long, ScheduleHandle>> _timeHandleMap;

        // Map of handle and handle list for faster removal
        private readonly IDictionary<ScheduleHandle, IDictionary<long, ScheduleHandle>> _handleSetMap;

        // Current time - used for evaluation as well as for adding new handles
        private long _currentTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stageId">stage id or -1 when not applicable</param>
        /// <param name="timeSourceService">time source provider</param>
        public SchedulingServiceImpl(int stageId, TimeSourceService timeSourceService)
        {
            _stageId = stageId;
            _timeHandleMap = new OrderedDictionary<long, IDictionary<long, ScheduleHandle>>();
            _handleSetMap = new Dictionary<ScheduleHandle, IDictionary<long, ScheduleHandle>>();
            // initialize time to just before now as there is a check for duplicate external time events
            _currentTime = timeSourceService.TimeMillis - 1;
        }

        public void Dispose()
        {
            Log.Debug("Destroying scheduling service");
            _handleSetMap.Clear();
            _timeHandleMap.Clear();
        }

        public virtual long Time
        {
            get {
                // note that this.currentTime is volatile
                return _currentTime;
            }
            set {
                lock (this)
                {
                    _currentTime = value;
                }
            }
        }

        public void Add(
            long afterTime,
            ScheduleHandle handle,
            long slot)
        {
            lock (this)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QScheduleAdd(_currentTime, afterTime, handle, slot);
                }

                if (_handleSetMap.ContainsKey(handle))
                {
                    Remove(handle, slot);
                }

                var triggerOnTime = _currentTime + afterTime;
                AddTrigger(slot, handle, triggerOnTime);

                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AScheduleAdd();
                }
            }
        }

        public void Remove(ScheduleHandle handle, long slot)
        {
            lock (this)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QScheduleRemove(handle, slot);
                }

                var handleSet = _handleSetMap.Get(handle);
                if (handleSet == null)
                {
                    // If it already has been removed then that's fine;
                    // Such could be the case when 2 timers fireStatementStopped at the same time, and one stops the other
                    return;
                }

                handleSet.Remove(slot);
                _handleSetMap.Remove(handle);

                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AScheduleRemove();
                }
            }
        }

        public void Evaluate(ICollection<ScheduleHandle> handles)
        {
            lock (this)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QScheduleEval(_currentTime);
                }

                // Get the values on or before the current time - to get those that are exactly on the
                // current time we just add one to the current time for getting the head map
                var headMap = _timeHandleMap.Head(_currentTime + 1);

                if (headMap.IsEmpty())
                {
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().AScheduleEval(handles);
                    }

                    return;
                }

                // First determine all triggers to shoot
                IList<long> removeKeys = new List<long>();
                foreach (var entry in headMap)
                {
                    var key = entry.Key;
                    var value = entry.Value;
                    removeKeys.Add(key);
                    foreach (var handle in value.Values)
                    {
                        handles.Add(handle);
                    }
                }

                // Next remove all handles
                foreach (var entry in headMap)
                {
                    foreach (var handle in entry.Value.Values)
                    {
                        _handleSetMap.Remove(handle);
                    }
                }

                // Remove all triggered msec values
                foreach (var key in removeKeys)
                {
                    _timeHandleMap.Remove(key);
                }

                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AScheduleEval(handles);
                }
            }
        }

        public void Transfer(ICollection<int> statementIds, SchedulingServiceSPI schedulingService)
        {
            var currentTime = Time;
            var targetTime = schedulingService.Time;
            
            foreach (var schedule in _timeHandleMap)
            {
                foreach (var entry in schedule.Value)
                {
                    if (statementIds.Contains(entry.Value.StatementId))
                    {
                        long relative = ScheduleTransferHelper.ComputeTransferTime(currentTime, targetTime, schedule.Key);
                        Remove(entry.Value, entry.Key);
                        schedulingService.Add(relative, entry.Value, entry.Key);
                    }
                }
            }
        }

        public void Init()
        {
            // no action required
        }

        private void AddTrigger(long slot, ScheduleHandle handle, long triggerTime)
        {
            var handleSet = _timeHandleMap.Get(triggerTime);
            if (handleSet == null)
            {
                handleSet = new SortedDictionary<long, ScheduleHandle>();
                _timeHandleMap.Put(triggerTime, handleSet);
            }
            handleSet.Put(slot, handle);
            _handleSetMap.Put(handle, handleSet);
        }

        public int TimeHandleCount
        {
            get => _timeHandleMap.Count;
        }

        public long? FurthestTimeHandle
        {
            get {
                if (!_timeHandleMap.IsEmpty())
                {
                    return _timeHandleMap.Keys.Last();
                }

                return null;
            }
        }

        public int ScheduleHandleCount
        {
            get => _handleSetMap.Count;
        }

        public bool IsScheduled(ScheduleHandle handle)
        {
            return _handleSetMap.ContainsKey(handle);
        }

        public long? NearestTimeHandle
        {
            get {
                lock (this)
                {
                    if (_timeHandleMap.IsEmpty())
                    {
                        return null;
                    }

                    foreach (var entry in _timeHandleMap)
                    {
                        if (entry.Value.IsEmpty())
                        {
                            continue;
                        }

                        return entry.Key;
                    }

                    return null;
                }
            }
        }

        public void VisitSchedules(ScheduleVisitor visitor)
        {
            var visit = new ScheduleVisit();
            foreach (var entry in _timeHandleMap)
            {
                visit.Timestamp = entry.Key;

                foreach (var inner in entry.Value)
                {
                    visit.StatementId = inner.Value.StatementId;
                    visit.AgentInstanceId = inner.Value.AgentInstanceId;
                    visitor.Visit(visit);
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(SchedulingServiceImpl));
    }
} // end of namespace