///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.timer;

namespace com.espertech.esper.schedule
{
    /// <summary>
    /// Implements the schedule service by simply keeping a sorted set of long
    /// millisecond values and a set of handles for each.
    /// <para/>
    /// Synchronized since statement creation and event evaluation by multiple (event
    /// send) threads can lead to callbacks added/removed asynchronously.
    /// </summary>
    public sealed class SchedulingServiceImpl : SchedulingServiceSPI
    {
        private readonly ILockable _uLock;

        // Map of time and handle
        private readonly IDictionary<long, IDictionary<long, ScheduleHandle>> _timeHandleMap;

        // Map of handle and handle list for faster removal
        private readonly IDictionary<ScheduleHandle, IDictionary<long, ScheduleHandle>> _handleSetMap;

        // Current time - used for evaluation as well as for adding new handles
        private long _currentTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeSourceService">time source provider</param>
        /// <param name="lockManager">The lock manager.</param>
        public SchedulingServiceImpl(TimeSourceService timeSourceService, ILockManager lockManager)
        {
            _uLock = lockManager.CreateLock(GetType());
            _timeHandleMap = new SortedList<long, IDictionary<long, ScheduleHandle>>();
            _handleSetMap = new Dictionary<ScheduleHandle, IDictionary<long, ScheduleHandle>>();
            // initialize time to just before now as there is a check for duplicate external time events
            _currentTime = timeSourceService.GetTimeMillis() - 1;
        }

        public SchedulingServiceImpl(TimeSourceService timeSourceService, IContainer container)
            : this(timeSourceService, container.Resolve<ILockManager>())
        {
        }

        public void Dispose()
        {
            Log.Debug("Destroying scheduling service");
            _handleSetMap.Clear();
            _timeHandleMap.Clear();
        }

        public long Time
        {
            get { return _currentTime; }
            set
            {
                using (_uLock.Acquire())
                {
                    _currentTime = value;
                }
            }
        }

        public void Add(long afterTime, ScheduleHandle handle, long slot)
        {
            using (Instrument.With(
                i => i.QScheduleAdd(_currentTime, afterTime, handle, slot),
                i => i.AScheduleAdd()))
            {
                using (_uLock.Acquire())
                {
                    if (_handleSetMap.ContainsKey(handle))
                    {
                        Remove(handle, slot);
                    }

                    long triggerOnTime = _currentTime + afterTime;

                    AddTrigger(slot, handle, triggerOnTime);
                }
            }
        }

        public void Remove(ScheduleHandle handle, long scheduleSlot)
        {
            using (Instrument.With(
                i => i.QScheduleRemove(handle, scheduleSlot),
                i => i.AScheduleRemove()))
            {
                using (_uLock.Acquire())
                {
                    var handleSet = _handleSetMap.Get(handle);
                    if (handleSet == null)
                    {
                        // If it already has been removed then that's fine;
                        // Such could be the case when 2 timers fireStatementStopped at the same time, and one stops the other
                        return;
                    }
                    handleSet.Remove(scheduleSlot);
                    _handleSetMap.Remove(handle);
                }
            }
        }

        public void Evaluate(ICollection<ScheduleHandle> handles)
        {
            using (Instrument.With(
                i => i.QScheduleEval(_currentTime),
                i => i.AScheduleEval(handles)))
            {
                using (_uLock.Acquire())
                {
                    // Get the values on or before the current time - to get those that are exactly on the
                    // current time we just add one to the current time for getting the head map
                    var current = _currentTime + 1;
                    if (_timeHandleMap.Count == 0)
                        return;

                    var headMap = _timeHandleMap.Where(keyValuePair => keyValuePair.Key < current);

                    // First determine all triggers to shoot
                    var removeKeys = new List<long>();
                    foreach (var entry in headMap)
                    {
                        var key = entry.Key;
                        var value = entry.Value;

                        removeKeys.Add(key);
                        foreach (ScheduleHandle handle in value.Values)
                        {
                            handles.Add(handle);
                        }
                    }

                    if (headMap.HasFirst())
                    {
                        // Next remove all handles
                        foreach (var entry in headMap)
                        {
                            foreach (var handle in entry.Value.Values)
                            {
                                _handleSetMap.Remove(handle);
                            }
                        }
                    }

                    // Remove all triggered msec values
                    int removeKeyCount = removeKeys.Count;
                    if (removeKeyCount != 0)
                    {
                        for (int ii = 0; ii < removeKeyCount; ii++)
                        {
                            long key = removeKeys[ii];
                            _timeHandleMap.Remove(key);
                        }
                    }
                }
            }
        }

        public ScheduleSet Take(ICollection<int> statementIds)
        {
            var list = new List<ScheduleSetEntry>();
            var currentTime = Time;
            foreach (var schedule in _timeHandleMap)
            {
                foreach (var entry in schedule.Value)
                {
                    if (statementIds.Contains(entry.Value.StatementId))
                    {
                        var relative = schedule.Key - currentTime;
                        list.Add(new ScheduleSetEntry(relative, entry.Key, entry.Value));
                    }
                }
            }

            list.ForEach(entry => Remove(entry.Handle, entry.Slot));

            return new ScheduleSet(list);
        }

        public void Apply(ScheduleSet scheduleSet)
        {
            var list = scheduleSet;
            var listCount = list.Count;

            for (int ii = 0; ii < listCount; ii++)
            {
                var entry = list[ii];
                Add(entry.Time, entry.Handle, entry.Slot);
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
                //handleSet = new NullableDictionary<long, ScheduleHandle>(
                //    new OrderedDictionary<long, ScheduleHandle>());
                handleSet = new OrderedDictionary<long, ScheduleHandle>();
                _timeHandleMap.Put(triggerTime, handleSet);
            }

            handleSet.Put(slot, handle);
            _handleSetMap.Put(handle, handleSet);
        }

        public int TimeHandleCount
        {
            get { return _timeHandleMap.Count; }
        }

        public string FurthestTimeHandleDate
        {
            get
            {
                var handle = FurthestTimeHandle;
                if (handle != null)
                {
                    return handle.Value.TimeFromMillis(null).ToString();
                }
                return null;
            }
        }

        public string NearestTimeHandleDate
        {
            get
            {
                var handle = NearestTimeHandle;
                if (handle != null)
                {
                    return handle.Value.TimeFromMillis(null).ToString();
                }
                return null;
            }
        }

        public long? FurthestTimeHandle
        {
            get
            {
                if (_timeHandleMap.IsNotEmpty())
                {
                    return _timeHandleMap.Keys.Last();
                }
                return null;
            }
        }

        public int ScheduleHandleCount
        {
            get { return _handleSetMap.Count; }
        }

        public bool IsScheduled(ScheduleHandle handle)
        {
            return _handleSetMap.ContainsKey(handle);
        }

        /// <summary>
        /// Returns the nearest time handle.
        /// </summary>
        /// <value>The nearest time handle.</value>
        public long? NearestTimeHandle
        {
            get
            {
                foreach (var entry in _timeHandleMap)
                {
                    if (!entry.Value.IsEmpty())
                    {
                        return entry.Key;
                    }
                }

                return null;
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
                    visitor.Invoke(visit);
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
